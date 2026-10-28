
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

        public async Task<(IEnumerable<Comment> Items, int TotalCount)> GetPageByPostIdAsync(int postId, int page, int pageSize)
        {
            if (page < 1)
                throw new ArgumentOutOfRangeException(nameof(page));

            if (pageSize < 1)
                throw new ArgumentOutOfRangeException(nameof(pageSize));

            IQueryable<Comment> topLevelComments = context.Comments
                .AsNoTracking()
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

        public async Task<List<Comment>> GetRepliesByParentIdsAsync(int postId, List<int> parentIds)
        {
            if (parentIds == null || parentIds.Count == 0)
                return new List<Comment>();

            return await context.Comments
                .Where(c =>
                    c.PostId == postId &&
                    c.ParentCommentId.HasValue &&
                    parentIds.Contains(c.ParentCommentId.Value))
                .OrderBy(c => c.CreatedAt)
                .ThenBy(c => c.Id)
                .ToListAsync();
        }

        public async Task<Comment?> GetByIdAsync(int id)
        {
            return await context.Comments.FirstOrDefaultAsync(c => c.Id == id);
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

        public async Task DeleteRangeAsync(IEnumerable<Comment> comments)
        {
            context.Comments.RemoveRange(comments);
            await context.SaveChangesAsync();
        }

    }
}
