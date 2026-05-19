using EntityDataModel.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ViewModel.Create
{
    public class PostViewModel : Post
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters.")]
        public new string Title { get; set; }

        [Required(ErrorMessage = "Content is required.")]
        [StringLength(50000, ErrorMessage = "Content cannot exceed 50000 characters.")]
        public new string PostInfomation { get; set; }

        public List<Guid?> TagId { get; set; }
    }
}
