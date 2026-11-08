using Application.Interfaces.Services.Features;
using Application.Interfaces.Services.User;
using Application.Models;
using EntityModels;
using Repository.interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using ViewModels.Friends;

namespace Application.Services.Features
{
    public class FriendshipService : IFriendshipService
    {
        private readonly IFriendshipRepository friendshipRepository;
        private readonly IUserService userService;
        private readonly ICurrentUserService currentUserService;

        private const int PageSize = 10;

        public FriendshipService(
            IFriendshipRepository friendshipRepository,
            IUserService userService,
            ICurrentUserService currentUserService)
        {
            this.friendshipRepository = friendshipRepository;
            this.userService = userService;
            this.currentUserService = currentUserService;
        }

        public async Task SendRequestAsync(string addresseeUserId)
        {
            string requesterUserId = currentUserService.GetRequiredUserId();

            if (string.IsNullOrWhiteSpace(addresseeUserId))
                throw new ArgumentException("A recipient user ID is required.");

            if (requesterUserId == addresseeUserId)
                throw new InvalidOperationException("You cannot send a friend request to yourself.");

            bool recipientExists = await userService.UserExistsAsync(addresseeUserId);

            if (!recipientExists)
                throw new KeyNotFoundException("The requested user does not exist.");

            Friendship? existingFriendship = await friendshipRepository.GetBetweenUsersAsync(requesterUserId, addresseeUserId);

            if (existingFriendship != null)
                throw new InvalidOperationException("A friendship or friend request already exists between these users.");

            // Normalize the pair so the unique database index works
            // regardless of which user sends the request.
            string userIdLow;
            string userIdHigh;

            if (string.Compare(requesterUserId, addresseeUserId, StringComparison.Ordinal) < 0)
            {
                userIdLow = requesterUserId;
                userIdHigh = addresseeUserId;
            }
            else
            {
                userIdLow = addresseeUserId;
                userIdHigh = requesterUserId;
            }

            Friendship friendship = new Friendship
            {
                RequesterUserId = requesterUserId,
                AddresseeUserId = addresseeUserId,
                UserIdLow = userIdLow,
                UserIdHigh = userIdHigh,
                Status = FriendshipStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await friendshipRepository.AddAsync(friendship);
        }

        public async Task AcceptRequestAsync(int friendshipId)
        {
            string currentUserId = currentUserService.GetRequiredUserId();

            Friendship friendship = await GetFriendshipOrThrowAsync(friendshipId);

            if (friendship.AddresseeUserId != currentUserId)
                throw new UnauthorizedAccessException("Only the recipient can accept this friend request.");

            if (friendship.Status != FriendshipStatus.Pending)
                throw new InvalidOperationException("This friend request is no longer pending.");

            friendship.Status = FriendshipStatus.Accepted;

            await friendshipRepository.UpdateAsync(friendship);
        }

        public async Task DeclineRequestAsync(int friendshipId)
        {
            string currentUserId = currentUserService.GetRequiredUserId();

            Friendship friendship = await GetFriendshipOrThrowAsync(friendshipId);

            if (friendship.AddresseeUserId != currentUserId)
                throw new UnauthorizedAccessException("Only the recipient can decline this friend request.");

            if (friendship.Status != FriendshipStatus.Pending)
                throw new InvalidOperationException("This friend request is no longer pending.");

            await friendshipRepository.DeleteAsync(friendship.Id);
        }

        public async Task RemoveFriendAsync(int friendshipId)
        {
            string currentUserId = currentUserService.GetRequiredUserId();

            Friendship friendship = await GetFriendshipOrThrowAsync(friendshipId);

            bool isParticipant = friendship.RequesterUserId == currentUserId || friendship.AddresseeUserId == currentUserId;

            if (!isParticipant)
                throw new UnauthorizedAccessException("You are not part of this friendship.");

            if (friendship.Status != FriendshipStatus.Accepted)
                throw new InvalidOperationException("Only accepted friendships can be removed here.");

            await friendshipRepository.DeleteAsync(friendship.Id);
        }

        public async Task<IEnumerable<Friendship>> GetFriendsAsync()
        {
            string currentUserId = currentUserService.GetRequiredUserId();

            return await friendshipRepository.GetAcceptedForUserAsync(currentUserId);
        }

        public async Task<IEnumerable<Friendship>> GetIncomingRequestsAsync()
        {
            string currentUserId = currentUserService.GetRequiredUserId();

            return await friendshipRepository.GetIncomingPendingRequestsAsync(currentUserId);
        }

        private async Task<Friendship> GetFriendshipOrThrowAsync(int id)
        {
            Friendship? friendship = await friendshipRepository.GetByIdAsync(id);

            if (friendship == null)
                throw new KeyNotFoundException("Friendship or friend request not found.");

            return friendship;
        }

        public async Task<UserSearchView> SearchUsersAsync(string query,int page = 1)
        {
            page = Math.Max(1, page);

            string currentUserId = currentUserService.GetRequiredUserId();

            query = query?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(query))
            {
                return new UserSearchView
                {
                    SearchQuery = query,
                    Users = Enumerable.Empty<UserSearchItemView>(),
                    CurrentPage = 1,
                    TotalPages = 1
                };
            }

            int skip = (page - 1) * PageSize;

            (IEnumerable<UserSearchResult> Items, int TotalCount) searchResult = await userService.SearchUsersByUsernameAsync(query, currentUserId, skip, PageSize);

            int totalPages = Math.Max(1, (int)Math.Ceiling(searchResult.TotalCount / (double)PageSize));

            // Correct the page if it is beyond the last available page.
            if (page > totalPages)
            {
                page = totalPages;
                skip = (page - 1) * PageSize;
                searchResult = await userService.SearchUsersByUsernameAsync(query, currentUserId, skip, PageSize);
            }

            IEnumerable<Friendship> relationships = await friendshipRepository.GetAllForUserAsync(currentUserId);

            Dictionary<string, Friendship> relationshipLookup = relationships.ToDictionary(f => f.RequesterUserId == currentUserId ? f.AddresseeUserId : f.RequesterUserId);

            List<UserSearchItemView> users = searchResult.Items.Select(user =>
                {
                    relationshipLookup.TryGetValue(user.UserId, out Friendship? relationship);

                    string? status = null;
                    bool canSendRequest = relationship == null;

                    if (relationship != null)
                    {
                        if (relationship.Status == FriendshipStatus.Accepted)
                        {
                            status = "Friends";
                        }
                        else if (relationship.RequesterUserId == currentUserId)
                        {
                            status = "Request sent";
                        }
                        else
                        {
                            status = "Request received";
                        }
                    }

                    return new UserSearchItemView
                    {
                        UserId = user.UserId,
                        Username = user.Username,
                        CanSendRequest = canSendRequest,
                        RelationshipStatus = status
                    };
                })
                .ToList();

            return new UserSearchView
            {
                SearchQuery = query,
                Users = users,
                CurrentPage = page,
                TotalPages = totalPages
            };
        }

