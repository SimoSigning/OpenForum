using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ViewModels.Topic
{
    public class TopicCreateView
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;
    }
}
