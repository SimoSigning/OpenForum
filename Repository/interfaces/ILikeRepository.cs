using EntityModels;

namespace Repository.interfaces
{
    public interface ILikeRepository
    {
        Task<Like?> GetPostLikeAsync(int postId, string userId);

        Task<Like?> GetCommentLikeAsync(int commentId, string userId);

        Task<int> GetPostLikeCountAsync(int postId);

        Task<bool> IsPostLikedByUserAsync(int postId, string userId);

        Task<Dictionary<int, int>> GetCommentLikeCountsAsync(List<int> commentIds);

        Task<List<int>> GetCommentIdsLikedByUserAsync(List<int> commentIds,string userId);

        Task<Dictionary<int, int>> GetPostLikeCountsAsync(List<int> postIds);

        Task<List<int>> GetPostIdsLikedByUserAsync(List<int> postIds,string userId);

        Task AddAsync(Like like);

        Task DeleteAsync(Like like);

        Task DeleteByPostIdAsync(int postId);

        Task DeleteByCommentIdsAsync(List<int> commentIds);
    }
}