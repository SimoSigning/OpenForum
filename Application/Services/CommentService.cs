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
        private readonly ICurrentUserService currentUser;
        private readonly IUserService userService;
        public CommentService(
            ICommentRepository commentRepository,
            IPostRepository postRepository,
            ICommentMapper commentMapper,
            ICurrentUserService currentUser,
            IUserService userService)
        {
            this.commentRepository = commentRepository;
            this.postRepository = postRepository;
            this.commentMapper = commentMapper;
            this.currentUser = currentUser;
            this.userService = userService;
        }

        public async Task<CommentIndexView?> GetCommentIndexViewAsync(int postId)
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
                Creator = post.CreatedByUserName,
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

            string userId = currentUser.GetRequiredUserId();

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
            string userId = currentUser.GetRequiredUserId();

            Comment comment = commentMapper.ToEntity(model);
            comment.CreatedByUserId = userId;
            comment.CreatedByUserName = await userService.GetUsernameByIdAsync(userId);

            await commentRepository.AddAsync(comment);
        }

        public async Task UpdateAsync(CommentEditView model)
        {
            string userId = currentUser.GetRequiredUserId();
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
            string userId = currentUser.GetRequiredUserId();
            Comment? comment = await commentRepository.GetByIdAsync(id);

            if (comment == null)
                return;

            if (comment.CreatedByUserId != userId)
                throw new UnauthorizedAccessException(
                    "You are not allowed to delete this comment.");

            await commentRepository.DeleteAsync(id);
        }

    }
}
