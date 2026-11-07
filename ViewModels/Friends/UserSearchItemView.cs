
namespace ViewModels.Friends
{
    public class UserSearchItemView
    {
        public string UserId { get; set; } = string.Empty;

        public int FriendshipId { get; set; }

        public string Username { get; set; } = string.Empty;

        public bool CanSendRequest { get; set; }

        public string? RelationshipStatus { get; set; }
    }
}
