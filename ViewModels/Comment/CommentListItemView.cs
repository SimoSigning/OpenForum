
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

        // Reply relationship
        public int? ParentCommentId { get; set; }

        public string? ParentTitle { get; set; }

        public string? ParentCreator { get; set; }

        // Replies to this comment, including replies to replies.
        public List<CommentListItemView> Replies { get; set; } = new();
    }
}
