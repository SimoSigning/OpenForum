
namespace ViewModels.Comment
{
    public class CommentListItemView
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string Creator { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public bool CanManage { get; set; }

        public int? ParentCommentId { get; set; }

        public string? ParentTitle { get; set; }

        public string? ParentCreator { get; set; }

        public List<CommentListItemView> Replies { get; set; } = new();

        public int LikeCount { get; set; }

        public bool IsLikedByCurrentUser { get; set; }

        public bool CanLike { get; set; }
    }
}
