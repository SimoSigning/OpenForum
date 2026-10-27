using System.ComponentModel.DataAnnotations;

namespace ViewModels.Comment
{
    public class CommentCreateView
    {
        public int PostId { get; set; }

        // Null for a top-level comment; set for a reply.
        public int? ParentCommentId { get; set; }

        [StringLength(500)]
        public string? Title { get; set; }

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;
    }
}