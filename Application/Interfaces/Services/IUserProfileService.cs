using System;
using System.Collections.Generic;
using System.Text;
using ViewModels.User;

namespace Application.Interfaces.Services
{
    public interface IUserProfileService
    {
        Task<UserProfileView> GetProfileAsync();

        Task UpdateProfileAsync(UserProfileEditView model);

        Task CreateProfileForUserAsync(string userId);
    }
}
