using Application.Interfaces.Mappers;
using Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Repository.interfaces;
using ViewModels.Topic;

namespace Application.Services
{
    public class TopicService : ITopicService
    {
        private readonly ITopicRepository topicRepository;
        private readonly ITopicMapper topicMapper;


        public TopicService(
            ITopicRepository topicRepository,
            ITopicMapper topicMapper)
        {
            this.topicRepository = topicRepository;
            this.topicMapper = topicMapper;
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


            return new TopicIndexView
            {
                Topics = topics.Select(topicMapper.ToListItemView),

                CreateTopic = new TopicCreateView(),

                EditTopic = topicMapper.ToEditView(topic)
            };
        }


        public async Task CreateAsync(TopicCreateView model)
        {
            var topic = topicMapper.ToEntity(model);

            await topicRepository.AddAsync(topic);
        }


        public async Task UpdateAsync(TopicEditView model)
        {
            var topic = await topicRepository.GetByIdAsync(model.Id);

            if (topic == null)
                return;

            topic.Title = model.Title;

            await topicRepository.UpdateAsync(topic);
        }


        public async Task DeleteAsync(int id)
        {
            await topicRepository.DeleteAsync(id);
        }
    }
}
