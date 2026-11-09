using EntityModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository.interfaces
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