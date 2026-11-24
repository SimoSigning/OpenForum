using System;
using System.Collections.Generic;
using System.Text;
using ViewModels.Chat;

namespace Application.Interfaces.Services.Features
{
    public interface IChatService
    {
        // Room retrieval
        Task<IEnumerable<ChatRoomView>> GetMyRoomsAsync();

        Task<IEnumerable<ChatRoomView>> GetMiscellaneousRoomsAsync();

        Task<ChatRoomView> GetRoomAsync(int roomId);

        Task<IEnumerable<ChatMemberView>> GetRoomMembersAsync(int roomId);

        // Private rooms
        Task<IEnumerable<ChatFriendView>> GetFriendsForChatAsync();

        Task<IEnumerable<ChatFriendView>> GetAvailableFriendsToInviteAsync(int roomId);

        Task<int> CreatePrivateRoomAsync(string name, IEnumerable<string> invitedUserIds);

        Task InviteFriendAsync(int roomId, string invitedUserId);

        // Membership
        Task JoinMiscellaneousRoomAsync(int roomId);

        Task LeaveRoomAsync(int roomId);

        // Topic rooms
        Task<int> GetOrCreateTopicRoomAsync(int topicId);

        // Messages
        Task<IEnumerable<ChatMessageView>> GetLatestMessagesAsync(int roomId);

        Task<ChatMessageView> SendMessageAsync(int roomId, string content);
    }
}
