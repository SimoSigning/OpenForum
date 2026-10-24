using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels.Comment
{
    public class CommentIndexView
    {
        public int Id { get; set; }

        public int TopicId { get; set; }

        public string Content { get; set; } = string.Empty;

        public string Creator { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public IEnumerable<CommentListItemView> Comments { get; set; } = Enumerable.Empty<CommentListItemView>();

        public CommentCreateView CreateComment { get; set; } = new();

        public CommentEditView? EditComment { get; set; } = null;

        public int CurrentPage { get; set; } = 1;

        public int TotalPages { get; set; } = 1;

    }
}
