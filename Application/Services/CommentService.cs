
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

            if (page > totalPages)
            {
                page = totalPages;

                result = await commentRepository.GetPageByPostIdAsync(
                    postId, page, PageSize);
            }

            string? userId = currentUser.GetUserIdOrNull();

            // Retrieve every comment, including replies and nested replies.
            List<Comment> allComments =
                (await commentRepository.GetByPostIdAsync(postId)).ToList();

            // Map each comment once so we can build the tree by ID.
            Dictionary<int, CommentListItemView> items = allComments
                .ToDictionary(
                    c => c.Id,
                    c =>
                    {
                        CommentListItemView item =
                            commentMapper.ToListItemView(c);

                        item.CanManage =
                            userId != null &&
                            c.CreatedByUserId == userId;

                        return item;
                    });

            // Attach each reply to its parent.
            foreach (Comment comment in allComments)
            {
                if (!comment.ParentCommentId.HasValue)
                    continue;

                if (!items.TryGetValue(
                        comment.ParentCommentId.Value,
                        out CommentListItemView? parent))
                {
                    continue;
                }

                CommentListItemView reply = items[comment.Id];

                // Replies have no title, so use their content as the
                // reference text when someone replies to them.
                reply.ParentTitle = !string.IsNullOrWhiteSpace(parent.Title)
                    ? parent.Title
                    : parent.Content;

                reply.ParentCreator = parent.Creator;

                parent.Replies.Add(reply);
            }

            // Return only the paginated top-level comments.
            List<CommentListItemView> topLevelComments = result.Items
                .Select(c => items[c.Id])
                .ToList();

            return new CommentIndexView
            {
                PostId = post.Id,
                TopicId = post.TopicId,
                PostTitle = post.Title,
                PostContent = post.Content,
                PostCreator = post.CreatedByUserName,
                PostCreatedAt = post.CreatedAt,

                Comments = topLevelComments,

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


        public async Task<CommentIndexView?> GetEditViewAsync(int commentId, int page = 1)
        {
            Comment? comment = await commentRepository.GetByIdAsync(commentId);

            if (comment == null)
                return null;

            string userId = currentUser.GetRequiredUserId();

            if (comment.CreatedByUserId != userId)
            {
                throw new UnauthorizedAccessException("You are not allowed to edit this comment.");
            }

            Post? post = await postRepository.GetByIdAsync(comment.PostId);

            if (post == null)
                return null;

            CommentIndexView? viewModel = await GetCommentIndexViewAsync(post.Id, page);

            if (viewModel == null)
                return null;

            viewModel.EditComment = commentMapper.ToEditView(comment);

            return viewModel;
        }


        public async Task CreateAsync(CommentCreateView model)
        {
            string userId = currentUser.GetRequiredUserId();

            if (string.IsNullOrWhiteSpace(model.Content))
            {
                throw new ArgumentException("Comment content is required.");
            }

            if (!model.ParentCommentId.HasValue &&
                string.IsNullOrWhiteSpace(model.Title))
            {
                throw new ArgumentException(
                    "A title is required for a top-level comment.");
            }

            if (model.ParentCommentId.HasValue)
            {
                Comment? parent = await commentRepository
                    .GetByIdAsync(model.ParentCommentId.Value);

                if (parent == null || parent.PostId != model.PostId)
                {
                    throw new ArgumentException(
                        "The parent comment does not belong to this post.");
                }
            }

            Comment comment = commentMapper.ToEntity(model);
            comment.CreatedByUserId = userId;
            comment.CreatedByUserName =
                await userService.GetUsernameByIdAsync(userId);

            await commentRepository.AddAsync(comment);
        }



        public async Task UpdateAsync(CommentEditView model)
        {
            string userId = currentUser.GetRequiredUserId();

            Comment? comment = await commentRepository.GetByIdAsync(model.Id);

            if (comment == null)
                return;

            if (comment.CreatedByUserId != userId)
            {
                throw new UnauthorizedAccessException("You are not allowed to edit this comment.");
            }


            comment.Title = comment.ParentCommentId.HasValue ? string.Empty : model.Title?.Trim() ?? string.Empty;

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
            {
                throw new UnauthorizedAccessException(
                    "You are not allowed to delete this comment.");
            }

            List<Comment> comments =
                (await commentRepository.GetByPostIdAsync(comment.PostId)).ToList();

            Dictionary<int, List<Comment>> repliesByParent = comments
                .Where(c => c.ParentCommentId.HasValue)
                .GroupBy(c => c.ParentCommentId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            List<Comment> deletionOrder = new();

            void AddCommentAndReplies(int commentId)
            {
                Comment? current = comments.FirstOrDefault(c => c.Id == commentId);

                if (current == null)
                    return;

                deletionOrder.Add(current);

                if (repliesByParent.TryGetValue(commentId, out List<Comment>? replies))
                {
                    foreach (Comment reply in replies)
                    {
                        AddCommentAndReplies(reply.Id);
                    }
                }
            }

            AddCommentAndReplies(comment.Id);

            // Delete children before their parents to satisfy the database relationship.
            for (int i = deletionOrder.Count - 1; i >= 0; i--)
            {
                await commentRepository.DeleteAsync(deletionOrder[i].Id);
            }
        }


    }
}
