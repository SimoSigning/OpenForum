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

        public TopicService(
            ITopicRepository topicRepository,
            ITopicMapper topicMapper,
            ICurrentUserService currentUser)
        {
            this.topicRepository = topicRepository;
            this.topicMapper = topicMapper;
            this.currentUser = currentUser;
        }

        public async Task<TopicIndexView> GetTopicIndexViewAsync()
        {
            IEnumerable<Topic> topics = await topicRepository.GetAllAsync();

            return new TopicIndexView
            {
                Topics = topics.Select(topicMapper.ToListItemView),
                CreateTopic = new TopicCreateView(),
                EditTopic = null
            };
        }

        public async Task<TopicIndexView?> GetEditViewAsync(int id)
        {
            IEnumerable<Topic> topics = await topicRepository.GetAllAsync();
            Topic? topic = await topicRepository.GetByIdAsync(id);

            if (topic == null)
                return null;

            string userId = currentUser.GetRequiredUserId();

            if (topic.CreatedByUserId != userId)
                throw new UnauthorizedAccessException("You are not allowed to edit this topic.");

            return new TopicIndexView
            {
                Topics = topics.Select(topicMapper.ToListItemView),
                CreateTopic = new TopicCreateView(),
                EditTopic = topicMapper.ToEditView(topic)
            };
        }

        public async Task CreateAsync(TopicCreateView model)
        {
            string userId = currentUser.GetRequiredUserId();
            Topic topic = topicMapper.ToEntity(model);
            topic.CreatedByUserId = userId;

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
