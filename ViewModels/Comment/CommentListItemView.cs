using System;
using System.Collections.Generic;

namespace ViewModels.Comment
{
    public class CommentListItemView
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public string Content { get; set; } = string.Empty;

        public string Creator { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public bool CanManage { get; set; }

        public bool CanReply { get; set; }

        public List<CommentListItemView> Replies { get; set; } = new();
    }
}