using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ViewModels.Post
{
    public class PostEditView
    {
        public int Id { get; set; }

        public int TopicId { get; set; }

        [Required]
        [StringLength(5000)]
        public string Content { get; set; } = string.Empty;
    }
}
