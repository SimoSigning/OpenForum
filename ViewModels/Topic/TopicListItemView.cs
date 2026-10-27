using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels.Topic
{
    public class TopicListItemView
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Creator { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public bool CanManage { get; set; }
    }
}
