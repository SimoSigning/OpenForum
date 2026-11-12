
using Application.Interfaces.Services.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViewModels.MyUser;

namespace OpenForum.Controllers
{
    [Authorize]
    public class MyProfileController : Controller
    {
        private readonly IUserProfileService profileService;
        private readonly IAccountService accountService;

        public MyProfileController(
            IUserProfileService profileService,
            IAccountService accountService)
        {
            this.profileService = profileService;
            this.accountService = accountService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            MyProfilePageView model = await profileService.GetMyProfilePageAsync();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile([Bind(Prefix = "EditProfile")] UserProfileEditView model)
        {
            if (!ModelState.IsValid)
            {
                MyProfilePageView page = await profileService.GetMyProfilePageAsync(editProfile: model);

                return View("Index", page);
            }

            try
            {
                await profileService.UpdateProfileAsync(model);

                TempData["Success"] = "Profile updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                MyProfilePageView page = await profileService.GetMyProfilePageAsync(editProfile: model);

                return View("Index", page);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeEmail([Bind(Prefix = "ChangeEmail")] ChangeEmailView model)
        {
            if (!ModelState.IsValid)
            {
                MyProfilePageView page = await profileService.GetMyProfilePageAsync(changeEmail: model);

                return View("Index", page);
            }

            try
            {
                await accountService.ChangeEmailAsync(model.NewEmail);

                TempData["Success"] = "Email changed successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                MyProfilePageView page = await profileService.GetMyProfilePageAsync(changeEmail: model);

                return View("Index", page);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            [Bind(Prefix = "ChangePassword")] ChangePasswordView model)
        {
            if (!ModelState.IsValid)
            {
                MyProfilePageView page =await profileService.GetMyProfilePageAsync(changePassword: model);

                return View("Index", page);
            }

            try
            {
                await accountService.ChangePasswordAsync(model.CurrentPassword, model.NewPassword);

                TempData["Success"] = "Password changed successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                MyProfilePageView page = await profileService.GetMyProfilePageAsync(changePassword: model);

                return View("Index", page);
            }
        }
    }
}
