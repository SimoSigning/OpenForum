
using Microsoft.EntityFrameworkCore;
using EntityModels;
using Repository.interfaces;
using EntityContext;

namespace Repository
{
    public class FriendshipRepository : IFriendshipRepository
    {
        private readonly ForumContext context;

        public FriendshipRepository(ForumContext forumContext)
        {
            context = forumContext;
        }

        public async Task<Friendship?> GetByIdAsync(int id)
        {
            return await context.Friendships.FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<Friendship?> GetBetweenUsersAsync(string firstUserId, string secondUserId)
        {
            return await context.Friendships.FirstOrDefaultAsync(f =>
                (f.RequesterUserId == firstUserId && f.AddresseeUserId == secondUserId) || (f.RequesterUserId == secondUserId && f.AddresseeUserId == firstUserId));
        }

        public async Task<IEnumerable<Friendship>> GetAllForUserAsync(string userId)
        {
            return await context.Friendships
                .AsNoTracking()
                .Where(f => f.RequesterUserId == userId || f.AddresseeUserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Friendship>> GetIncomingPendingRequestsAsync(string userId)
        {
            return await context.Friendships
                .AsNoTracking()
                .Where(f => f.AddresseeUserId == userId && f.Status == FriendshipStatus.Pending)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Friendship>> GetAcceptedForUserAsync(string userId)
        {
            return await context.Friendships
                .AsNoTracking()
                .Where(f => f.Status == FriendshipStatus.Accepted && (f.RequesterUserId == userId || f.AddresseeUserId == userId))
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(Friendship friendship)
        {
            context.Friendships.Add(friendship);

            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Friendship friendship)
        {
            context.Friendships.Update(friendship);

            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            Friendship? friendship = await context.Friendships.FindAsync(id);

            if (friendship == null)
                return;

            context.Friendships.Remove(friendship);

            await context.SaveChangesAsync();
        }
    }
}
