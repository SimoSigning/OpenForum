using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services.User
{
    public interface IAccountService
    {
        Task<string> GetEmailAsync();

        Task ChangeEmailAsync(string newEmail);

        Task ChangePasswordAsync(string currentPassword, string newPassword);
    }
}
