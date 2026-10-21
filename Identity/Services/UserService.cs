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

        public async Task<string?> GetUsernameByIdAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            return user?.UserName;
        }
    }
}