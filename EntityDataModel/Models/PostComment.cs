﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace EntityDataModel.Models
{
    public partial class PostComment
    {
        public Guid PostId { get; set; }
        public Guid CommentId { get; set; }

        public virtual Comments Comment { get; set; }
        public virtual Post Post { get; set; }
    }
}