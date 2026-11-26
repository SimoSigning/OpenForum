
using ViewModels.Messages;

namespace Application.Interfaces.Services.Features
{
    public interface IMessageService
    {
        Task<InboxView> GetInboxAsync();

        Task<SentMessagesView> GetSentAsync();

        Task<MessageDetailsView> GetDetailsAsync(int messageId);

        Task<ComposeMessageView> GetComposeViewAsync(string recipientUserName);

        Task<ComposeMessageView> GetReplyComposeViewAsync(int originalMessageId);

        Task SendAsync(ComposeMessageView model);

        Task<int> GetUnreadCountAsync();

        // Delete a message from the sender's Sent folder.
        Task DeleteSentMessageAsync(int messageId);

        // Delete a message from the recipient's Inbox.
        Task DeleteInboxMessageAsync(int messageId);
    }
}
