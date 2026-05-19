using System;
using System.Collections.Generic;

namespace EntityDataModel.Models
{
    public class User
    {
        public User()
        {
            Comments = new HashSet<Comments>();
        }

        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string ImageUrl { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }

        public virtual ICollection<Comments> Comments { get; set; }
    }
}
