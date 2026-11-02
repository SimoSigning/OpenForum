using Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUserService userService;
        private readonly ICurrentUserService currentUser;

        public AccountService(
            IUserService userService,
            ICurrentUserService currentUser)
        {
            this.userService = userService;
            this.currentUser = currentUser;
        }

        public async Task<string> GetEmailAsync()
        {
            string userId = currentUser.GetRequiredUserId();

            return await userService.GetEmailByIdAsync(userId);
        }

        public async Task ChangeEmailAsync(string newEmail)
        {
            string userId = currentUser.GetRequiredUserId();

            await userService.ChangeEmailAsync(userId, newEmail);
        }

        public async Task ChangePasswordAsync(string currentPassword, string newPassword)
        {
            string userId = currentUser.GetRequiredUserId();

            await userService.ChangePasswordAsync(userId, currentPassword, newPassword);
        }
    }

}
