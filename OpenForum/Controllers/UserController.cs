using Application.Interfaces.Services.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViewModels.User;

namespace OpenForum.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserProfileService profileService;
        private readonly IAccountService accountService;

        public UserController(
            IUserProfileService profileService,
            IAccountService accountService)
        {
            this.profileService = profileService;
            this.accountService = accountService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ProfilePageView model = await BuildProfilePageAsync();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile([Bind(Prefix = "EditProfile")] UserProfileEditView model)
        {
            if (!ModelState.IsValid)
            {
                ProfilePageView page = await BuildProfilePageAsync(editProfile: model);

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

                ProfilePageView page = await BuildProfilePageAsync(editProfile: model);

                return View("Index", page);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeEmail([Bind(Prefix = "ChangeEmail")] ChangeEmailView model)
        {
            if (!ModelState.IsValid)
            {
                ProfilePageView page = await BuildProfilePageAsync(changeEmail: model);

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

                ProfilePageView page = await BuildProfilePageAsync(changeEmail: model);

                return View("Index", page);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword([Bind(Prefix = "ChangePassword")] ChangePasswordView model)
        {
            if (!ModelState.IsValid)
            {
                ProfilePageView page = await BuildProfilePageAsync(changePassword: model);

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

                ProfilePageView page = await BuildProfilePageAsync(changePassword: model);

                return View("Index", page);
            }
        }

        private async Task<ProfilePageView> BuildProfilePageAsync(
            UserProfileEditView? editProfile = null,
            ChangeEmailView? changeEmail = null,
            ChangePasswordView? changePassword = null)
        {
            UserProfileView profile = await profileService.GetProfileAsync();

            string email = await accountService.GetEmailAsync();

            return new ProfilePageView
            {
                Profile = profile,

                EditProfile = editProfile ?? new UserProfileEditView
                {
                    FirstName = profile.FirstName,
                    LastName = profile.LastName,
                    Address = profile.Address,
                    PostalCode = profile.PostalCode,
                    City = profile.City
                },

                ChangeEmail = changeEmail ?? new ChangeEmailView(),

                ChangePassword = changePassword ?? new ChangePasswordView(),

                CurrentEmail = email
            };
        }
    }
}
