using Application.Interfaces.Contexts;
using Application.Interfaces.Mappers;
using Application.Interfaces.Services;
using EntityModels;
using Repository.interfaces;
using ViewModels.Topic;

namespace Application.Services
{
    public class TopicService : ITopicService
    {
        private readonly ITopicRepository topicRepository;
        private readonly ITopicMapper topicMapper;
        private readonly ICurrentUserService currentUser;
        private readonly IUserService userService;
        private const int PageSize = 5;

        public TopicService(
            ITopicRepository topicRepository,
            ITopicMapper topicMapper,
            ICurrentUserService currentUser,
            IUserService userService)
        {
            this.topicRepository = topicRepository;
            this.topicMapper = topicMapper;
            this.currentUser = currentUser;
            this.userService = userService;
        }

        public async Task<TopicIndexView> GetTopicIndexViewAsync(int page = 1)
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
                    item.CanManage = userId != null && topic.CreatedByUserId == userId;
                    return item;
                })
                .ToList();

            return new TopicIndexView
            {
                Topics = topics,
                CreateTopic = new TopicCreateView(),
                EditTopic = null,
                CanCreate = userId != null,
                CurrentPage = page,
                TotalPages = totalPages
            };
        }

        public async Task<TopicIndexView?> GetEditViewAsync(int id, int page = 1)
        {
            Topic? topic = await topicRepository.GetByIdAsync(id);

            if (topic == null)
                return null;

            string userId = currentUser.GetRequiredUserId();

            if (topic.CreatedByUserId != userId)
                throw new UnauthorizedAccessException("You are not allowed to edit this topic.");

            TopicIndexView viewModel = await GetTopicIndexViewAsync(page);

            viewModel.EditTopic = topicMapper.ToEditView(topic);

            return viewModel;
        }

        public async Task CreateAsync(TopicCreateView model)
        {
            string userId = currentUser.GetRequiredUserId();

            Topic topic = topicMapper.ToEntity(model);
            topic.CreatedByUserId = userId;
            topic.CreatedByUserName = await userService.GetUsernameByIdAsync(userId);

            await topicRepository.AddAsync(topic);
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
