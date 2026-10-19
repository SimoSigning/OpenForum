
using Application.Interfaces.Contexts;
using Application.Interfaces.Mappers;
using Application.Interfaces.Services;
using Repository.interfaces;
using ViewModels.Post;
using ViewModels.Topic;

namespace Application.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository postRepository;
        private readonly ITopicRepository topicRepository;
        private readonly IPostMapper postMapper;
        private readonly IUserContext userContext;

        public PostService(
            IPostRepository postRepository,
            ITopicRepository topicRepository,
            IPostMapper postMapper,
            IUserContext userContext)
        {
            this.postRepository = postRepository;
            this.topicRepository = topicRepository;
            this.postMapper = postMapper;
            this.userContext = userContext;
        }

        public async Task<TopicDetailsView?> GetTopicDetailsViewAsync(
            int topicId)
        {
            var topic = await topicRepository.GetByIdAsync(topicId);

            if (topic == null)
                return null;

            var posts = await postRepository.GetByTopicIdAsync(topicId);

            return new TopicDetailsView
            {
                Id = topic.Id,
                Title = topic.Title,
                Posts = posts.Select(postMapper.ToListItemView),
                CreatePost = new PostCreateView
                {
                    TopicId = topicId
                },
                EditPost = null
            };
        }

        public async Task<TopicDetailsView?> GetEditViewAsync(int postId)
        {
            var post = await postRepository.GetByIdAsync(postId);

            if (post == null)
                return null;

            var userId = GetRequiredUserId();

            if (post.CreatedByUserId != userId)
                throw new UnauthorizedAccessException(
                    "You are not allowed to edit this post.");

            var topic = await topicRepository.GetByIdAsync(post.TopicId);

            if (topic == null)
                return null;

            var posts = await postRepository.GetByTopicIdAsync(post.TopicId);

            return new TopicDetailsView
            {
                Id = topic.Id,
                Title = topic.Title,
                Posts = posts.Select(postMapper.ToListItemView),
                CreatePost = new PostCreateView
                {
                    TopicId = topic.Id
                },
                EditPost = postMapper.ToEditView(post)
            };
        }


        public async Task CreateAsync(PostCreateView model)
        {
            var userId = GetRequiredUserId();

            var post = postMapper.ToEntity(model);
            post.CreatedByUserId = userId;

            await postRepository.AddAsync(post);
        }

        public async Task UpdateAsync(PostEditView model)
        {
            var userId = GetRequiredUserId();
            var post = await postRepository.GetByIdAsync(model.Id);

            if (post == null)
                return;

            if (post.CreatedByUserId != userId)
                throw new UnauthorizedAccessException(
                    "You are not allowed to edit this post.");

            post.Content = model.Content;

            await postRepository.UpdateAsync(post);
        }

        public async Task DeleteAsync(int id)
        {
            var userId = GetRequiredUserId();
            var post = await postRepository.GetByIdAsync(id);

            if (post == null)
                return;

            if (post.CreatedByUserId != userId)
                throw new UnauthorizedAccessException(
                    "You are not allowed to delete this post.");

            await postRepository.DeleteAsync(id);
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
