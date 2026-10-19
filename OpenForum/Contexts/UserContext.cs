using Application.Interfaces;
using Application.Interfaces.Contexts;
using System.Security.Claims;

namespace OpenForum.Services
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor httpContextAccessor;


        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }


        public string? UserId =>
            httpContextAccessor.HttpContext?
                .User?
                .FindFirstValue(ClaimTypes.NameIdentifier);


        public bool IsAuthenticated =>
            httpContextAccessor.HttpContext?
                .User?
                .Identity?
                .IsAuthenticated ?? false;
    }
}