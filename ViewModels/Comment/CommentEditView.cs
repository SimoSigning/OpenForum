using System.ComponentModel.DataAnnotations;

namespace ViewModels.Comment
{
    public class CommentEditView
    {
        public int Id { get; set; }

        public int PostId { get; set; }

        [StringLength(500)]
        public string? Title { get; set; }

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;
    }
}