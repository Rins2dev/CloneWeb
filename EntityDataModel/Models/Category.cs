using System;

namespace EntityDataModel.Models
{
    public class Category
    {
        public Guid CategoryId { get; set; }
        public Guid? ParentId { get; set; }
        public string Title { get; set; }
    }
}
