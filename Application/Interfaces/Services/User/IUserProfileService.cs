using System;
using System.Collections.Generic;
using System.Text;
using ViewModels.PublicUserProfile;
using ViewModels.MyUser;

namespace Application.Interfaces.Services.User
{
    public interface IUserProfileService
    {
        Task<MyUserProfileView> GetLoggedInProfileAsync();

        Task UpdateProfileAsync(UserProfileEditView model);

        Task CreateProfileForUserAsync(string userId);

        Task<PublicUserProfileView> GetPublicProfileAsync(string userId, string userName);
    }
}
