using EntityContext;
using EntityModels;
using Microsoft.EntityFrameworkCore;
using Repository.interfaces;

namespace Repository
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly ForumContext context;

        public UserProfileRepository(ForumContext forumContext)
        {
            context = forumContext;
        }

        public async Task<UserProfile?> GetByUserIdAsync(string userId)
        {
            return await context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task AddAsync(UserProfile profile)
        {
            context.UserProfiles.Add(profile);

            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserProfile profile)
        {
            context.UserProfiles.Update(profile);

            await context.SaveChangesAsync();
        }
    }
}