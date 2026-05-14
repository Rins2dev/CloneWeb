using CookieAuthentication.Models;
using EntityDataModel.Data;
using EntityDataModel.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ViewModel;

namespace CloneWeb.Controllers
{
    [AllowAnonymous]
    public class AuthenticationController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly EntityDataContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthenticationController(IConfiguration configuration, EntityDataContext context, IPasswordHasher<User> passwordHasher)
        {
            _configuration = configuration;
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [AllowAnonymous]
        public IActionResult Login(string ReturnUrl = "")
        {
            var objLoginModel = new LoginModel();
            objLoginModel.ReturnUrl = ReturnUrl;
            return View(objLoginModel);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginModel objLoginModel)
        {
            if (ModelState.IsValid)
            {
                var user = _context.User.Where(x => x.UserName == objLoginModel.UserName).FirstOrDefault();
                if (user != null)
                {
                    // Support transitional login: verify against legacy MD5 hash (32-char uppercase hex)
                    // or current PasswordHasher hash, then rehash if needed
                    bool passwordValid = false;
                    if (user.Password != null && user.Password.Length <= 40 && IsHexString(user.Password))
                    {
                        // Legacy MD5 — verify then immediately rehash
                        var legacyHash = GetLegacyMd5(objLoginModel.Password);
                        if (user.Password.Equals(legacyHash, StringComparison.OrdinalIgnoreCase))
                        {
                            passwordValid = true;
                            user.Password = _passwordHasher.HashPassword(user, objLoginModel.Password);
                            _context.SaveChanges();
                        }
                    }
                    else
                    {
                        var result = _passwordHasher.VerifyHashedPassword(user, user.Password, objLoginModel.Password);
                        passwordValid = result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
                    }

                    if (passwordValid)
                    {
                        if (string.IsNullOrEmpty(objLoginModel.ReturnUrl))
                            objLoginModel.ReturnUrl = "/";

                        var claims = new List<Claim>() {
                            new Claim("UserId", user.UserId.ToString()),
                            new Claim(ClaimTypes.Name, user.UserName),
                            new Claim(ClaimTypes.Role, user.Role),
                        };
                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties()
                        {
                            IsPersistent = objLoginModel.RememberMe
                        });
                        return LocalRedirect(objLoginModel.ReturnUrl);
                    }
                }

                ViewBag.Message = "Wrong username or password";
                return View(objLoginModel);
            }
            return View(objLoginModel);
        }

        [AllowAnonymous]
        public IActionResult Register(string ReturnUrl = "")
        {
            var model = new RegisterModel();
            model.ReturnUrl = ReturnUrl;
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (_context.User.Any(x => x.UserName == model.UserName))
            {
                ModelState.AddModelError("UserName", "Username already taken.");
                return View(model);
            }

            var user = new User
            {
                UserId = Guid.NewGuid(),
                UserName = model.UserName,
                Email = model.Email,
                Role = "User",
            };
            user.Password = _passwordHasher.HashPassword(user, model.Password);
            _context.User.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return LocalRedirect("/");
        }

        private static bool IsHexString(string s)
        {
            foreach (char c in s)
            {
                if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f')))
                    return false;
            }
            return true;
        }

        private static string GetLegacyMd5(string str)
        {
            using var enc = System.Text.Encoding.Unicode;
            var bytes = enc.GetBytes(str);
            using var md5 = System.Security.Cryptography.MD5.Create();
            var hash = md5.ComputeHash(bytes);
            var sb = new System.Text.StringBuilder();
            foreach (var b in hash)
                sb.Append(b.ToString("X2"));
            return sb.ToString();
        }
    }
}
