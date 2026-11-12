
using ViewModels.MyUser;
using ViewModels.PublicUserProfile;

namespace Application.Interfaces.Services.User
{
    public interface IUserProfileService
    {
        Task<MyUserProfileView> GetLoggedInProfileAsync();

        Task<MyProfilePageView> GetMyProfilePageAsync(
            UserProfileEditView? editProfile = null,
            ChangeEmailView? changeEmail = null,
            ChangePasswordView? changePassword = null);

        Task UpdateProfileAsync(UserProfileEditView model);

        Task CreateProfileForUserAsync(string userId);

        Task<PublicUserProfileView> GetPublicProfileAsync(
            string userId,
            string userName);

        Task<PublicUserProfileView> GetPublicProfilePageAsync(
            string userName);
    }
}
