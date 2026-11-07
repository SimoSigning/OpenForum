using Application.Interfaces.Services.User;
using EntityModels;
using Repository.interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using ViewModels.User;

namespace Application.Services.User
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IUserProfileRepository profileRepository;
        private readonly ICurrentUserService currentUser;

        public UserProfileService(
            IUserProfileRepository profileRepository,
            ICurrentUserService currentUser)
        {
            this.profileRepository = profileRepository;
            this.currentUser = currentUser;
        }

        public async Task<UserProfileView> GetProfileAsync()
        {
            string userId = currentUser.GetRequiredUserId();

            UserProfile? profile = await profileRepository.GetByUserIdAsync(userId);

            if (profile == null)
            {
                throw new InvalidOperationException("User profile could not be found.");
            }

            return new UserProfileView
            {
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                Address = profile.Address,
                PostalCode = profile.PostalCode,
                City = profile.City
            };
        }

        public async Task UpdateProfileAsync(UserProfileEditView model)
        {
            string userId = currentUser.GetRequiredUserId();

            UserProfile? profile = await profileRepository.GetByUserIdAsync(userId);

            if (profile == null)
            {
                throw new InvalidOperationException("User profile could not be found.");
            }

            profile.FirstName = model.FirstName;
            profile.LastName = model.LastName;
            profile.Address = model.Address;
            profile.PostalCode = model.PostalCode;
            profile.City = model.City;

            await profileRepository.UpdateAsync(profile);
        }

        public async Task CreateProfileForUserAsync(string userId)
        {
            UserProfile profile = new UserProfile
            {
                UserId = userId,
                FirstName = string.Empty,
                LastName = string.Empty,
                Address = null,
                PostalCode = null,
                City = null
            };

            await profileRepository.AddAsync(profile);
        }
    }
}
