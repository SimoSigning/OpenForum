using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<string> GetUsernameByIdAsync(string userId);
    }
}
