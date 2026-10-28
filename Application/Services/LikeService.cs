using Application.Interfaces.Services;
using EntityModels;
using Repository.interfaces;

namespace Application.Services
{
    public class LikeService : ILikeService
    {
        private readonly ILikeRepository likeRepository;
        private readonly IPostRepository postRepository;
        private readonly ICommentRepository commentRepository;
        private readonly ICurrentUserService currentUser;
        private readonly IUserService userService;

        public LikeService(
            ILikeRepository likeRepository,
            IPostRepository postRepository,
            ICommentRepository commentRepository,
            ICurrentUserService currentUser,
            IUserService userService)
        {
            this.likeRepository = likeRepository;
            this.postRepository = postRepository;
            this.commentRepository = commentRepository;
            this.currentUser = currentUser;
            this.userService = userService;
        }

        public async Task<LikeToggleResult> TogglePostLikeAsync(int postId)
        {
            string userId = currentUser.GetRequiredUserId();

            Post? post = await postRepository.GetByIdWithoutCommentsAsync(postId);

            if (post == null)
                throw new KeyNotFoundException("Post not found.");

            Like? existingLike =
                await likeRepository.GetPostLikeAsync(postId, userId);

            bool isLiked;

            if (existingLike != null)
            {
                await likeRepository.DeleteAsync(existingLike);
                isLiked = false;
            }
            else
            {
                Like like = new Like
                {
                    PostId = postId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                await likeRepository.AddAsync(like);
                isLiked = true;
            }

            int likeCount =
                await likeRepository.GetPostLikeCountAsync(postId);

            return new LikeToggleResult
            {
                IsLiked = isLiked,
                LikeCount = likeCount
            };
        }

        public async Task<LikeToggleResult> ToggleCommentLikeAsync(int commentId)
        {
            string userId = currentUser.GetRequiredUserId();

            Comment? comment =
                await commentRepository.GetByIdAsync(commentId);

            if (comment == null)
                throw new KeyNotFoundException("Comment not found.");

            Like? existingLike =
                await likeRepository.GetCommentLikeAsync(commentId, userId);

            bool isLiked;

            if (existingLike != null)
            {
                await likeRepository.DeleteAsync(existingLike);
                isLiked = false;
            }
            else
            {
                Like like = new Like
                {
                    CommentId = commentId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                await likeRepository.AddAsync(like);
                isLiked = true;
            }

            Dictionary<int, int> counts =
                await likeRepository.GetCommentLikeCountsAsync(
                    new List<int> { commentId });

            int likeCount = counts.GetValueOrDefault(commentId, 0);

            return new LikeToggleResult
            {
                IsLiked = isLiked,
                LikeCount = likeCount
            };
        }

        public async Task<List<string>> GetPostLikerUsernamesAsync(int postId)
        {
            List<string> userIds =
                await likeRepository.GetUserIdsWhoLikedPostAsync(postId);

            Dictionary<string, string> usernames =
                await userService.GetUsernamesByIdsAsync(userIds);

            return userIds
                .Where(usernames.ContainsKey)
                .Select(userId => usernames[userId])
                .ToList();
        }

        public async Task<List<string>> GetCommentLikerUsernamesAsync(int commentId)
        {
            List<string> userIds =
                await likeRepository.GetUserIdsWhoLikedCommentAsync(commentId);

            Dictionary<string, string> usernames =
                await userService.GetUsernamesByIdsAsync(userIds);

            return userIds
                .Where(usernames.ContainsKey)
                .Select(userId => usernames[userId])
                .ToList();
        }

    }
}