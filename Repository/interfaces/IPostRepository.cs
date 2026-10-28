using EntityModels;

namespace Repository.interfaces
{
    public interface IPostRepository
    {

        Task<(IEnumerable<Post> Items, int TotalCount)> GetPageByTopicIdAsync(int topicId, int page, int pageSize);

        Task<Post?> GetByIdWithoutCommentsAsync(int id);

        Task<Post?> GetByIdAsync(int id);

        Task AddAsync(Post post);

        Task UpdateAsync(Post post);

        Task DeleteAsync(int id);
    }
}
