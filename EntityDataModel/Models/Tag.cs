using System;
using System.Collections.Generic;

namespace EntityDataModel.Models
{
    public class Tag
    {
        public Tag()
        {
            PostTag = new HashSet<PostTag>();
        }

        public Guid TagId { get; set; }
        public string Title { get; set; }

        public virtual ICollection<PostTag> PostTag { get; set; }
    }
}
