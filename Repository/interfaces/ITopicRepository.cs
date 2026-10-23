using EntityModels;

namespace Repository.interfaces
{
    public interface ITopicRepository
    {
        Task<(IEnumerable<Topic> Items, int TotalCount)> GetPageAsync(int page, int pageSize);

        Task<IEnumerable<Topic>> GetAllAsync();

        Task<Topic?> GetByIdAsync(int id);

        Task AddAsync(Topic topic);

        Task UpdateAsync(Topic topic);

        Task DeleteAsync(int id);
    }
}
