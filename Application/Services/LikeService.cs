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

        public LikeService(
            ILikeRepository likeRepository,
            IPostRepository postRepository,
            ICommentRepository commentRepository,
            ICurrentUserService currentUser)
        {
            this.likeRepository = likeRepository;
            this.postRepository = postRepository;
            this.commentRepository = commentRepository;
            this.currentUser = currentUser;
        }

        public async Task TogglePostLikeAsync(int postId)
        {
            string userId = currentUser.GetRequiredUserId();

            Post? post = await postRepository.GetByIdWithoutCommentsAsync(postId);

            if (post == null)
                throw new KeyNotFoundException("Post not found.");

            Like? existingLike =
                await likeRepository.GetPostLikeAsync(postId, userId);

            if (existingLike != null)
            {
                await likeRepository.DeleteAsync(existingLike);
                return;
            }

            Like like = new Like
            {
                PostId = postId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await likeRepository.AddAsync(like);
        }

        public async Task ToggleCommentLikeAsync(int commentId)
        {
            string userId = currentUser.GetRequiredUserId();

            Comment? comment =
                await commentRepository.GetByIdAsync(commentId);

            if (comment == null)
                throw new KeyNotFoundException("Comment not found.");

            Like? existingLike =
                await likeRepository.GetCommentLikeAsync(commentId, userId);

            if (existingLike != null)
            {
                await likeRepository.DeleteAsync(existingLike);
                return;
            }

            Like like = new Like
            {
                CommentId = commentId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await likeRepository.AddAsync(like);
        }
    }
}