using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels.Chat
{
    public class ChatMessageView
    {
        public long Id { get; set; }

        public int ChatRoomId { get; set; }

        public string SenderUserId { get; set; } = string.Empty;

        public string SenderUsername { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; }

        public bool IsOwnMessage { get; set; }
    }
}
