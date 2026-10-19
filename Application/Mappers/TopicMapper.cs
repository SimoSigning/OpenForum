using Application.Interfaces.Mappers;
using EntityModels;
using System;
using System.Collections.Generic;
using System.Text;
using ViewModels.Topic;

namespace Application.Mappers
{
    public class TopicMapper : ITopicMapper
    {
        public TopicListItemView ToListItemView(Topic topic)
        {
            return new TopicListItemView
            {
                Id = topic.Id,
                Title = topic.Title,
                CreatedAt = topic.CreatedAt
            };
        }

        public TopicEditView ToEditView(Topic topic)
        {
            return new TopicEditView
            {
                Id = topic.Id,
                Title = topic.Title
            };
        }

        public Topic ToEntity(TopicCreateView model)
        {
            return new Topic
            {
                Title = model.Title,
                CreatedAt = DateTime.UtcNow
            };
        }

        public Topic ToEntity(TopicEditView model)
        {
            return new Topic
            {
                Id = model.Id,
                Title = model.Title
            };
        }
    }
}
