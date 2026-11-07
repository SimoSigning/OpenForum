using EntityModels;

namespace Repository.interfaces
{
    public interface IFriendshipRepository
    {
        Task<Friendship?> GetByIdAsync(int id);

        Task<Friendship?> GetBetweenUsersAsync(string firstUserId, string secondUserId);

        Task<IEnumerable<Friendship>> GetAllForUserAsync(string userId);

        Task<IEnumerable<Friendship>> GetIncomingPendingRequestsAsync(string userId);

        Task<IEnumerable<Friendship>> GetAcceptedForUserAsync(string userId);

        Task AddAsync(Friendship friendship);

        Task UpdateAsync(Friendship friendship);

        Task DeleteAsync(int id);
    }
}