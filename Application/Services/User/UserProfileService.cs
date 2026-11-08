using Application.Interfaces.Services.User;
using EntityModels;
using Repository.interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using ViewModels.PublicUserProfile;
using ViewModels.MyUser;

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

        public async Task<MyUserProfileView> GetLoggedInProfileAsync()
        {
            string userId = currentUser.GetRequiredUserId();

            UserProfile? profile = await profileRepository.GetByUserIdAsync(userId);

            if (profile == null)
            {
                throw new InvalidOperationException("User profile could not be found.");
            }

            return new MyUserProfileView
            {
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                Address = profile.Address,
                PostalCode = profile.PostalCode,
                City = profile.City,
                ProfileText = profile.ProfileText
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
            profile.ProfileText = model.ProfileText;

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
                City = null,
                ProfileText= null
            };

            await profileRepository.AddAsync(profile);
        }

        public async Task<PublicUserProfileView> GetPublicProfileAsync(string userId, string userName)
        {
            UserProfile? profile = await profileRepository.GetByUserIdAsync(userId);

            if (profile == null)
            {
                throw new InvalidOperationException("User profile could not be found.");
            }

            return new PublicUserProfileView
            {
                UserId = userId,
                UserName = userName,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                City = profile.City,
                ProfileText = profile.ProfileText
            };
        }
    }
}
