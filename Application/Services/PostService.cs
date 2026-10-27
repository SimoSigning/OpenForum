using Application.Interfaces.Contexts;
using Application.Interfaces.Mappers;
using Application.Interfaces.Services;
using Application.Mappers;
using EntityModels;
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
        private readonly ICurrentUserService currentUser;
        private readonly IUserService userService;
        private const int PageSize = 5;

        public PostService(
            IPostRepository postRepository,
            ITopicRepository topicRepository,
            IPostMapper postMapper,
            ICurrentUserService currentUser,
            IUserService userService)
        {
            this.postRepository = postRepository;
            this.topicRepository = topicRepository;
            this.postMapper = postMapper;
            this.currentUser = currentUser;
            this.userService = userService;
        }

        public async Task<PostIndexView?> GetPostIndexViewAsync(int topicId, int page = 1)
        {
            Topic? topic = await topicRepository.GetByIdAsync(topicId);

            if (topic == null)
                return null;

            page = Math.Max(1, page);

            (IEnumerable<Post> Items, int TotalCount) result = await postRepository.GetPageByTopicIdAsync(topicId, page, PageSize);

            int totalPages = Math.Max(1, (int)Math.Ceiling(result.TotalCount / (double)PageSize));

            // If the requested page is beyond the last page,
            // use the last valid page instead.
            if (page > totalPages)
            {
                page = totalPages;

                result = await postRepository.GetPageByTopicIdAsync(topicId, page, PageSize);
            }

            string? userId = currentUser.GetUserIdOrNull();

            List<PostListItemView> posts = result.Items.Select(
            post =>
            {
                PostListItemView item = postMapper.ToListItemView(post);
                item.CanManage = userId != null && post.CreatedByUserId == userId;
                return item;
            })
            .ToList();

            return new PostIndexView
            {
                TopicId = topic.Id,
                TopicTitle = topic.Title,
                TopicCreator = topic.CreatedByUserName,
                TopicCreatedAt = topic.CreatedAt,
                Posts = posts,
                CreatePost = new PostCreateView
                {
                    TopicId = topicId
                },
                EditPost = null,
                CanCreate = userId != null,
                CurrentPage = page,
                TotalPages = totalPages
            };
        }

        public async Task<PostIndexView?> GetEditViewAsync(int postId, int page = 1)
        {
            Post? post = await postRepository.GetByIdAsync(postId);

            if (post == null)
                return null;

            string userId = currentUser.GetRequiredUserId();

            if (post.CreatedByUserId != userId)
                throw new UnauthorizedAccessException("You are not allowed to edit this post.");

            Topic? topic = await topicRepository.GetByIdAsync(post.TopicId);

            if (topic == null)
                return null;

            PostIndexView? viewModel = await GetPostIndexViewAsync(topic.Id, page);

            if (viewModel == null)
                return null;

            viewModel.EditPost = postMapper.ToEditView(post);

            return viewModel;
        }

        public async Task CreateAsync(PostCreateView model)
        {
            string userId = currentUser.GetRequiredUserId();

            Post post = postMapper.ToEntity(model);
            post.CreatedByUserId = userId;
            post.CreatedByUserName = await userService.GetUsernameByIdAsync(userId);

            await postRepository.AddAsync(post);
        }

        public async Task UpdateAsync(PostEditView model)
        {
            string userId = currentUser.GetRequiredUserId();
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
            string userId = currentUser.GetRequiredUserId();
            Post? post = await postRepository.GetByIdAsync(id);

            if (post == null)
                return;

            if (post.CreatedByUserId != userId)
                throw new UnauthorizedAccessException("You are not allowed to delete this post.");

            await postRepository.DeleteAsync(id);
        }

    }
}
