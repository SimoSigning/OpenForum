
using System.ComponentModel.DataAnnotations;

namespace ViewModels.Comment
{
    public class CommentCreateView
    {
        public int PostId { get; set; }

        public int? ParentCommentId { get; set; }

        // Required for top-level comments; omitted for replies.
        [StringLength(500)]
        public string? Title { get; set; }

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;
    }
}
