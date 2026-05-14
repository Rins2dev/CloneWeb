using EntityDataModel.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ViewModel.ResultViewModel;

namespace CloneWeb.Views.ViewComponents
{
    public class ListCommentsViewComponent : Microsoft.AspNetCore.Mvc.ViewComponent
    {
        private readonly EntityDataContext _context;

        public ListCommentsViewComponent(EntityDataContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid? PostId)
        {
            var result = await GetListComment(PostId);
            return View(result);
        }

        private async Task<List<CommentResultViewModel>> GetListComment(Guid? PostId)
        {
            return await (from pc in _context.PostComment
                          join c in _context.Comments on pc.CommentId equals c.CommentId
                          join u in _context.User on c.CreateBy equals u.UserId
                          where pc.PostId == PostId
                          select new CommentResultViewModel
                          {
                              UserName = u.UserName,
                              CreateTime = c.CreateTime,
                              CommentMessage = c.CommentMessage,
                              AvatarUrl = u.ImageUrl,
                          })
                          .OrderByDescending(x => x.CreateTime)
                          .ToListAsync();
        }
    }
}
