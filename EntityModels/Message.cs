using System;
using System.Collections.Generic;
using System.Text;

namespace EntityModels
{
    public class Message
    {
        public int Id { get; set; }

        // User who sent the message.
        public string SenderUserId { get; set; } = null!;

        // User who receives the message.
        public string RecipientUserId { get; set; } = null!;

        public string Subject { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // False until the recipient opens the message.
        public bool IsRead { get; set; } = false;
    }
}
