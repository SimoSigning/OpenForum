using System;
using System.Collections.Generic;
using System.Text;

namespace EntityModels
{
    public class Post
    {
        public int Id { get; set; }

        public string Content { get; set; } = string.Empty;

        public string CreatedByUserName { get; set; } = string.Empty;

        public string CreatedByUserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int TopicId { get; set; }

        public Topic Topic { get; set; } = null!;

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
