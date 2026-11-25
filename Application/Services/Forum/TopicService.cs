using Application.common.Application.Common;
using Application.Interfaces.Contexts;
using Application.Interfaces.Mappers.Forum;
using Application.Interfaces.Services.Features;
using Application.Interfaces.Services.Forum;
using Application.Interfaces.Services.User;
using EntityContext;
using EntityModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Repository.Interfaces.Repositories;
using Repository.Interfaces.Utils;
using ViewModels.Topic;

namespace Application.Services.Forum
{
    public class TopicService : ITopicService
    {
        private readonly ITopicRepository topicRepository;
        private readonly ITopicMapper topicMapper;
        private readonly ICurrentUserService currentUser;
        private readonly IUserService userService;
        private readonly IUnitOfWork unitOfWork;
        private readonly IChatService chatService;
        private const int PageSize = 5;

        public TopicService(
            ITopicRepository topicRepository,
            ITopicMapper topicMapper,
            ICurrentUserService currentUser,
            IUserService userService,
            IUnitOfWork unitOfWork,
            IChatService chatService)
        {
            this.topicRepository = topicRepository;
            this.topicMapper = topicMapper;
            this.currentUser = currentUser;
            this.userService = userService;
            this.unitOfWork = unitOfWork;
            this.chatService = chatService;
        }

        public async Task<TopicIndexView> GetTopicIndexViewAsync(int page = 1, TopicCreateView? createTopic = null)
        {
            page = Math.Max(1, page);

            (IEnumerable<Topic> Items, int TotalCount) result = await topicRepository.GetPageAsync(page, PageSize);

            int totalPages = Math.Max(1, (int)Math.Ceiling(result.TotalCount / (double)PageSize));

            // If the requested page is beyond the last page,
            // use the last valid page instead.
            if (page > totalPages)
            {
                page = totalPages;
                result = await topicRepository.GetPageAsync(page, PageSize);
            }

            string? userId = currentUser.GetUserIdOrNull();

            List<TopicListItemView> topics = result.Items.Select(
                topic => 
                {
                    TopicListItemView item = topicMapper.ToListItemView(topic);
                    item.CreatorDisplayName = CreatorDisplayNameResolver.Resolve(userId, topic.CreatedByUserId, item.Creator);
                    item.CanManage = userId != null && topic.CreatedByUserId == userId;
                    return item;
                })
                .ToList();

            return new TopicIndexView
            {
                Topics = topics,
                CreateTopic = createTopic ?? new TopicCreateView(),
                EditTopic = null,
                CanCreate = userId != null,
                CurrentPage = page,
                TotalPages = totalPages
            };
        }

        public async Task<TopicIndexView?> GetEditViewAsync(int id, int page = 1, TopicEditView? editTopic = null)
        {
            Topic? topic = await topicRepository.GetByIdAsync(id);

            if (topic == null)
                return null;

            string userId = currentUser.GetRequiredUserId();

            if (topic.CreatedByUserId != userId)
                throw new UnauthorizedAccessException("You are not allowed to edit this topic.");

            TopicIndexView viewModel = await GetTopicIndexViewAsync(page);

            viewModel.EditTopic = editTopic ?? topicMapper.ToEditView(topic);

            return viewModel;
        }

        public async Task CreateAsync(TopicCreateView model)
        {
            string userId = currentUser.GetRequiredUserId();

            Topic topic = topicMapper.ToEntity(model);
            topic.CreatedByUserId = userId;
            topic.CreatedByUserName = await userService.GetUsernameByIdAsync(userId);

            await unitOfWork.BeginTransactionAsync();

            try
            {
                // Save the topic first so it receives its database ID.
                await topicRepository.AddAsync(topic);

                // Create its chatroom and add the creator as the first member.
                await chatService.GetOrCreateTopicRoomAsync(topic.Id);

                await unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task UpdateAsync(TopicEditView model)
        {
            string userId = currentUser.GetRequiredUserId();
            Topic? topic = await topicRepository.GetByIdAsync(model.Id);

            if (topic == null)
                return;

            if (topic.CreatedByUserId != userId)
                throw new UnauthorizedAccessException("You are not allowed to edit this topic.");

            topic.Title = model.Title;

            await topicRepository.UpdateAsync(topic);
        }

        public async Task DeleteAsync(int id)
        {
            string userId = currentUser.GetRequiredUserId();

            Topic? topic = await topicRepository.GetByIdAsync(id);

            if (topic == null)
                return;

            if (topic.CreatedByUserId != userId)
                throw new UnauthorizedAccessException("You are not allowed to delete this topic.");

            await topicRepository.DeleteAsync(id);
        }

    }
}
