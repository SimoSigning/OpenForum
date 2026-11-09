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
        private readonly IUserService userService;

        public PublicProfileController(
            IUserProfileService profileService,
            IFriendshipService friendshipService,
            IUserService userService)
        {
            this.profileService = profileService;
            this.friendshipService = friendshipService;
            this.userService = userService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index(
            string userName,
            string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return BadRequest();
            }

            try
            {
                // Resolve the ID internally using the username.
                string userId =
                    await userService.GetUserIdByUsernameAsync(userName);

                PublicUserProfileView profileView =
                    await profileService.GetPublicProfileAsync(
                        userId, userName);

                UserRelationshipView relationship =
                    await friendshipService.GetRelationshipAsync(
                        profileView.UserId);

                profileView.RelationshipStatus =
                    relationship.RelationshipStatus;

                profileView.CanSendFriendRequest =
                    relationship.CanSendRequest;

                profileView.FriendshipId =
                    relationship.FriendshipId;

                ViewData["ReturnUrl"] = Url.IsLocalUrl(returnUrl)
                    ? returnUrl
                    : Url.Action("Index", "Topic");

                return View(profileView);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}