using CloneWeb.Middleware;
using CloneWeb.Views.ViewComponents;
using EntityDataModel.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Identity;
using System.Data.Entity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EntityDataModel.Models;

namespace CloneWeb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews()
                .AddRazorOptions(options =>
                {
                    options.ViewLocationFormats.Add("/{0}.cshtml");
                });
            var connectionString = Configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<EntityDataContext>(options => options.UseSqlServer(connectionString));

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                       .AddCookie(options =>
                       {
                           options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                           options.SlidingExpiration = true;
                           options.AccessDeniedPath = "/Forbidden/";
                           options.LoginPath = "/Authentication/Login";
                           options.LogoutPath = new PathString("/Authentication/Logout");
                       });
            //services.AddAuthentication(x =>
            //{
            //    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //}).AddJwtBearer(o =>
            //{
            //    var Key = Encoding.UTF8.GetBytes(Configuration["JWT:Key"]);
            //    o.SaveToken = true;
            //    o.TokenValidationParameters = new TokenValidationParameters
            //    {
            //        ValidateIssuer = false,
            //        ValidateAudience = false,
            //        ValidateLifetime = true,
            //        ValidateIssuerSigningKey = true,
            //        ValidIssuer = Configuration["JWT:Issuer"],
            //        ValidAudience = Configuration["JWT:Audience"],
            //        IssuerSigningKey = new SymmetricSecurityKey(Key),
            //        ClockSkew = TimeSpan.Zero
            //    };
            //});
            services.AddMvc();
            services.AddAuthorization();
            services.AddSignalR();
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            //app.UseStatusCodePages();
            app.UseStatusCodePagesWithReExecute("/Error/Index", "?statusCode={0}");
            //add Visitor Counter Middleware
            //app.UseMiddleware(typeof(VisitorCounterMiddleware));
            app.UseRouting();

            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                //endpoints.MapControllerRoute(
                //   name: "post",
                //   pattern: "{CreateTime}/{Url}",
                //   defaults: new
                //   {
                //       controller = "Post",
                //       action = "ViewPost",
                //       PostId = "PostId?"
                //   });

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapHub<OnlineCountHub>("/onlinecount");
            });
        }
    }
}
