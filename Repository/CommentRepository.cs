
using Microsoft.EntityFrameworkCore;
using EntityContext;
using EntityModels;
using Repository.interfaces;

namespace Repository
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ForumContext context;

        public CommentRepository(ForumContext forumContext)
        {
            context = forumContext;
        }

        public async Task<(IEnumerable<Comment> Items, int TotalCount)>
            GetPageByPostIdAsync(int postId, int page, int pageSize)
        {
            if (page < 1)
                throw new ArgumentOutOfRangeException(nameof(page));

            if (pageSize < 1)
                throw new ArgumentOutOfRangeException(nameof(pageSize));

            IQueryable<Comment> topLevelComments = context.Comments
                .Where(c => c.PostId == postId && c.ParentCommentId == null);

            int totalCount = await topLevelComments.CountAsync();

            List<Comment> items = await topLevelComments
                .OrderByDescending(c => c.CreatedAt)
                .ThenByDescending(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<IEnumerable<Comment>> GetAllAsync()
        {
            return await context.Comments
                .OrderBy(c => c.CreatedAt)
                .ThenBy(c => c.Id)
                .ToListAsync();
        }

        public async Task<Comment?> GetByIdAsync(int id)
        {
            return await context.Comments
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Comment>> GetByPostIdAsync(int postId)
        {
            return await context.Comments
                .Where(c => c.PostId == postId)
                .OrderBy(c => c.CreatedAt)
                .ThenBy(c => c.Id)
                .ToListAsync();
        }

        public async Task AddAsync(Comment comment)
        {
            context.Comments.Add(comment);

            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Comment comment)
        {
            context.Comments.Update(comment);

            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            Comment? comment = await context.Comments.FindAsync(id);

            if (comment == null)
                return;

            context.Comments.Remove(comment);

            await context.SaveChangesAsync();
        }
    }
}
