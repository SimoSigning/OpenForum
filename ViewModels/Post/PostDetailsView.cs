using System;
using System.Collections.Generic;
using System.Text;
using ViewModels.Comment;

namespace ViewModels.Post
{
    public class PostDetailsView
    {
        public int Id { get; set; }

        public int TopicId { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public IEnumerable<CommentListItemView> Comments { get; set; } = Enumerable.Empty<CommentListItemView>();

        public CommentCreateView CreateComment { get; set; } = new();

        public CommentEditView? EditComment { get; set; } = null;

    }
}
