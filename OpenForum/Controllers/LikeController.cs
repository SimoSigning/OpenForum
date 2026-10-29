using Application.Interfaces.Services;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

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
        public async Task<IActionResult> TogglePostLike(int postId, string? returnUrl)
        {
            LikeToggleResult result = await likeService.TogglePostLikeAsync(postId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Ok(result);
            }

            return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCommentLike(int commentId, string? returnUrl)
        {
            LikeToggleResult result = await likeService.ToggleCommentLikeAsync(commentId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Ok(result);
            }

            return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/");
        }

        [HttpGet]
        public async Task<IActionResult> GetPostLikers(int postId)
        {
            if (postId <= 0)
                return BadRequest();

            List<string> usernames = await likeService.GetPostLikerUsernamesAsync(postId);

            return Ok(usernames);
        }

        [HttpGet]
        public async Task<IActionResult> GetCommentLikers(int commentId)
        {
            if (commentId <= 0)
                return BadRequest();

            List<string> usernames = await likeService.GetCommentLikerUsernamesAsync(commentId);

            return Ok(usernames);
        }
    }
}
