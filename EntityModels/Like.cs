using System;
using System.Collections.Generic;
using System.Text;

namespace EntityModels
{
    public class Like
    {
        public long Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? PostId { get; set; }

        public Post? Post { get; set; }

        public int? CommentId { get; set; }

        public Comment? Comment { get; set; }
    }
}