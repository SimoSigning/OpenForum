using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels.Messages
{
    public class MessageDetailsView
    {
        public int Id { get; set; }

        public string SenderUserId { get; set; } = string.Empty;

        public string SenderUserName { get; set; } = string.Empty;

        public string RecipientUserId { get; set; } = string.Empty;

        public string RecipientUserName { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public DateTime SentAt { get; set; }

        public bool IsRead { get; set; }

        public bool CanReply { get; set; }
    }
}
