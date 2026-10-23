using Microsoft.EntityFrameworkCore;
using EntityContext;
using EntityModels;
using Repository.interfaces;

namespace Repository
{
    public class PostRepository : IPostRepository
    {
        private readonly ForumContext context;

        public PostRepository(ForumContext forumContext)
        {
            context = forumContext;
        }

        public async Task<(IEnumerable<Post> Items, int TotalCount)> GetPageByTopicIdAsync(int topicId, int page, int pageSize)
        {
            if (page < 1)
                throw new ArgumentOutOfRangeException(nameof(page));

            if (pageSize < 1)
                throw new ArgumentOutOfRangeException(nameof(pageSize));

            int totalCount = await context.Posts
                .Where(p => p.TopicId == topicId)
                .CountAsync();

            List<Post> items = await context.Posts
                .Where(p => p.TopicId == topicId)
                .OrderByDescending(p => p.CreatedAt)
                .ThenByDescending(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Post?> GetByIdAsync(int id)
        {
            return await context.Posts
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Post>> GetByTopicIdAsync(int topicId)
        {
            return await context.Posts
                .Where(p => p.TopicId == topicId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(Post post)
        {
            context.Posts.Add(post);

            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Post post)
        {
            context.Posts.Update(post);

            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            Post? post = await context.Posts.FindAsync(id);

            if (post == null)
                return;

            context.Posts.Remove(post);

            await context.SaveChangesAsync();
        }
    }
}
