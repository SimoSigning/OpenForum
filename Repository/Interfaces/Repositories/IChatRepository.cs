using EntityModels;

namespace Repository.Interfaces.Repositories
{
    public interface IChatRepository
    {
        // Chat rooms
        Task<ChatRoom?> GetRoomByIdAsync(int roomId);

        Task<ChatRoom?> GetRoomByTopicIdAsync(int topicId);

        Task<IEnumerable<ChatRoom>> GetRoomsForUserAsync(string userId);

        Task<IEnumerable<ChatRoom>> GetMiscellaneousRoomsAsync();

        Task CreateRoomWithMembersAsync(ChatRoom room, IEnumerable<ChatRoomMember> members);

        Task LeaveRoomAsync(int roomId, string userId);

        Task<string?> GetTopicTitleAsync(int topicId);

        Task<bool> TopicExistsAsync(int topicId);

        Task<int> GetUnseenPrivateRoomCountAsync(string userId);

        Task MarkRoomAsVisitedAsync(int roomId, string userId);

        // Membership
        Task<ChatRoomMember?> GetMembershipAsync(int roomId, string userId);

        Task<IEnumerable<ChatRoomMember>> GetMembersAsync(int roomId);

        Task AddMemberAsync(ChatRoomMember member);

        Task RemoveMemberAsync(ChatRoomMember member);

        // Messages
        Task<IEnumerable<ChatMessage>> GetLatestMessagesAsync(int roomId, int count);

        Task AddMessageAsync(ChatMessage message);

    }
}