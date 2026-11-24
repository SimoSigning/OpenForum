namespace EntityModels
{
    public class ChatRoom
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public ChatRoomType RoomType { get; set; }

        public string CreatedByUserId { get; set; } = null!;

        public string? OwnerUserId { get; set; }

        public int? TopicId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ChatRoomMember> Members { get; set; } = new List<ChatRoomMember>();

        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

        public Topic? Topic { get; set; }
    }
}