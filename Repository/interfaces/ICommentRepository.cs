using EntityModels;

namespace Repository.interfaces
{
    public interface ICommentRepository
    {
        Task<IEnumerable<Comment>> GetAllAsync();

        Task<Comment?> GetByIdAsync(int id);

        Task<IEnumerable<Comment>> GetByPostIdAsync(int postId);

        Task AddAsync(Comment comment);

        Task UpdateAsync(Comment comment);

        Task DeleteAsync(int id);
    }

}
