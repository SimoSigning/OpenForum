using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ViewModels.Comment
{
    public class CommentEditView
    {
        public int Id { get; set; }

        public int PostId { get; set; }

        public int? ParentCommentId { get; set; }

        [StringLength(500)]
        public string? Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;
    }
}
