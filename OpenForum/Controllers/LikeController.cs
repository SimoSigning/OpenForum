using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OpenForum.Controllers
{
    [Authorize]
    public class LikeController : Controller
    {
        private readonly ILikeService likeService;

        public LikeController(ILikeService likeService)
        {
            this.likeService = likeService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePostLike(
            int postId,
            string returnUrl)
        {
            await likeService.TogglePostLikeAsync(postId);

            return LocalRedirect(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCommentLike(
            int commentId,
            string returnUrl)
        {
            await likeService.ToggleCommentLikeAsync(commentId);

            return LocalRedirect(returnUrl);
        }
    }
}
