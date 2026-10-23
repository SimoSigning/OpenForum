using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels.Topic
{
    public class TopicIndexView
    {
        public IEnumerable<TopicListItemView> Topics { get; set; } = Enumerable.Empty<TopicListItemView>();

        public TopicCreateView CreateTopic { get; set; } = new();

        public TopicEditView? EditTopic { get; set; } = null;

        public int CurrentPage { get; set; } = 1;

        public int TotalPages { get; set; } = 1;
    }
}
