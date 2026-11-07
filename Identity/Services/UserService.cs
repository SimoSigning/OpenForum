using Application.Interfaces.Services.User;
using Application.Models;
using Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Identity.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> userManager;

        public UserService(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        public async Task<string> GetUsernameByIdAsync(string userId)
        {
            ApplicationUser? user = await userManager.FindByIdAsync(userId);

            if (user?.UserName is not string username)
            {
                throw new InvalidOperationException($"Could not find a valid username for user '{userId}'.");
            }

            return username;
        }

        public async Task<Dictionary<string, string>> GetUsernamesByIdsAsync(IEnumerable<string> userIds)
        {
            List<string> ids = userIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            if (ids.Count == 0)
            {
                return new Dictionary<string, string>();
            }

            List<ApplicationUser> users = await userManager.Users
                .Where(user => ids.Contains(user.Id) && user.UserName != null)
                .ToListAsync();

            return users.ToDictionary(user => user.Id, user => user.UserName!);
        }

        public async Task<(IEnumerable<UserSearchResult> Items, int TotalCount)> SearchUsersByUsernameAsync(string query, string excludedUserId, int skip, int take)
        {
            if (string.IsNullOrWhiteSpace(query))
                return (Enumerable.Empty<UserSearchResult>(), 0);

            IQueryable<ApplicationUser> users = userManager.Users
                .Where(u =>
                    u.Id != excludedUserId &&
                    u.UserName != null &&
                    u.UserName.Contains(query));

            int totalCount = await users.CountAsync();

            List<UserSearchResult> items = await users
                .OrderBy(u => u.UserName)
                .Skip(skip)
                .Take(take)
                .Select(u => new UserSearchResult
                {
                    UserId = u.Id,
                    Username = u.UserName!
                })
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> UserExistsAsync(string userId)
        {
            ApplicationUser? user = await userManager.FindByIdAsync(userId);

            return user != null;
        }

        public async Task<string> GetEmailByIdAsync(string userId)
        {
            ApplicationUser? user = await userManager.FindByIdAsync(userId);

            if (user?.Email == null)
            {
                throw new InvalidOperationException("Could not find a valid email for the user.");
            }

            return user.Email;
        }

        public async Task ChangeEmailAsync(string userId, string newEmail)
        {
            ApplicationUser? user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            string token = await userManager.GenerateChangeEmailTokenAsync(user, newEmail);

            IdentityResult result = await userManager.ChangeEmailAsync(user, newEmail, token);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        public async Task ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            ApplicationUser? user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            IdentityResult result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}