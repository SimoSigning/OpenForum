
using Application.Interfaces.Contexts;
using Application.Interfaces.Mappers;
using Application.Interfaces.Services;
using Repository.interfaces;
using ViewModels.Comment;
using ViewModels.Post;

namespace Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository commentRepository;
        private readonly IPostRepository postRepository;
        private readonly ICommentMapper commentMapper;
        private readonly IUserContext userContext;

        public CommentService(
            ICommentRepository commentRepository,
            IPostRepository postRepository,
            ICommentMapper commentMapper,
            IUserContext userContext)
        {
            this.commentRepository = commentRepository;
            this.postRepository = postRepository;
            this.commentMapper = commentMapper;
            this.userContext = userContext;
        }

        public async Task<PostDetailsView?> GetPostDetailsViewAsync(
            int postId)
        {
            var post = await postRepository.GetByIdAsync(postId);

            if (post == null)
                return null;

            var comments = await commentRepository.GetByPostIdAsync(postId);

            return new PostDetailsView
            {
                Id = post.Id,
                TopicId = post.TopicId,
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                Comments = comments.Select(commentMapper.ToListItemView),
                CreateComment = new CommentCreateView
                {
                    PostId = postId
                },
                EditComment = null
            };
        }


        public async Task<PostDetailsView?> GetEditViewAsync(int commentId)
        {
            var comment = await commentRepository.GetByIdAsync(commentId);

            if (comment == null)
                return null;

            var userId = GetRequiredUserId();

            if (comment.CreatedByUserId != userId)
                throw new UnauthorizedAccessException(
                    "You are not allowed to edit this comment.");

            var post = await postRepository.GetByIdAsync(comment.PostId);

            if (post == null)
                return null;

            var comments = await commentRepository.GetByPostIdAsync(post.Id);

            return new PostDetailsView
            {
                Id = post.Id,
                TopicId = post.TopicId,
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                Comments = comments.Select(commentMapper.ToListItemView),
                CreateComment = new CommentCreateView
                {
                    PostId = post.Id
                },
                EditComment = commentMapper.ToEditView(comment)
            };
        }


        public async Task CreateAsync(CommentCreateView model)
        {
            var userId = GetRequiredUserId();

            var comment = commentMapper.ToEntity(model);
            comment.CreatedByUserId = userId;

            await commentRepository.AddAsync(comment);
        }

        public async Task UpdateAsync(CommentEditView model)
        {
            var userId = GetRequiredUserId();
            var comment = await commentRepository.GetByIdAsync(model.Id);

            if (comment == null)
                return;

            if (comment.CreatedByUserId != userId)
                throw new UnauthorizedAccessException(
                    "You are not allowed to edit this comment.");

            comment.Content = model.Content;

            await commentRepository.UpdateAsync(comment);
        }

        public async Task DeleteAsync(int id)
        {
            var userId = GetRequiredUserId();
            var comment = await commentRepository.GetByIdAsync(id);

            if (comment == null)
                return;

            if (comment.CreatedByUserId != userId)
                throw new UnauthorizedAccessException(
                    "You are not allowed to delete this comment.");

            await commentRepository.DeleteAsync(id);
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
