using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Contexts
{
    public interface IUserContext
    {
        string? UserId { get; }

        bool IsAuthenticated { get; }
    }
}
