using EntityModels;

namespace Repository.interfaces
{
    public interface ITopicRepository
    {
        Task<(IEnumerable<Topic> Items, int TotalCount)> GetPageAsync(int page, int pageSize);

        Task<IEnumerable<Topic>> GetByCreatedByUserIdAsync(string userId);

        Task<Topic?> GetByIdAsync(int id);

        Task<IEnumerable<Topic>> GetPopularTopicsAsync();

        Task<IEnumerable<Topic>> SearchByTitleAsync(string query);

        Task AddAsync(Topic topic);

        Task UpdateAsync(Topic topic);

        Task DeleteAsync(int id);
    }
}
