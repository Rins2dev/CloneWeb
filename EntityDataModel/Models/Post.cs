using System;
using System.Collections.Generic;

namespace EntityDataModel.Models
{
    public class Post
    {
        public Post()
        {
            PostComment = new HashSet<PostComment>();
            PostTag = new HashSet<PostTag>();
        }

        public Guid PostId { get; set; }
        public Guid CategoryId { get; set; }
        public string Title { get; set; }
        public Guid? CreateBy { get; set; }
        public DateTime? CreateTime { get; set; }
        public Guid? LastEditBy { get; set; }
        public DateTime? LastEditTime { get; set; }
        public string PostInfomation { get; set; }
        public string PostImageUrl { get; set; }
        public string Url { get; set; }

        public virtual ICollection<PostComment> PostComment { get; set; }
        public virtual ICollection<PostTag> PostTag { get; set; }
    }
}
