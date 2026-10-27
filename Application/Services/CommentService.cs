
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

        private const int PageSize = 10;

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

        public async Task<CommentIndexView?> GetCommentIndexViewAsync(
            int postId,
            int page = 1)
        {
            Post? post = await postRepository.GetByIdAsync(postId);

            if (post == null)
                return null;

            page = Math.Max(1, page);

            var result = await commentRepository.GetPageByPostIdAsync(
                postId, page, PageSize);

            int totalPages = Math.Max(
                1,
                (int)Math.Ceiling(result.TotalCount / (double)PageSize));

            // If the requested page exceeds the last page,
            // use the last valid page.
            if (page > totalPages)
            {
                page = totalPages;

                result = await commentRepository.GetPageByPostIdAsync(
                    postId, page, PageSize);
            }

            string? userId = currentUser.GetUserIdOrNull();

            List<CommentListItemView> comments = result.Items
                .Select(comment => MapCommentForList(comment, userId))
                .ToList();

            return new CommentIndexView
            {
                PostId = post.Id,
                TopicId = post.TopicId,
                PostTitle = post.Title,
                PostContent = post.Content,
                PostCreator = post.CreatedByUserName,
                PostCreatedAt = post.CreatedAt,
                Comments = comments,
                CreateComment = new CommentCreateView
                {
                    PostId = postId
                },
                CanCreate = userId != null,
                EditComment = null,
                CurrentPage = page,
                TotalPages = totalPages
            };
        }

        private CommentListItemView MapCommentForList(
            Comment comment,
            string? userId)
        {
            CommentListItemView item =
                commentMapper.ToListItemView(comment);

            item.CanManage =
                userId != null &&
                comment.CreatedByUserId == userId;

            // Replies are limited to one level.
            item.CanReply =
                userId != null &&
                comment.ParentCommentId == null;

            item.Replies = comment.Replies
                .OrderBy(reply => reply.CreatedAt)
                .ThenBy(reply => reply.Id)
                .Select(reply =>
                {
                    CommentListItemView replyItem =
                        commentMapper.ToListItemView(reply);

                    replyItem.CanManage =
                        userId != null &&
                        reply.CreatedByUserId == userId;

                    // Do not allow replies to replies.
                    replyItem.CanReply = false;

                    // Enforce a single reply level in the view model.
                    replyItem.Replies = new List<CommentListItemView>();

                    return replyItem;
                })
                .ToList();

            return item;
        }

        public async Task<CommentIndexView?> GetEditViewAsync(
            int commentId,
            int page = 1)
        {
            Comment? comment =
                await commentRepository.GetByIdAsync(commentId);

            if (comment == null)
                return null;

            string userId = currentUser.GetRequiredUserId();

            if (comment.CreatedByUserId != userId)
            {
                throw new UnauthorizedAccessException(
                    "You are not allowed to edit this comment.");
            }

            Post? post =
                await postRepository.GetByIdAsync(comment.PostId);

            if (post == null)
                return null;

            CommentIndexView? viewModel =
                await GetCommentIndexViewAsync(post.Id, page);

            if (viewModel == null)
                return null;

            viewModel.EditComment = commentMapper.ToEditView(comment);

            return viewModel;
        }

        public async Task CreateAsync(CommentCreateView model)
        {
            string userId = currentUser.GetRequiredUserId();

            // Ensure the target post exists.
            Post? post = await postRepository.GetByIdAsync(model.PostId);

            if (post == null)
            {
                throw new ArgumentException(
                    "The post does not exist.",
                    nameof(model.PostId));
            }

            if (model.ParentCommentId.HasValue)
            {
                Comment? parent = await commentRepository.GetByIdAsync(
                    model.ParentCommentId.Value);

                // The parent must exist and belong to this post.
                if (parent == null || parent.PostId != model.PostId)
                {
                    throw new ArgumentException(
                        "The selected parent comment is invalid.",
                        nameof(model.ParentCommentId));
                }

                // Only top-level comments can have replies.
                if (parent.ParentCommentId.HasValue)
                {
                    throw new ArgumentException(
                        "Replies to replies are not allowed.",
                        nameof(model.ParentCommentId));
                }
            }

            Comment comment = commentMapper.ToEntity(model);

            // Set hierarchy and identity on the server.
            comment.PostId = model.PostId;
            comment.ParentCommentId = model.ParentCommentId;
            comment.CreatedByUserId = userId;
            comment.CreatedByUserName =
                await userService.GetUsernameByIdAsync(userId);

            // Replies do not have titles.
            if (comment.ParentCommentId.HasValue)
            {
                comment.Title = string.Empty;
            }

            await commentRepository.AddAsync(comment);
        }

        public async Task UpdateAsync(CommentEditView model)
        {
            string userId = currentUser.GetRequiredUserId();

            Comment? comment =
                await commentRepository.GetByIdAsync(model.Id);

            if (comment == null)
                return;

            if (comment.CreatedByUserId != userId)
            {
                throw new UnauthorizedAccessException(
                    "You are not allowed to edit this comment.");
            }

            comment.Content = model.Content;

            // Keep the title empty for replies.
            // Top-level title validation should be handled by the
            // view model/controller before this method is called.
            if (comment.ParentCommentId == null)
            {
                comment.Title = model.Title ?? string.Empty;
            }
            else
            {
                comment.Title = string.Empty;
            }

            await commentRepository.UpdateAsync(comment);
        }

        public async Task DeleteAsync(int id)
        {
            string userId = currentUser.GetRequiredUserId();

            Comment? comment =
                await commentRepository.GetByIdAsync(id);

            if (comment == null)
                return;

            if (comment.CreatedByUserId != userId)
            {
                throw new UnauthorizedAccessException(
                    "You are not allowed to delete this comment.");
            }

            await commentRepository.DeleteAsync(id);
        }
    }
}
