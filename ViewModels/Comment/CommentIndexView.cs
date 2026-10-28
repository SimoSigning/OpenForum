using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels.Comment
{
    public class CommentIndexView
    {
        public int PostId { get; set; }

        public int TopicId { get; set; }

        public string PostTitle { get; set; } = string.Empty;

        public string PostContent { get; set; } = string.Empty;

        public string PostCreator { get; set; } = string.Empty;

        public DateTime PostCreatedAt { get; set; }

        public IEnumerable<CommentListItemView> Comments { get; set; } = Enumerable.Empty<CommentListItemView>();

        public CommentCreateView CreateComment { get; set; } = new();

        public CommentEditView? EditComment { get; set; } = null;

        public int CurrentPage { get; set; } = 1;

        public int TotalPages { get; set; } = 1;

        public bool CanCreate { get; set; }

        public int PostLikeCount { get; set; }

        public bool IsPostLikedByCurrentUser { get; set; }

        public bool CanLikePost { get; set; }
    }
}
