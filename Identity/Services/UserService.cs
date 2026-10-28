using Application.Interfaces.Services;
using Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;

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
                throw new InvalidOperationException(
                    $"Could not find a valid username for user '{userId}'.");
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
                .Where(user => ids.Contains(user.Id)
                    && user.UserName != null)
                .ToListAsync();

            return users.ToDictionary(
                user => user.Id,
                user => user.UserName!);
        }
    }
}