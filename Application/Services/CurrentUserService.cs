using Application.Interfaces.Contexts;
using Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IUserContext userContext;

        public CurrentUserService(IUserContext userContext)
        {
            this.userContext = userContext;
        }

        public string GetRequiredUserId()
        {
            string? userId = userContext.UserId;

            if (!userContext.IsAuthenticated || string.IsNullOrWhiteSpace(userId))
            {
                throw new UnauthorizedAccessException("An authenticated user is required.");
            }

            return userId;
        }

        public string? GetUserIdOrNull()
        {
            string? userId = userContext.UserId;

            if (!userContext.IsAuthenticated || string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }

            return userId;
        }
    }
}
