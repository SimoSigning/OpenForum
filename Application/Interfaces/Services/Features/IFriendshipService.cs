using EntityModels;
using System;
using System.Collections.Generic;
using System.Text;
using ViewModels.Friends;

namespace Application.Interfaces.Services.Features
{
    public interface IFriendshipService
    {
        Task SendRequestAsync(string addresseeUserId);

        Task AcceptRequestAsync(int friendshipId);

        Task DeclineRequestAsync(int friendshipId);

        Task RemoveFriendAsync(int friendshipId);

        Task<IEnumerable<Friendship>> GetFriendsAsync();

        Task<IEnumerable<Friendship>> GetIncomingRequestsAsync();

        Task<UserSearchView> SearchUsersAsync(string query, int page = 1);

        Task<FriendsIndexView> GetFriendsIndexViewAsync();
    }
}
