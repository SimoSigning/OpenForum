using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ViewModels.Messages
{
    public class ComposeMessageView
    {
        public string RecipientUserId { get; set; } = string.Empty;

        [Required]
        public string RecipientUserName { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [StringLength(5000)]
        public string Body { get; set; } = string.Empty;

        public bool IsReply { get; set; }

        public int? OriginalMessageId { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
