
namespace ViewModels.Friends
{
    public class FriendRequestItemView
    {
        public int FriendshipId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
