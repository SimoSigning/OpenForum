using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Contexts
{
    public interface IUserContext
    {
        string? UserId { get; }

        string? UserName { get; }

        bool IsAuthenticated { get; }
    }
}
