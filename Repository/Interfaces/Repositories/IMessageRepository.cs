using EntityModels;

namespace Repository.Interfaces.Repositories
{
    public interface IMessageRepository
    {
        Task<List<Message>> GetInboxAsync(string recipientUserId);

        Task<List<Message>> GetSentAsync(string senderUserId);

        Task<Message?> GetByIdAsync(int messageId);

        Task<int> GetUnreadCountAsync(string recipientUserId);

        Task AddAsync(Message message);

        Task UpdateAsync(Message message);
    }
}
