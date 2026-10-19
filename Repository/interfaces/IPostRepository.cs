using EntityModels;

namespace Repository.interfaces
{
    public interface IPostRepository
    {
        Task<IEnumerable<Post>> GetAllAsync();

        Task<Post?> GetByIdAsync(int id);

        Task<IEnumerable<Post>> GetByTopicIdAsync(int topicId);

        Task AddAsync(Post post);

        Task UpdateAsync(Post post);

        Task DeleteAsync(int id);
    }
}