        public async Task<FriendsIndexView> GetFriendsIndexViewAsync()
        {
            string currentUserId = currentUserService.GetRequiredUserId();

            // Retrieve accepted friendships and incoming requests.
            List<Friendship> friendships = (await GetFriendsAsync()).ToList();

            List<Friendship> incomingRequests = (await GetIncomingRequestsAsync()).ToList();

            // Find the IDs of all other users.
            List<string> userIds = friendships
                .Select(f => f.RequesterUserId == currentUserId ? f.AddresseeUserId : f.RequesterUserId)
                .Concat(incomingRequests.Select(f => f.RequesterUserId))
                .Distinct()
                .ToList();

            // Retrieve all usernames in one call.
            Dictionary<string, string> usernames = await userService.GetUsernamesByIdsAsync(userIds);

            // Map accepted friendships.
            List<UserSearchItemView> friends = friendships
                .Select(f =>
                {
                    string friendUserId = f.RequesterUserId == currentUserId ? f.AddresseeUserId : f.RequesterUserId;

                    usernames.TryGetValue(friendUserId, out string? username);

                    return new UserSearchItemView
                    {
                        UserId = friendUserId,
                        FriendshipId = f.Id,
                        Username = username ?? "Unknown user",
                        CanSendRequest = false,
                        RelationshipStatus = "Friends"
                    };
                })
                .ToList();

            // Map incoming requests.
            List<FriendRequestItemView> requests = incomingRequests
                .Select(f =>
                {
                    usernames.TryGetValue(f.RequesterUserId, out string? username);

                    return new FriendRequestItemView
                    {
                        FriendshipId = f.Id,
                        UserId = f.RequesterUserId,
                        Username = username ?? "Unknown user",
                        CreatedAt = f.CreatedAt
                    };
                })
                .ToList();

            return new FriendsIndexView
            {
                Friends = friends,
                IncomingRequests = requests
            };
        }

        public async Task<UserRelationshipView> GetRelationshipAsync(string targetUserId)
        {
            string currentUserId = currentUserService.GetRequiredUserId();

            if (string.IsNullOrWhiteSpace(targetUserId))
                throw new ArgumentException("A target user ID is required.");

            // A user cannot send a friend request to themselves.
            if (currentUserId == targetUserId)
            {
                return new UserRelationshipView
                {
                    CanSendRequest = false
                };
            }

            Friendship? relationship = await friendshipRepository.GetBetweenUsersAsync(currentUserId,targetUserId);

            if (relationship == null)
            {
                return new UserRelationshipView
                {
                    CanSendRequest = true
                };
            }

            return new UserRelationshipView
            {
                FriendshipId = relationship.Id,
                CanSendRequest = false,
                RelationshipStatus = relationship.Status switch
                {
                    FriendshipStatus.Accepted => "Friends",
                    FriendshipStatus.Pending when relationship.RequesterUserId == currentUserId => "Request sent",
                    FriendshipStatus.Pending => "Request received",
                    _ => null
                }
            };
        }
    }
}
