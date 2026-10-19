using Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Services
{
    public class IdentitySeeder
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration configuration;

        public IdentitySeeder(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.configuration = configuration;
        }


        public async Task SeedAsync()
        {
            await CreateRoleIfNotExistsAsync("Admin");
            await CreateRoleIfNotExistsAsync("User");

            await CreateAdminUserAsync();
        }


        private async Task CreateRoleIfNotExistsAsync(string roleName)
        {
            if (await roleManager.RoleExistsAsync(roleName))
                return;


            var result = await roleManager.CreateAsync(
                new IdentityRole(roleName));


            if (!result.Succeeded)
            {
                var errors = string.Join(
                    Environment.NewLine,
                    result.Errors.Select(e =>
                        $"{e.Code}: {e.Description}"));


                throw new InvalidOperationException(
                    $"Failed to create role '{roleName}'.{Environment.NewLine}{errors}");
            }
        }


        private async Task CreateAdminUserAsync()
        {
            var email = configuration["AdminUser:Email"];
            var password = configuration["AdminUser:Password"];


            if (string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException(
                    "Admin user configuration is missing.");
            }


            var existingUser = await userManager.FindByEmailAsync(email);


            if (existingUser != null)
                return;


            var adminUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };


            var result = await userManager.CreateAsync(
                adminUser,
                password);


            if (!result.Succeeded)
            {
                var errors = string.Join(
                    Environment.NewLine,
                    result.Errors.Select(e =>
                        $"{e.Code}: {e.Description}"));


                throw new InvalidOperationException(
                    $"Failed to create admin user.{Environment.NewLine}{errors}");
            }


            await userManager.AddToRoleAsync(
                adminUser,
                "Admin");
        }
    }
}
