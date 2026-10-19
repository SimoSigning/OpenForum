using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels.Comment
{
    public class CommentListItemView
    {
        public int Id { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
