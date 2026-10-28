using Microsoft.EntityFrameworkCore;
using EntityContext;
using EntityModels;
using Repository.interfaces;

namespace Repository
{
    public class LikeRepository : ILikeRepository
    {
        private readonly ForumContext context;

        public LikeRepository(ForumContext forumContext)
        {
            context = forumContext;
        }

        public async Task<Like?> GetPostLikeAsync(int postId, string userId)
        {
            return await context.Likes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
        }

        public async Task<Like?> GetCommentLikeAsync(int commentId, string userId)
        {
            return await context.Likes.FirstOrDefaultAsync(l =>l.CommentId == commentId && l.UserId == userId);
        }

        public async Task<int> GetPostLikeCountAsync(int postId)
        {
            return await context.Likes.CountAsync(l => l.PostId == postId);
        }

        public async Task<bool> IsPostLikedByUserAsync(int postId, string userId)
        {
            return await context.Likes.AnyAsync(l => l.PostId == postId && l.UserId == userId);
        }

        public async Task<List<string>> GetUserIdsWhoLikedPostAsync(int postId)
        {
            return await context.Likes
                .Where(l => l.PostId == postId)
                .Select(l => l.UserId)
                .ToListAsync();
        }

        public async Task<List<string>> GetUserIdsWhoLikedCommentAsync(int commentId)
        {
            return await context.Likes
                .Where(l => l.CommentId == commentId)
                .Select(l => l.UserId)
                .ToListAsync();
        }

        public async Task<Dictionary<int, int>> GetCommentLikeCountsAsync(List<int> commentIds)
        {
            if (commentIds.Count == 0)
                return new Dictionary<int, int>();

            return await context.Likes
                .Where(l =>l.CommentId.HasValue && commentIds.Contains(l.CommentId.Value))
                .GroupBy(l => l.CommentId!.Value)
                .Select(g => new
                {
                    CommentId = g.Key,
                    Count = g.Count()
                }).ToDictionaryAsync(x => x.CommentId, x => x.Count);
        }

        public async Task<List<int>> GetCommentIdsLikedByUserAsync(List<int> commentIds, string userId)
        {
            if (commentIds.Count == 0)
                return new List<int>();

            return await context.Likes
                .Where(l => l.UserId == userId && l.CommentId.HasValue && commentIds.Contains(l.CommentId.Value))
                .Select(l => l.CommentId!.Value)
                .ToListAsync();
        }

        public async Task<Dictionary<int, int>> GetPostLikeCountsAsync(List<int> postIds)
        {
            if (postIds.Count == 0) return new Dictionary<int, int>();

            return await context.Likes
                .Where(l => l.PostId.HasValue && postIds.Contains(l.PostId.Value))
                .GroupBy(l => l.PostId!.Value)
                .Select(g => new
                {
                    PostId = g.Key,
                    Count = g.Count()
                }).ToDictionaryAsync(x => x.PostId, x => x.Count);
        }

        public async Task<List<int>> GetPostIdsLikedByUserAsync(List<int> postIds, string userId)
        {
            if (postIds.Count == 0)
                return new List<int>();

            return await context.Likes
                .Where(l => l.UserId == userId && l.PostId.HasValue && postIds.Contains(l.PostId.Value))
                .Select(l => l.PostId!.Value)
                .ToListAsync();
        }

        public async Task AddAsync(Like like)
        {
            await context.Likes.AddAsync(like);

            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Like like)
        {
            context.Likes.Remove(like);

            await context.SaveChangesAsync();
        }

        public async Task DeleteByPostIdAsync(int postId)
        {
            await context.Likes
                .Where(l => l.PostId == postId)
                .ExecuteDeleteAsync();
        }

        public async Task DeleteByCommentIdsAsync(List<int> commentIds)
        {
            if (commentIds.Count == 0)
                return;

            await context.Likes
                .Where(l => l.CommentId.HasValue && commentIds.Contains(l.CommentId.Value))
                .ExecuteDeleteAsync();
        }
    }
}