using System;
using System.Collections.Generic;

namespace EntityDataModel.Models
{
    public class Comments
    {
        public Comments()
        {
            PostComment = new HashSet<PostComment>();
        }

        public Guid CommentId { get; set; }
        public DateTime? CreateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public DateTime? LastEditTime { get; set; }
        public string CommentMessage { get; set; }

        public virtual User CreateByNavigation { get; set; }
        public virtual ICollection<PostComment> PostComment { get; set; }
    }
}
