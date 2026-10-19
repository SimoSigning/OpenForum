
using Application.Interfaces.Contexts;
using Application.Interfaces.Mappers;
using Application.Interfaces.Services;
using Repository.interfaces;
using ViewModels.Topic;

namespace Application.Services
{
    public class TopicService : ITopicService
    {
        private readonly ITopicRepository topicRepository;
        private readonly ITopicMapper topicMapper;
        private readonly IUserContext userContext;

        public TopicService(
            ITopicRepository topicRepository,
            ITopicMapper topicMapper,
            IUserContext userContext)
        {
            this.topicRepository = topicRepository;
            this.topicMapper = topicMapper;
            this.userContext = userContext;
        }

        public async Task<TopicIndexView> GetTopicIndexViewAsync()
        {
            var topics = await topicRepository.GetAllAsync();

            return new TopicIndexView
            {
                Topics = topics.Select(topicMapper.ToListItemView),
                CreateTopic = new TopicCreateView(),
                EditTopic = null
            };
        }

        public async Task<TopicIndexView?> GetEditViewAsync(int id)
        {
            var topics = await topicRepository.GetAllAsync();
            var topic = await topicRepository.GetByIdAsync(id);

            if (topic == null)
                return null;

            var userId = GetRequiredUserId();

            if (topic.CreatedByUserId != userId)
                throw new UnauthorizedAccessException(
                    "You are not allowed to edit this topic.");

            return new TopicIndexView
            {
                Topics = topics.Select(topicMapper.ToListItemView),
                CreateTopic = new TopicCreateView(),
                EditTopic = topicMapper.ToEditView(topic)
            };
        }

        public async Task CreateAsync(TopicCreateView model)
        {
            var userId = GetRequiredUserId();

            var topic = topicMapper.ToEntity(model);
            topic.CreatedByUserId = userId;

            await topicRepository.AddAsync(topic);
        }

        public async Task UpdateAsync(TopicEditView model)
        {
            var userId = GetRequiredUserId();
            var topic = await topicRepository.GetByIdAsync(model.Id);

            if (topic == null)
                return;

            if (topic.CreatedByUserId != userId)
                throw new UnauthorizedAccessException(
                    "You are not allowed to edit this topic.");

            topic.Title = model.Title;

            await topicRepository.UpdateAsync(topic);
        }

        public async Task DeleteAsync(int id)
        {
            var userId = GetRequiredUserId();
            var topic = await topicRepository.GetByIdAsync(id);

            if (topic == null)
                return;

            if (topic.CreatedByUserId != userId)
                throw new UnauthorizedAccessException(
                    "You are not allowed to delete this topic.");

            await topicRepository.DeleteAsync(id);
        }

        private string GetRequiredUserId()
        {
            if (!userContext.IsAuthenticated ||
                string.IsNullOrWhiteSpace(userContext.UserId))
            {
                throw new UnauthorizedAccessException(
                    "An authenticated user is required.");
            }

            return userContext.UserId;
        }
    }
}
