using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels.Chat
{
    public class ChatMemberView
    {
        public string UserId { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public DateTime JoinedAt { get; set; }

        public bool IsOwner { get; set; }
    }
}
