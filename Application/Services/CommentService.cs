using Application.Interfaces.Contexts;
using Application.Interfaces.Mappers;
using Application.Interfaces.Services;
using EntityModels;
using Repository.interfaces;
using ViewModels.Comment;

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

        public async Task<CommentIndexView?> GetPostDetailsViewAsync(int postId)
        {
            Post? post = await postRepository.GetByIdAsync(postId);

            if (post == null)
                return null;

            IEnumerable<Comment> comments = await commentRepository.GetByPostIdAsync(postId);

            return new CommentIndexView
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

        public async Task<CommentIndexView?> GetEditViewAsync(int commentId)
        {
            Comment? comment = await commentRepository.GetByIdAsync(commentId);

            if (comment == null)
                return null;

            string userId = GetRequiredUserId();

            if (comment.CreatedByUserId != userId) throw new UnauthorizedAccessException("You are not allowed to edit this comment.");

            Post? post = await postRepository.GetByIdAsync(comment.PostId);

            if (post == null)
                return null;

            IEnumerable<Comment> comments = await commentRepository.GetByPostIdAsync(post.Id);

            return new CommentIndexView
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
            string userId = GetRequiredUserId();

            Comment comment = commentMapper.ToEntity(model);
            comment.CreatedByUserId = userId;

            await commentRepository.AddAsync(comment);
        }

        public async Task UpdateAsync(CommentEditView model)
        {
            string userId = GetRequiredUserId();
            Comment? comment = await commentRepository.GetByIdAsync(model.Id);

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
            string userId = GetRequiredUserId();
            Comment? comment = await commentRepository.GetByIdAsync(id);

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
