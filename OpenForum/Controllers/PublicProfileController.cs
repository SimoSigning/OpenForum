
using Application.Interfaces.Services.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViewModels.PublicUserProfile;

namespace OpenForum.Controllers
{
    public class PublicProfileController : Controller
    {
        private readonly IUserProfileService profileService;

        public PublicProfileController(IUserProfileService profileService)
        {
            this.profileService = profileService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index(string userName, string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return BadRequest();
            }

            try
            {
                PublicUserProfileView model = await profileService.GetPublicProfilePageAsync(userName);

                ViewData["ReturnUrl"] = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action("Index", "Topic");

                return View(model);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
