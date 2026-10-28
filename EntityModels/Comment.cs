
namespace EntityModels
{
    public class Comment
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string CreatedByUserName { get; set; } = string.Empty;

        public string CreatedByUserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int PostId { get; set; }

        public Post Post { get; set; } = null!;

        public int? ParentCommentId { get; set; }

        public Comment? ParentComment { get; set; }

        public ICollection<Comment> Replies { get; set; } = new List<Comment>();

        public ICollection<Like> Likes { get; set; } = new List<Like>();
    }
}
