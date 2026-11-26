
using EntityContext;
using EntityModels;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces.Repositories;

namespace Repository.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly ForumContext context;

        public MessageRepository(ForumContext forumContext)
        {
            context = forumContext;
        }

        public async Task<List<Message>> GetInboxAsync(string recipientUserId)
        {
            return await context.Messages
                .AsNoTracking()
                .Where(m => m.RecipientUserId == recipientUserId && !m.IsDeletedByRecipient)
                .OrderByDescending(m => m.SentAt)
                .ThenByDescending(m => m.Id)
                .ToListAsync();
        }

        public async Task<List<Message>> GetSentAsync(string senderUserId)
        {
            return await context.Messages
                .AsNoTracking()
                .Where(m => m.SenderUserId == senderUserId && !m.IsDeletedBySender)
                .OrderByDescending(m => m.SentAt)
                .ThenByDescending(m => m.Id)
                .ToListAsync();
        }

        public async Task<Message?> GetByIdAsync(int messageId)
        {
            return await context.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId);
        }

        public async Task<int> GetUnreadCountAsync(string recipientUserId)
        {
            return await context.Messages
                .CountAsync(m =>
                    m.RecipientUserId == recipientUserId &&
                    !m.IsRead &&
                    !m.IsDeletedByRecipient &&
                    !m.IsDeletedBySender);
        }

        public async Task AddAsync(Message message)
        {
            context.Messages.Add(message);

            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Message message)
        {
            context.Messages.Update(message);

            await context.SaveChangesAsync();
        }
    }
}
