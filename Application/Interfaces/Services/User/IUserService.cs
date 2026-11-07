using Application.Models;
using System.Collections.Generic;

namespace Application.Interfaces.Services.User
{
    public interface IUserService
    {
        Task<string> GetUsernameByIdAsync(string userId);

        Task<Dictionary<string, string>> GetUsernamesByIdsAsync(IEnumerable<string> userIds);

        Task<(IEnumerable<UserSearchResult> Items, int TotalCount)> SearchUsersByUsernameAsync(string query, string excludedUserId, int skip, int take);

        Task<bool> UserExistsAsync(string userId);

        Task<string> GetEmailByIdAsync(string userId);

        Task ChangeEmailAsync(string userId, string newEmail);

        Task ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    }
}