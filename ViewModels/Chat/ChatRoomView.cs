using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels.Chat
{
    public class ChatRoomView
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string RoomType { get; set; } = string.Empty;

        public string? OwnerUserId { get; set; }

        public string CurrentUserId { get; set; } = string.Empty;

        public bool IsOwner { get; set; }

        public bool IsMember { get; set; }

        public int MemberCount { get; set; }

        public int? TopicId { get; set; }
    }
}
