using Application.Interfaces.Contexts;
using Application.Interfaces.Mappers;
using Application.Interfaces.Services;
using EntityModels;
using Repository.interfaces;
using ViewModels.Post;

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

        public async Task<PostIndexView?> GetTopicDetailsViewAsync(int topicId)
        {
            Topic? topic = await topicRepository.GetByIdAsync(topicId);

            if (topic == null)
                return null;

            IEnumerable<Post> posts = await postRepository.GetByTopicIdAsync(topicId);

            return new PostIndexView
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

        public async Task<PostIndexView?> GetEditViewAsync(int postId)
        {
            Post? post = await postRepository.GetByIdAsync(postId);

            if (post == null)
                return null;

            string userId = GetRequiredUserId();

            if (post.CreatedByUserId != userId)
                throw new UnauthorizedAccessException("You are not allowed to edit this post.");

            Topic? topic = await topicRepository.GetByIdAsync(post.TopicId);

            if (topic == null)
                return null;

            IEnumerable<Post> posts = await postRepository.GetByTopicIdAsync(post.TopicId);

            return new PostIndexView
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
            string userId = GetRequiredUserId();

            Post post = postMapper.ToEntity(model);
            post.CreatedByUserId = userId;

            await postRepository.AddAsync(post);
        }

        public async Task UpdateAsync(PostEditView model)
        {
            string userId = GetRequiredUserId();
            Post? post = await postRepository.GetByIdAsync(model.Id);

            if (post == null)
                return;

            if (post.CreatedByUserId != userId)
                throw new UnauthorizedAccessException("You are not allowed to edit this post.");

            post.Content = model.Content;

            await postRepository.UpdateAsync(post);
        }

        public async Task DeleteAsync(int id)
        {
            string userId = GetRequiredUserId();
            Post? post = await postRepository.GetByIdAsync(id);

            if (post == null)
                return;

            if (post.CreatedByUserId != userId)
                throw new UnauthorizedAccessException("You are not allowed to delete this post.");

            await postRepository.DeleteAsync(id);
        }

        private string GetRequiredUserId()
        {
            if (!userContext.IsAuthenticated || string.IsNullOrWhiteSpace(userContext.UserId))
            {
                throw new UnauthorizedAccessException("An authenticated user is required.");
            }

            return userContext.UserId;
        }
    }
}
