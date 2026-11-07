using EntityModels;

namespace Repository.interfaces
{
    public interface ICommentRepository
    {

        Task<(IEnumerable<Comment> Items, int TotalCount)> GetPageByPostIdAsync(int postId, int page, int pageSize);

        Task<IEnumerable<Comment>> GetByCreatedByUserIdAsync(
    string userId,
    int skip,
    int take);

        Task<int> CountByCreatedByUserIdAsync(string userId);

        Task<List<Comment>> GetRepliesByParentIdsAsync(int postId, List<int> parentIds);

        Task<Comment?> GetByIdAsync(int id);

        Task<IEnumerable<Comment>> GetByPostIdAsync(int postId);

        Task AddAsync(Comment comment);

        Task UpdateAsync(Comment comment);

        Task DeleteRangeAsync(IEnumerable<Comment> comments);

    }

}
