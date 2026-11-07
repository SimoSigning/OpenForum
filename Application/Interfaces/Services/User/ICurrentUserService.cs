using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services.User
{
    public interface ICurrentUserService
    {
        string GetRequiredUserId();

        string? GetUserIdOrNull();
    }
}
