
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
        private readonly ILikeRepository likeRepository;
        private readonly ICommentMapper commentMapper;
        private readonly ICurrentUserService currentUser;
        private readonly IUserService userService;
        private const int PageSize = 5;

        public CommentService(
            ICommentRepository commentRepository,
            IPostRepository postRepository,
            ILikeRepository likeRepository,
            ICommentMapper commentMapper,
            ICurrentUserService currentUser,
            IUserService userService
            )
        {
            this.commentRepository = commentRepository;
            this.postRepository = postRepository;
            this.likeRepository = likeRepository;
            this.commentMapper = commentMapper;
            this.currentUser = currentUser;
            this.userService = userService;
        }


        public async Task<CommentIndexView?> GetCommentIndexViewAsync(
            int postId,
            int page)
        {
            // Get the post.
            var post = await postRepository.GetByIdWithoutCommentsAsync(postId);

            if (post == null)
                return null;

            // Ensure the page number is valid.
            page = Math.Max(1, page);

            // Get the paginated top-level comments.
            var result = await commentRepository.GetPageByPostIdAsync(
                postId, page, PageSize);

            int totalPages = (int)Math.Ceiling(
                result.TotalCount / (double)PageSize);

            // Keep the requested page within the available range.
            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;

                result = await commentRepository.GetPageByPostIdAsync(
                    postId, page, PageSize);
            }

            // Get the current user.
            var userId = currentUser.GetUserIdOrNull();

            int postLikeCount = await likeRepository.GetPostLikeCountAsync(post.Id);

            bool isPostLikedByCurrentUser = userId != null && await likeRepository.IsPostLikedByUserAsync(post.Id, userId);

            // Create view models for the top-level comments on this page.
            var commentsById = result.Items.ToDictionary(
                c => c.Id,
                c =>
                {
                    var view = commentMapper.ToListItemView(c);
                    view.CanManage = c.CreatedByUserId == userId;
                    return view;
                });

            // Start with the IDs of the top-level comments on this page.
            List<int> parentIds = result.Items
                .Select(c => c.Id)
                .ToList();

            // Load replies one level at a time until there are no more.
            while (parentIds.Count > 0)
            {
                var replies = await commentRepository
                    .GetRepliesByParentIdsAsync(postId, parentIds);

                // The next query will find replies to these replies.
                parentIds = new List<int>();

                foreach (var reply in replies)
                {
                    var replyView = commentMapper.ToListItemView(reply);
                    replyView.CanManage = reply.CreatedByUserId == userId;

                    // Attach the reply to its parent.
                    if (reply.ParentCommentId.HasValue &&
                        commentsById.TryGetValue(
                            reply.ParentCommentId.Value,
                            out var parentView))
                    {
                        replyView.ParentTitle =
                            !string.IsNullOrWhiteSpace(parentView.Title)
                                ? parentView.Title
                                : parentView.Content;

                        replyView.ParentCreator = parentView.Creator;

                        parentView.Replies.Add(replyView);
                    }

                    // Store this reply so its own replies can be attached later.
                    commentsById.Add(reply.Id, replyView);
                    parentIds.Add(reply.Id);
                }
            }

            // Get all comment and reply IDs loaded for this page.
            List<int> allCommentIds = commentsById.Keys.ToList();

            // Get like counts for all comments and replies in one query.
            Dictionary<int, int> likeCounts =
                await likeRepository.GetCommentLikeCountsAsync(allCommentIds);

            // Get the comments and replies liked by the current user.
            HashSet<int> likedCommentIds = userId == null
                ? new HashSet<int>()
                : (await likeRepository.GetCommentIdsLikedByUserAsync(
                    allCommentIds, userId)).ToHashSet();

            // Populate the Like properties on every ViewModel.
            foreach (var entry in commentsById)
            {
                CommentListItemView view = entry.Value;

                view.LikeCount = likeCounts.GetValueOrDefault(entry.Key);
                view.IsLikedByCurrentUser = likedCommentIds.Contains(entry.Key);
                view.CanLike = userId != null;
            }

            // Only return the paginated top-level comments.
            // Their Replies properties contain the nested reply trees.
            var comments = result.Items
                .Select(c => commentsById[c.Id])
                .ToList();

            // Build the page view model.
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
                CanCreate = !string.IsNullOrEmpty(userId),
                EditComment = null,
                CurrentPage = page,
                TotalPages = totalPages,
                PostLikeCount = postLikeCount,
                IsPostLikedByCurrentUser = isPostLikedByCurrentUser,
                CanLikePost = userId != null,
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

            Post? post = await postRepository.GetByIdWithoutCommentsAsync(comment.PostId);

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

            Dictionary<int, Comment> commentsById =
                comments.ToDictionary(c => c.Id);

            Dictionary<int, List<Comment>> repliesByParent = comments
                .Where(c => c.ParentCommentId.HasValue)
                .GroupBy(c => c.ParentCommentId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            List<Comment> deletionOrder = new();

            void AddCommentAndReplies(int commentId)
            {
                if (!commentsById.TryGetValue(commentId, out Comment? current))
                    return;

                deletionOrder.Add(current);

                if (repliesByParent.TryGetValue(
                    commentId, out List<Comment>? replies))
                {
                    foreach (Comment reply in replies)
                    {
                        AddCommentAndReplies(reply.Id);
                    }
                }
            }

            AddCommentAndReplies(comment.Id);

            // Mark descendants before their parents for deletion.
            deletionOrder.Reverse();

            // Delete likes belonging to the comment and all its replies.
            List<int> commentIdsToDelete = deletionOrder
                .Select(c => c.Id)
                .ToList();

            await likeRepository.DeleteByCommentIdsAsync(commentIdsToDelete);

            // Delete the comments after their likes have been removed.
            await commentRepository.DeleteRangeAsync(deletionOrder);
        }

    }
}
