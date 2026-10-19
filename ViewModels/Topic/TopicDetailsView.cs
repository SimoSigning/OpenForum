using System;
using System.Collections.Generic;
using System.Text;
using ViewModels.Post;

namespace ViewModels.Topic
{
    public class TopicDetailsView
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public IEnumerable<PostListItemView> Posts { get; set; } = Enumerable.Empty<PostListItemView>();

        public PostCreateView CreatePost { get; set; } = new();

        public PostEditView? EditPost { get; set; } = null;
    }
}
