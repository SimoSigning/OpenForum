using Application.Interfaces.Services.Features;
using Application.Interfaces.Services.User;
using EntityModels;
using Repository.interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using ViewModels.Chat;

namespace Application.Services.Features
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository chatRepository;
        private readonly IFriendshipRepository friendshipRepository;
        private readonly ICurrentUserService currentUser;
        private readonly IUserService userService;

        private const int MessageLimit = 100;
        private const int MaxMessageLength = 2000;

        public ChatService(
            IChatRepository chatRepository,
            IFriendshipRepository friendshipRepository,
            ICurrentUserService currentUser,
            IUserService userService)
        {
            this.chatRepository = chatRepository;
            this.friendshipRepository = friendshipRepository;
            this.currentUser = currentUser;
            this.userService = userService;
        }

        // --------------------------------
        // Room retrieval
        // --------------------------------

        public async Task<IEnumerable<ChatRoomView>> GetMyRoomsAsync()
        {
            string userId = currentUser.GetRequiredUserId();

            IEnumerable<ChatRoom> rooms =
                await chatRepository.GetRoomsForUserAsync(userId);

            return rooms.Select(room => MapRoom(room, userId));
        }

        public async Task<IEnumerable<ChatRoomView>> GetMiscellaneousRoomsAsync()
        {
            string userId = currentUser.GetRequiredUserId();

            IEnumerable<ChatRoom> rooms =
                await chatRepository.GetMiscellaneousRoomsAsync();

            return rooms.Select(room => new ChatRoomView
            {
                Id = room.Id,
                Name = room.Name,
                RoomType = room.RoomType.ToString(),
                OwnerUserId = room.OwnerUserId,
                IsOwner = false,
                IsMember = room.Members.Any(m => m.UserId == userId),
                MemberCount = room.Members.Count,
                TopicId = room.TopicId
            });
        }

        public async Task<ChatRoomView> GetRoomAsync(int roomId)
        {
            string userId = currentUser.GetRequiredUserId();

            ChatRoom room = await GetRoomOrThrowAsync(roomId);

            if (room.RoomType == ChatRoomType.Topic)
            {
                ChatRoomMember? membership =
                    await chatRepository.GetMembershipAsync(roomId, userId);

                if (membership == null)
                {
                    await chatRepository.AddMemberAsync(new ChatRoomMember
                    {
                        ChatRoomId = roomId,
                        UserId = userId,
                        JoinedAt = DateTime.UtcNow
                    });
                }
            }
            else if (room.RoomType == ChatRoomType.Miscellaneous)
            {
                await JoinMiscellaneousRoomAsync(roomId);
            }
            else if (room.RoomType == ChatRoomType.Private)
            {
                await EnsureMembershipAsync(roomId, userId);

                await chatRepository.MarkRoomAsVisitedAsync(roomId, userId);
            }
            else
            {
                throw new InvalidOperationException("Unsupported chat room type.");
            }

            IEnumerable<ChatRoomMember> members =
                await chatRepository.GetMembersAsync(room.Id);

            return new ChatRoomView
            {
                Id = room.Id,
                Name = room.Name,
                RoomType = room.RoomType.ToString(),
                OwnerUserId = room.OwnerUserId,
                IsOwner = room.OwnerUserId == userId,
                IsMember = true,
                MemberCount = members.Count(),
                TopicId = room.TopicId,
                CurrentUserId = userId
            };
        }

        public async Task<IEnumerable<ChatMemberView>> GetRoomMembersAsync(int roomId)
        {
            string currentUserId = currentUser.GetRequiredUserId();

            ChatRoom room = await GetRoomOrThrowAsync(roomId);

            // Verify that the current user belongs to the room.
            await EnsureMembershipAsync(roomId, currentUserId);

            IEnumerable<ChatRoomMember> members =
                await chatRepository.GetMembersAsync(roomId);

            List<ChatRoomMember> memberList = members.ToList();

            Dictionary<string, string> usernames =
                await userService.GetUsernamesByIdsAsync(
                    memberList.Select(m => m.UserId));

            return memberList.Select(member =>
                new ChatMemberView
                {
                    UserId = member.UserId,
                    Username = usernames.TryGetValue(
                        member.UserId,
                        out string? username)
                            ? username
                            : "Unknown user",
                    JoinedAt = member.JoinedAt,
                    IsOwner = room.OwnerUserId == member.UserId
                });
        }

        // --------------------------------
        // Private rooms
        // --------------------------------

        public async Task<IEnumerable<ChatFriendView>> GetFriendsForChatAsync()
        {
            string currentUserId = currentUser.GetRequiredUserId();

            IEnumerable<Friendship> friendships =
                await friendshipRepository.GetAcceptedForUserAsync(currentUserId);

            List<Friendship> friendshipList = friendships.ToList();

            List<string> friendUserIds = friendshipList
                .Select(f =>
                    f.RequesterUserId == currentUserId
                        ? f.AddresseeUserId
                        : f.RequesterUserId)
                .Distinct()
                .ToList();

            Dictionary<string, string> usernames =
                await userService.GetUsernamesByIdsAsync(friendUserIds);

            return friendUserIds.Select(userId =>
                new ChatFriendView
                {
                    UserId = userId,
                    Username = usernames.TryGetValue(
                        userId,
                        out string? username)
                            ? username
                            : "Unknown user"
                });
        }

        public async Task<IEnumerable<ChatFriendView>> GetAvailableFriendsToInviteAsync(int roomId)
        {
            string currentUserId = currentUser.GetRequiredUserId();

            ChatRoom room = await GetRoomOrThrowAsync(roomId);

            if (room.RoomType != ChatRoomType.Private)
                throw new InvalidOperationException(
                    "Friends can only be invited to private rooms.");

            await EnsureMembershipAsync(roomId, currentUserId);

            IEnumerable<Friendship> friendships =
                await friendshipRepository.GetAcceptedForUserAsync(currentUserId);

            List<Friendship> friendshipList = friendships.ToList();

            List<string> friendUserIds = friendshipList
                .Select(f =>
                    f.RequesterUserId == currentUserId
                        ? f.AddresseeUserId
                        : f.RequesterUserId)
                .Distinct()
                .ToList();

            IEnumerable<ChatRoomMember> members =
                await chatRepository.GetMembersAsync(roomId);

            HashSet<string> existingMemberIds = members
                .Select(m => m.UserId)
                .ToHashSet();

            List<string> availableFriendIds = friendUserIds
                .Where(id => !existingMemberIds.Contains(id))
                .ToList();

            Dictionary<string, string> usernames =
                await userService.GetUsernamesByIdsAsync(availableFriendIds);

            return availableFriendIds.Select(userId =>
                new ChatFriendView
                {
                    UserId = userId,
                    Username = usernames.TryGetValue(
                        userId,
                        out string? username)
                            ? username
                            : "Unknown user"
                });
        }

        public async Task<int> CreatePrivateRoomAsync(
            string name,
            IEnumerable<string> invitedUserIds)
        {
            string currentUserId = currentUser.GetRequiredUserId();

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("A room name is required.");

            name = name.Trim();

            if (name.Length > 100)
                throw new ArgumentException(
                    "Room names cannot exceed 100 characters.");

            if (invitedUserIds == null)
                throw new ArgumentNullException(nameof(invitedUserIds));

            List<string> userIds = invitedUserIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Where(id => id != currentUserId)
                .Distinct()
                .ToList();

            foreach (string invitedUserId in userIds)
            {
                await EnsureAcceptedFriendshipAsync(
                    currentUserId,
                    invitedUserId);
            }

            ChatRoom room = new ChatRoom
            {
                Name = name,
                RoomType = ChatRoomType.Private,
                CreatedByUserId = currentUserId,
                OwnerUserId = currentUserId,
                CreatedAt = DateTime.UtcNow
            };

            List<ChatRoomMember> members = new()
            {
                new ChatRoomMember
                {
                    UserId = currentUserId,
                    JoinedAt = DateTime.UtcNow
                }
            };

            members.AddRange(userIds.Select(userId =>
                new ChatRoomMember
                {
                    UserId = userId,
                    JoinedAt = DateTime.UtcNow
                }));

            await chatRepository.CreateRoomWithMembersAsync(
                room,
                members);

            return room.Id;
        }

        public async Task InviteFriendAsync(
            int roomId,
            string invitedUserId)
        {
            string currentUserId = currentUser.GetRequiredUserId();

            if (string.IsNullOrWhiteSpace(invitedUserId))
                throw new ArgumentException("A user ID is required.");

            if (invitedUserId == currentUserId)
                throw new InvalidOperationException(
                    "You cannot invite yourself.");

            ChatRoom room = await GetRoomOrThrowAsync(roomId);

            if (room.RoomType != ChatRoomType.Private)
                throw new InvalidOperationException(
                    "Friends can only be invited to private rooms.");

            await EnsureMembershipAsync(roomId, currentUserId);

            await EnsureAcceptedFriendshipAsync(
                currentUserId,
                invitedUserId);

            ChatRoomMember? existingMember =
                await chatRepository.GetMembershipAsync(
                    roomId,
                    invitedUserId);

            if (existingMember != null)
                throw new InvalidOperationException(
                    "This user is already a member of the room.");

            await chatRepository.AddMemberAsync(new ChatRoomMember
            {
                ChatRoomId = roomId,
                UserId = invitedUserId,
                JoinedAt = DateTime.UtcNow
            });
        }

        // --------------------------------
        // Membership
        // --------------------------------

        public async Task JoinMiscellaneousRoomAsync(int roomId)
        {
            string userId = currentUser.GetRequiredUserId();

            ChatRoom room = await GetRoomOrThrowAsync(roomId);

            if (room.RoomType != ChatRoomType.Miscellaneous)
                throw new InvalidOperationException(
                    "This room cannot be joined through this operation.");

            ChatRoomMember? existingMember =
                await chatRepository.GetMembershipAsync(roomId, userId);

            if (existingMember != null)
                return;

            await chatRepository.AddMemberAsync(new ChatRoomMember
            {
                ChatRoomId = roomId,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            });
        }

        public async Task RemoveTemporaryMembershipAsync(int roomId, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return;

            // The room might have been deleted while the user was disconnected.
            ChatRoom? room =
                await chatRepository.GetRoomByIdAsync(roomId);

            if (room == null)
                return;

            // Never automatically remove private-room membership.
            if (room.RoomType != ChatRoomType.Miscellaneous &&
                room.RoomType != ChatRoomType.Topic)
            {
                return;
            }

            ChatRoomMember? membership =
                await chatRepository.GetMembershipAsync(roomId, userId);

            if (membership == null)
                return;

            await chatRepository.RemoveMemberAsync(membership);
        }

        public async Task LeaveRoomAsync(int roomId)
        {
            string userId = currentUser.GetRequiredUserId();

            await chatRepository.LeaveRoomAsync(roomId, userId);
        }

        public async Task<int> GetUnseenPrivateRoomCountAsync()
        {
            string userId = currentUser.GetRequiredUserId();

            return await chatRepository.GetUnseenPrivateRoomCountAsync(userId);
        }

        public async Task MarkRoomAsVisitedAsync(int roomId)
        {
            string userId = currentUser.GetRequiredUserId();

            ChatRoom room = await GetRoomOrThrowAsync(roomId);

            if (room.RoomType != ChatRoomType.Private)
                return;

            await EnsureMembershipAsync(roomId, userId);

            await chatRepository.MarkRoomAsVisitedAsync(roomId, userId);
        }

        // --------------------------------
        // Topic rooms
        // --------------------------------

        public async Task<int> GetOrCreateTopicRoomAsync(int topicId)
        {
            string userId = currentUser.GetRequiredUserId();

            if (topicId <= 0)
                throw new ArgumentException("Invalid topic ID.");

            bool topicExists =
                await chatRepository.TopicExistsAsync(topicId);

            if (!topicExists)
                throw new KeyNotFoundException("Topic not found.");

            ChatRoom? room =
                await chatRepository.GetRoomByTopicIdAsync(topicId);

            if (room == null)
            {
                string? topicTitle =
                    await chatRepository.GetTopicTitleAsync(topicId);

                room = new ChatRoom
                {
                    Name = topicTitle ?? $"Topic {topicId}",
                    RoomType = ChatRoomType.Topic,
                    CreatedByUserId = userId,
                    OwnerUserId = null,
                    TopicId = topicId,
                    CreatedAt = DateTime.UtcNow
                };

                try
                {
                    await chatRepository.CreateRoomWithMembersAsync(
                        room,
                        new[]
                        {
                            new ChatRoomMember
                            {
                                UserId = userId,
                                JoinedAt = DateTime.UtcNow
                            }
                        });
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateException)
                {
                    // Another request may have created this topic room.
                    room = await chatRepository.GetRoomByTopicIdAsync(topicId);

                    if (room == null)
                        throw;
                }
            }

            await GetRoomAsync(room.Id);

            return room.Id;
        }

        // --------------------------------
        // Messages
        // --------------------------------

        public async Task<IEnumerable<ChatMessageView>>
            GetLatestMessagesAsync(int roomId)
        {
            string userId = currentUser.GetRequiredUserId();

            await GetRoomAsync(roomId);

            IEnumerable<ChatMessage> messages =
                await chatRepository.GetLatestMessagesAsync(
                    roomId,
                    MessageLimit);

            List<ChatMessage> messageList = messages.ToList();

            Dictionary<string, string> usernames =
                await userService.GetUsernamesByIdsAsync(
                    messageList.Select(m => m.SenderUserId));

            return messageList.Select(message =>
                new ChatMessageView
                {
                    Id = message.Id,
                    ChatRoomId = message.ChatRoomId,
                    SenderUserId = message.SenderUserId,
                    SenderUsername =
                        usernames.TryGetValue(
                            message.SenderUserId,
                            out string? username)
                            ? username
                            : "Unknown user",
                    Content = message.Content,
                    SentAt = message.SentAt,
                    IsOwnMessage = message.SenderUserId == userId
                });
        }

        public async Task<ChatMessageView> SendMessageAsync(
            int roomId,
            string content)
        {
            string userId = currentUser.GetRequiredUserId();

            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException(
                    "A message cannot be empty.");

            content = content.Trim();

            if (content.Length > MaxMessageLength)
                throw new ArgumentException(
                    $"Messages cannot exceed {MaxMessageLength} characters.");

            await GetRoomOrThrowAsync(roomId);
            await EnsureMembershipAsync(roomId, userId);

            ChatMessage message = new ChatMessage
            {
                ChatRoomId = roomId,
                SenderUserId = userId,
                Content = content,
                SentAt = DateTime.UtcNow
            };

            string username = await userService.GetUsernameByIdAsync(userId);

            await chatRepository.AddMessageAsync(message);

            return new ChatMessageView
            {
                Id = message.Id,
                ChatRoomId = message.ChatRoomId,
                SenderUserId = userId,
                SenderUsername = username,
                Content = message.Content,
                SentAt = message.SentAt,
                IsOwnMessage = true
            };
        }

        // --------------------------------
        // Private helper methods
        // --------------------------------

        private async Task<ChatRoom> GetRoomOrThrowAsync(int roomId)
        {
            ChatRoom? room =
                await chatRepository.GetRoomByIdAsync(roomId);

            if (room == null)
                throw new KeyNotFoundException("Chat room not found.");

            return room;
        }

        private async Task EnsureMembershipAsync(
            int roomId,
            string userId)
        {
            ChatRoomMember? membership =
                await chatRepository.GetMembershipAsync(roomId, userId);

            if (membership == null)
                throw new UnauthorizedAccessException(
                    "You are not a member of this chatroom.");
        }

        private async Task EnsureAcceptedFriendshipAsync(
            string firstUserId,
            string secondUserId)
        {
            Friendship? friendship =
                await friendshipRepository.GetBetweenUsersAsync(
                    firstUserId,
                    secondUserId);

            if (friendship == null ||
                friendship.Status != FriendshipStatus.Accepted)
            {
                throw new InvalidOperationException(
                    "You can only invite accepted friends.");
            }
        }

        private static ChatRoomView MapRoom(
            ChatRoom room,
            string currentUserId)
        {
            return new ChatRoomView
            {
                Id = room.Id,
                Name = room.Name,
                RoomType = room.RoomType.ToString(),
                OwnerUserId = room.OwnerUserId,
                IsOwner = room.OwnerUserId == currentUserId,
                IsMember = true,
                MemberCount = room.Members.Count,
                TopicId = room.TopicId
            };
        }
    }
}
