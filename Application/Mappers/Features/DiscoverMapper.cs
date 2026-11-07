using Application.Interfaces.Mappers.Features;
using EntityModels;
using ViewModels.Discover;

namespace Application.Mappers.Features
{
    public class DiscoverMapper : IDiscoverMapper
    {
        public DiscoverTopicListView ToDiscoverTopicListView(Topic topic)
        {
            return new DiscoverTopicListView
            {
                Id = topic.Id,
                Title = topic.Title,
                Creator = topic.CreatedByUserName,
                CreatedAt = topic.CreatedAt
            };
        }

        public DiscoverPostListView ToDiscoverPostListView(Post post)
        {
            return new DiscoverPostListView
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                Creator = post.CreatedByUserName,
                CreatedAt = post.CreatedAt
            };
        }
    }
}