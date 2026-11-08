using Application.Interfaces.Services.Features;
using Application.Interfaces.Services.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViewModels.Friends;
using ViewModels.PublicUserProfile;

namespace OpenForum.Controllers
{
    public class PublicProfileController : Controller
    {
        private readonly IUserProfileService profileService;
        private readonly IFriendshipService friendshipService;

        public PublicProfileController(
            IUserProfileService profileService,
            IFriendshipService friendshipService)
        {
            this.profileService = profileService;
            this.friendshipService = friendshipService;
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index(string userId, string userName, string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(userName))
            {
                return BadRequest();
            }

            PublicUserProfileView profileView = await profileService.GetPublicProfileAsync(userId, userName);

            if (profileView == null)
                return NotFound();

            UserRelationshipView relationship = await friendshipService.GetRelationshipAsync(profileView.UserId);

            profileView.RelationshipStatus = relationship.RelationshipStatus;
            profileView.CanSendFriendRequest = relationship.CanSendRequest;
            profileView.FriendshipId = relationship.FriendshipId;

            // Only allow local return URLs to prevent open redirects.
            ViewData["ReturnUrl"] = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action("Index", "Topic");

            return View(profileView);
        }

    }
}
