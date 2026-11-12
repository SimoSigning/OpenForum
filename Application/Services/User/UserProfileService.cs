
using Application.Interfaces.Services.Features;
using Application.Interfaces.Services.User;
using EntityModels;
using Repository.interfaces;
using ViewModels.Friends;
using ViewModels.MyUser;
using ViewModels.PublicUserProfile;

namespace Application.Services.User
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IUserProfileRepository profileRepository;
        private readonly ICurrentUserService currentUser;
        private readonly IAccountService accountService;
        private readonly IUserService userService;
        private readonly IFriendshipService friendshipService;

        public UserProfileService(
            IUserProfileRepository profileRepository,
            ICurrentUserService currentUser,
            IAccountService accountService,
            IUserService userService,
            IFriendshipService friendshipService)
        {
            this.profileRepository = profileRepository;
            this.currentUser = currentUser;
            this.accountService = accountService;
            this.userService = userService;
            this.friendshipService = friendshipService;
        }

        public async Task<MyProfilePageView> GetMyProfilePageAsync(UserProfileEditView? editProfile = null, ChangeEmailView? changeEmail = null, 
            ChangePasswordView? changePassword = null)
        {
            MyUserProfileView profile =await GetLoggedInProfileAsync();

            string email = await accountService.GetEmailAsync();

            return new MyProfilePageView
            {
                Profile = profile,

                EditProfile = editProfile ?? new UserProfileEditView
                {
                    FirstName = profile.FirstName,
                    LastName = profile.LastName,
                    Address = profile.Address,
                    PostalCode = profile.PostalCode,
                    City = profile.City,
                    ProfileText = profile.ProfileText
                },

                ChangeEmail = changeEmail ?? new ChangeEmailView(),

                ChangePassword = changePassword ?? new ChangePasswordView(),

                CurrentEmail = email
            };
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
                ProfileText = null
            };

            await profileRepository.AddAsync(profile);
        }

        public async Task<PublicUserProfileView> GetPublicProfilePageAsync(
            string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentException("A username is required.", nameof(userName));
            }

            string userId = await userService.GetUserIdByUsernameAsync(userName);

            PublicUserProfileView profile = await GetPublicProfileAsync(userId, userName);

            UserRelationshipView relationship = await friendshipService.GetRelationshipAsync(profile.UserId);

            profile.RelationshipStatus = relationship.RelationshipStatus;

            profile.CanSendFriendRequest = relationship.CanSendRequest;

            profile.FriendshipId = relationship.FriendshipId;

            return profile;
        }

        public async Task<PublicUserProfileView> GetPublicProfileAsync(string userId,string userName)
        {
            UserProfile? profile =await profileRepository.GetByUserIdAsync(userId);

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
