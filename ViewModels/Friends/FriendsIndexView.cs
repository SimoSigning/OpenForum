
namespace ViewModels.Friends
{
    public class FriendsIndexView
    {
        public IEnumerable<UserSearchItemView> Friends { get; set; } = Enumerable.Empty<UserSearchItemView>();

        public IEnumerable<FriendRequestItemView> IncomingRequests { get; set; } = Enumerable.Empty<FriendRequestItemView>();
    }
}
