using Application.Interfaces.Services;
using Identity.Models;
using Microsoft.AspNetCore.Identity;

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
    }
}