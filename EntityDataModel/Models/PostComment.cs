using System;

namespace EntityDataModel.Models
{
    public class PostComment
    {
        public Guid PostId { get; set; }
        public Guid CommentId { get; set; }

        public virtual Comments Comment { get; set; }
        public virtual Post Post { get; set; }
    }
}
