namespace EntityModels
{
    public class ChatMessage
    {
        public long Id { get; set; }

        public int ChatRoomId { get; set; }

        public string SenderUserId { get; set; } = null!;

        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public ChatRoom ChatRoom { get; set; } = null!;
    }
}