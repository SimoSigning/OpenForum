using System;
using System.Collections.Generic;
using System.Text;
using ViewModels.Topic;
using EntityModels;

namespace Application.Interfaces.Mappers.Forum
{
    public interface ITopicMapper
    {
        TopicListItemView ToListItemView(Topic topic);

        TopicEditView ToEditView(Topic topic);

        Topic ToEntity(TopicCreateView model);

        Topic ToEntity(TopicEditView model);
    }
}
