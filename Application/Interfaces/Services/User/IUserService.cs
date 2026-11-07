using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services.User
{
    public interface IUserService
    {
        Task<string> GetUsernameByIdAsync(string userId);

        Task<Dictionary<string, string>> GetUsernamesByIdsAsync(
            IEnumerable<string> userIds);

        Task<string> GetEmailByIdAsync(string userId);

        Task ChangeEmailAsync(string userId, string newEmail);

        Task ChangePasswordAsync(
            string userId,
            string currentPassword,
            string newPassword);
    }
}
