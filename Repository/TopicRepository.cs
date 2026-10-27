using Microsoft.EntityFrameworkCore;
using EntityModels;
using Repository.interfaces;
using EntityContext;

namespace Repository
{
    public class TopicRepository : ITopicRepository
    {
        private readonly ForumContext context;

        public TopicRepository(ForumContext forumContext)
        {
            context = forumContext;
        }

        public async Task<IEnumerable<Topic>> GetAllAsync()
        {
            return await context.Topics
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Topic> Items, int TotalCount)> GetPageAsync(int page, int pageSize)
        {
            if (page < 1)
                throw new ArgumentOutOfRangeException(nameof(page));

            if (pageSize < 1)
                throw new ArgumentOutOfRangeException(nameof(pageSize));

            int totalCount = await context.Topics.CountAsync();

            List<Topic> items = await context.Topics
                .OrderByDescending(t => t.CreatedAt)
                .ThenByDescending(t => t.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Topic?> GetByIdAsync(int id)
        {
            return await context.Topics.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task AddAsync(Topic topic)
        {
            context.Topics.Add(topic);

            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Topic topic)
        {
            context.Topics.Update(topic);

            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            Topic? topic = await context.Topics.FindAsync(id);

            if (topic == null)
                return;

            context.Topics.Remove(topic);

            await context.SaveChangesAsync();
        }
    }
}
