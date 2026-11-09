using Application.Interfaces.Services.Features;
using Application.Interfaces.Services.User;
using EntityModels;
using Repository.interfaces;
using ViewModels.Messages;

namespace Application.Services.Features
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository messageRepository;
        private readonly IUserService userService;
        private readonly ICurrentUserService currentUser;

        public MessageService(
            IMessageRepository messageRepository,
            IUserService userService,
            ICurrentUserService currentUser)
        {
            this.messageRepository = messageRepository;
            this.userService = userService;
            this.currentUser = currentUser;
        }

        public async Task<InboxView> GetInboxAsync()
        {
            string userId = currentUser.GetRequiredUserId();

            List<Message> messages =
                await messageRepository.GetInboxAsync(userId);

            Dictionary<string, string> usernames =
                await userService.GetUsernamesByIdsAsync(
                    messages.Select(m => m.SenderUserId));

            return new InboxView
            {
                Messages = messages.Select(m => new MessageItemView
                {
                    Id = m.Id,
                    OtherUserId = m.SenderUserId,
                    OtherUserName = usernames.GetValueOrDefault(
                        m.SenderUserId, "Unknown user"),
                    Subject = m.Subject,
                    BodyPreview = CreatePreview(m.Body),
                    SentAt = m.SentAt,
                    IsRead = m.IsRead
                }),

                UnreadCount = await messageRepository.GetUnreadCountAsync(userId)
            };
        }

        public async Task<SentMessagesView> GetSentAsync()
        {
            string userId = currentUser.GetRequiredUserId();

            List<Message> messages =
                await messageRepository.GetSentAsync(userId);

            Dictionary<string, string> usernames =
                await userService.GetUsernamesByIdsAsync(
                    messages.Select(m => m.RecipientUserId));

            return new SentMessagesView
            {
                Messages = messages.Select(m => new MessageItemView
                {
                    Id = m.Id,
                    OtherUserId = m.RecipientUserId,
                    OtherUserName = usernames.GetValueOrDefault(
                        m.RecipientUserId, "Unknown user"),
                    Subject = m.Subject,
                    BodyPreview = CreatePreview(m.Body),
                    SentAt = m.SentAt,
                    IsRead = m.IsRead
                })
            };
        }

        public async Task<MessageDetailsView> GetDetailsAsync(int messageId)
        {
            string userId = currentUser.GetRequiredUserId();

            Message? message =
                await messageRepository.GetByIdAsync(messageId);

            if (message == null)
            {
                throw new KeyNotFoundException("Message not found.");
            }

            bool isRecipient = message.RecipientUserId == userId;
            bool isSender = message.SenderUserId == userId;

            if (!isRecipient && !isSender)
            {
                throw new UnauthorizedAccessException(
                    "You are not allowed to access this message.");
            }

            // Only mark incoming messages as read.
            if (isRecipient && !message.IsRead)
            {
                message.IsRead = true;

                await messageRepository.UpdateAsync(message);
            }

            string senderUsername =
                await userService.GetUsernameByIdAsync(message.SenderUserId);

            string recipientUsername =
                await userService.GetUsernameByIdAsync(message.RecipientUserId);

            return new MessageDetailsView
            {
                Id = message.Id,

                SenderUserId = message.SenderUserId,
                SenderUserName = senderUsername,

                RecipientUserId = message.RecipientUserId,
                RecipientUserName = recipientUsername,

                Subject = message.Subject,
                Body = message.Body,
                SentAt = message.SentAt,
                IsRead = message.IsRead,

                CanReply = isRecipient
            };
        }

        public async Task<ComposeMessageView> GetComposeViewAsync(
            string recipientUserName)
        {
            string currentUserId = currentUser.GetRequiredUserId();

            if (string.IsNullOrWhiteSpace(recipientUserName))
            {
                throw new ArgumentException("A recipient is required.");
            }

            string recipientUserId =
                await userService.GetUserIdByUsernameAsync(recipientUserName);

            if (recipientUserId == currentUserId)
            {
                throw new ArgumentException(
                    "You cannot send a message to yourself.");
            }

            string username =
                await userService.GetUsernameByIdAsync(recipientUserId);

            return new ComposeMessageView
            {
                RecipientUserId = recipientUserId,
                RecipientUserName = username
            };
        }

        public async Task<ComposeMessageView> GetReplyComposeViewAsync(
            int originalMessageId)
        {
            string userId = currentUser.GetRequiredUserId();

            Message? originalMessage =
                await messageRepository.GetByIdAsync(originalMessageId);

            if (originalMessage == null)
            {
                throw new KeyNotFoundException("Original message not found.");
            }

            // Only the recipient of the original message can reply.
            if (originalMessage.RecipientUserId != userId)
            {
                throw new UnauthorizedAccessException(
                    "You are not allowed to reply to this message.");
            }

            if (!await userService.UserExistsAsync(originalMessage.SenderUserId))
            {
                throw new InvalidOperationException(
                    "The original sender no longer exists.");
            }

            string username =
                await userService.GetUsernameByIdAsync(
                    originalMessage.SenderUserId);

            return new ComposeMessageView
            {
                RecipientUserId = originalMessage.SenderUserId,
                RecipientUserName = username,
                Subject = CreateReplySubject(originalMessage.Subject),
                IsReply = true,
                OriginalMessageId = originalMessage.Id
            };
        }

        public async Task SendAsync(ComposeMessageView model)
        {
            string senderUserId = currentUser.GetRequiredUserId();

            if (string.IsNullOrWhiteSpace(model.Body))
            {
                throw new ArgumentException("Message body is required.");
            }

            string recipientUserId;
            string subject;

            if (model.IsReply)
            {
                if (!model.OriginalMessageId.HasValue)
                {
                    throw new ArgumentException("Original message is required for a reply.");
                }

                Message? originalMessage =
                    await messageRepository.GetByIdAsync(model.OriginalMessageId.Value);

                if (originalMessage == null)
                {
                    throw new KeyNotFoundException(
                        "Original message not found.");
                }

                if (originalMessage.RecipientUserId != senderUserId)
                {
                    throw new UnauthorizedAccessException(
                        "You are not allowed to reply to this message.");
                }

                recipientUserId = originalMessage.SenderUserId;

                // Never trust the submitted reply subject.
                subject = CreateReplySubject(originalMessage.Subject);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(model.RecipientUserName))
                {
                    throw new ArgumentException("A recipient is required.");
                }

                // Resolve the recipient again instead of trusting a hidden ID.
                recipientUserId =
                    await userService.GetUserIdByUsernameAsync(
                        model.RecipientUserName);

                if (recipientUserId == senderUserId)
                {
                    throw new ArgumentException(
                        "You cannot send a message to yourself.");
                }

                subject = model.Subject?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(subject))
                {
                    throw new ArgumentException("Subject is required.");
                }
            }

            if (recipientUserId == senderUserId)
            {
                throw new ArgumentException(
                    "You cannot send a message to yourself.");
            }

            if (!await userService.UserExistsAsync(recipientUserId))
            {
                throw new KeyNotFoundException("Recipient not found.");
            }

            string body = model.Body.Trim();

            if (body.Length > 5000)
            {
                throw new ArgumentException(
                    "Message body cannot exceed 5000 characters.");
            }

            if (subject.Length > 150)
            {
                throw new ArgumentException(
                    "Message subject cannot exceed 150 characters.");
            }

            Message message = new Message
            {
                SenderUserId = senderUserId,
                RecipientUserId = recipientUserId,
                Subject = subject,
                Body = body,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            await messageRepository.AddAsync(message);
        }

        public async Task<int> GetUnreadCountAsync()
        {
            string userId = currentUser.GetRequiredUserId();

            return await messageRepository.GetUnreadCountAsync(userId);
        }

        private static string CreatePreview(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return string.Empty;
            }

            const int previewLength = 100;

            string preview = body.Trim();

            if (preview.Length <= previewLength)
            {
                return preview;
            }

            return preview.Substring(0, previewLength) + "...";
        }

        private static string CreateReplySubject(string originalSubject)
        {
            const string prefix = "Reply to: ";

            string subject = prefix + originalSubject;

            // The database allows a maximum of 150 characters.
            return subject.Length <= 150
                ? subject
                : subject.Substring(0, 150);
        }
    }
}