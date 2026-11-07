using Application.Interfaces.Mappers.Features;
using Application.Interfaces.Services.Features;
using EntityModels;
using Repository.interfaces;
using ViewModels.Discover;

namespace Application.Services.Features
{
    public class DiscoverService : IDiscoverService
    {
        private readonly ITopicRepository topicRepository;
        private readonly IPostRepository postRepository;
        private readonly IDiscoverMapper discoverMapper;

        public DiscoverService(
            ITopicRepository topicRepository,
            IPostRepository postRepository,
            IDiscoverMapper discoverMapper)
        {
            this.topicRepository = topicRepository;
            this.postRepository = postRepository;
            this.discoverMapper = discoverMapper;
        }

        public async Task<IEnumerable<DiscoverTopicListView>> GetPopularTopicsAsync()
        {
            IEnumerable<Topic> topics = await topicRepository.GetPopularTopicsAsync();

            return topics.Select(topic => discoverMapper.ToDiscoverTopicListView(topic)).ToList();
        }

        public async Task<IEnumerable<DiscoverPostListView>> GetPopularPostsAsync()
        {
            IEnumerable<Post> posts = await postRepository.GetPopularPostsAsync();

            return posts.Select(post => discoverMapper.ToDiscoverPostListView(post)).ToList();
        }

        public async Task<IEnumerable<DiscoverTopicListView>> SearchTopicListAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Enumerable.Empty<DiscoverTopicListView>();

            query = query.Trim();

            IEnumerable<Topic> topics = await topicRepository.SearchByTitleAsync(query);

            return topics.Select(topic => discoverMapper.ToDiscoverTopicListView(topic)).ToList();
        }
    }
}