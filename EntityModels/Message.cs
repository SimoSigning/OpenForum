
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

        // Sender has deleted their copy.
        public bool IsDeletedBySender { get; set; } = false;

        // Recipient has deleted their copy.
        public bool IsDeletedByRecipient { get; set; } = false;

        // Records when the sender deleted the message.
        public DateTime? DeletedBySenderAt { get; set; }
    }
}
