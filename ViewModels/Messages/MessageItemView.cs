using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels.Messages
{
    public class MessageItemView
    {
        public int Id { get; set; }

        public string OtherUserId { get; set; } = string.Empty;

        public string OtherUserName { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string BodyPreview { get; set; } = string.Empty;

        public DateTime SentAt { get; set; }

        public bool IsRead { get; set; }
    }
}
