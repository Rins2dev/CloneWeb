using EntityDataModel.Data;
using EntityDataModel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ViewModel;
using ViewModel.Create;
using ViewModel.ResultViewModel;
using System.Collections.Generic;

namespace CloneWeb.Controllers
{
    public class PostController : Controller
    {
        private EntityDataContext _context;
        private IConfiguration _configuration;

        public PostController(EntityDataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult CreatePost()
        {
            var lstCategory = _context.Category.ToList();
            ViewBag.CategoryId = new SelectList(lstCategory, "CategoryId", "Title");
            var lstTag = _context.Tag.ToList();
            ViewBag.TagId = new SelectList(lstTag, "TagId", "Title");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult CreatePost(PostViewModel Model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Not a valid model");

            if (!_context.Category.Any(c => c.CategoryId == Model.CategoryId))
                return BadRequest("Invalid category.");

            var identity = User?.Identities.FirstOrDefault();
            if (identity == null) return Unauthorized();
            var claims = identity.Claims.ToList();
            var userIdClaim = claims.FirstOrDefault(x => x.Type == "UserId");
            if (userIdClaim == null) return Unauthorized();
            Model.PostId = Guid.NewGuid();
            Model.CreateTime = DateTime.Now;
            Model.CreateBy = Guid.Parse(userIdClaim.Value);
            Model.Url = ToUrlSlug(Model.Title);

            if (Model.TagId != null && Model.TagId.Count > 0)
            {
                var validTagIds = _context.Tag.Select(t => (Guid?)t.TagId).ToHashSet();
                foreach (var tagId in Model.TagId)
                {
                    if (tagId == null || !validTagIds.Contains(tagId))
                        return BadRequest("One or more tags are invalid.");
                    _context.Add(new PostTag { PostId = Model.PostId, TagId = (Guid)tagId });
                }
            }

            _context.Post.Add(Model);
            _context.SaveChanges();
            return RedirectToAction("ListPost");
        }

        public async Task<IActionResult> ListPost(int page = 1, int pageSize = 10)
        {
            var total = await _context.Post.CountAsync();
            var lstPost = await _context.Post
                .OrderByDescending(x => x.CreateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Paging = new PagingViewModel
            {
                PageNumber = page,
                PageSize = pageSize,
                TotalRecords = total
            };
            return View(lstPost);
        }

        [Route("Post/ViewPost/{PostId?}")]
        public async Task<IActionResult> ViewPost(Guid? PostId)
        {
            var post = await (from db in _context.Post.Include(x => x.PostTag)
                              join cate in _context.Category on db.CategoryId equals cate.CategoryId
                              join creator in _context.User on db.CreateBy equals creator.UserId into creatorGroup
                              from creator in creatorGroup.DefaultIfEmpty()
                              join editor in _context.User on db.LastEditBy equals editor.UserId into editorGroup
                              from editor in editorGroup.DefaultIfEmpty()
                              where db.PostId == PostId
                              select new PostResultViewModel
                              {
                                  PostId = db.PostId,
                                  Title = db.Title,
                                  CreateBy = db.CreateBy,
                                  CreateByName = creator != null ? creator.UserName : null,
                                  CreateTime = db.CreateTime,
                                  LastEditBy = db.LastEditBy,
                                  LastEditByName = editor != null ? editor.UserName : null,
                                  LastEditTime = db.LastEditTime,
                                  CategoryName = cate.Title,
                                  PostTag = db.PostTag.ToList(),
                                  PostInfomation = db.PostInfomation,
                                  Url = db.Url,
                                  TotalComments = _context.PostComment.Count(pc => pc.PostId == db.PostId),
                                  Tags = (from pt in db.PostTag
                                          join tag in _context.Tag on pt.TagId equals tag.TagId
                                          select tag).ToList(),
                              }).FirstOrDefaultAsync();

            if (post == null) return NotFound();
            ViewBag.PostId = post.PostId;
            ViewBag.PostInfomation = post.PostInfomation;
            return View(post);
        }

        public async Task<IActionResult> SearchPost(string KeyWord)
        {
            ViewBag.DomainUrl = _configuration["DomainUrl"];
            KeyWord = KeyWord?.Trim() ?? string.Empty;

            var post = await (from db in _context.Post.Include(x => x.PostComment).Include(x => x.PostTag)
                              join c in _context.Category on db.CategoryId equals c.CategoryId
                              join creator in _context.User on db.CreateBy equals creator.UserId into creatorGroup
                              from creator in creatorGroup.DefaultIfEmpty()
                              join editor in _context.User on db.LastEditBy equals editor.UserId into editorGroup
                              from editor in editorGroup.DefaultIfEmpty()
                              where db.Title.Contains(KeyWord) || c.Title.Contains(KeyWord)
                              select new PostResultViewModel
                              {
                                  PostId = db.PostId,
                                  Title = db.Title,
                                  CreateBy = db.CreateBy,
                                  CreateByName = creator != null ? creator.UserName : null,
                                  CreateTime = db.CreateTime,
                                  LastEditBy = db.LastEditBy,
                                  LastEditByName = editor != null ? editor.UserName : null,
                                  LastEditTime = db.LastEditTime,
                                  CategoryName = c.Title,
                                  PostTag = db.PostTag.ToList(),
                                  PostInfomation = db.PostInfomation,
                                  TotalComments = db.PostComment.Count(),
                                  Tags = (from pt in db.PostTag
                                          join tag in _context.Tag on pt.TagId equals tag.TagId
                                          select tag).ToList(),
                              }).ToListAsync();

            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult AddComment(Guid PostId, string Comment)
        {
            if (string.IsNullOrWhiteSpace(Comment))
                return Json(new Response { isSuccess = false, code = 400, message = "Comment cannot be empty." });

            if (Comment.Length > 2000)
                return Json(new Response { isSuccess = false, code = 400, message = "Comment is too long (max 2000 characters)." });

            var identity = User?.Identities.FirstOrDefault();
            if (identity == null) return Unauthorized();
            var userIdClaim = identity.Claims.FirstOrDefault(x => x.Type == "UserId");
            if (userIdClaim == null) return Unauthorized();

            var comments = new Comments
            {
                CommentId = Guid.NewGuid(),
                CreateTime = DateTime.Now,
                CommentMessage = Comment,
                CreateBy = Guid.Parse(userIdClaim.Value)
            };

            var postComment = new PostComment
            {
                PostId = PostId,
                CommentId = comments.CommentId
            };

            _context.Comments.Add(comments);
            _context.PostComment.Add(postComment);
            _context.SaveChanges();
            return Json(new Response { isSuccess = true, code = 200 });
        }

        public IActionResult ReloadComment(Guid? PostId)
        {
            return ViewComponent("ListComments", new { PostId = PostId });
        }

        public IActionResult ReloadRecentComment(Guid? PostId)
        {
            return ViewComponent("ListRecentComment");
        }

        public static string ToUrlSlug(string value)
        {
            value = value.ToLowerInvariant();
            var bytes = Encoding.GetEncoding("Cyrillic").GetBytes(value);
            value = Encoding.ASCII.GetString(bytes);
            value = Regex.Replace(value, @"\s", "-", RegexOptions.Compiled);
            value = Regex.Replace(value, @"[^a-z0-9\s-_]", "", RegexOptions.Compiled);
            value = value.Trim('-', '_');
            value = Regex.Replace(value, @"([-_]){2,}", "$1", RegexOptions.Compiled);
            return value;
        }
    }
}
