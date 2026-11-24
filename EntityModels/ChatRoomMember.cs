namespace EntityModels
{
    public class ChatRoomMember
    {
        public int Id { get; set; }

        public int ChatRoomId { get; set; }

        public string UserId { get; set; } = null!;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastVisitedAt { get; set; }

        public ChatRoom ChatRoom { get; set; } = null!;
    }
}