using EntityModels;

namespace Repository.interfaces
{
    public interface IChatRepository
    {
        // Chat rooms
        Task<ChatRoom?> GetRoomByIdAsync(int roomId);

        Task<ChatRoom?> GetRoomByTopicIdAsync(int topicId);

        Task<IEnumerable<ChatRoom>> GetRoomsForUserAsync(string userId);

        Task<IEnumerable<ChatRoom>> GetMiscellaneousRoomsAsync();

        Task AddRoomAsync(ChatRoom room);

        Task DeleteRoomAsync(ChatRoom room);

        Task CreateRoomWithMembersAsync(ChatRoom room, IEnumerable<ChatRoomMember> members);

        Task LeaveRoomAsync(int roomId, string userId, string? newOwnerUserId, bool deleteRoom);

        Task<string?> GetTopicTitleAsync(int topicId);

        Task<bool> TopicExistsAsync(int topicId);

        // Membership
        Task<ChatRoomMember?> GetMembershipAsync(int roomId, string userId);

        Task<IEnumerable<ChatRoomMember>> GetMembersAsync(int roomId);

        Task AddMemberAsync(ChatRoomMember member);

        Task RemoveMemberAsync(ChatRoomMember member);

        // Messages
        Task<IEnumerable<ChatMessage>> GetLatestMessagesAsync(int roomId, int count);

        Task AddMessageAsync(ChatMessage message);

        // Ownership
        Task UpdateRoomAsync(ChatRoom room);
    }
}