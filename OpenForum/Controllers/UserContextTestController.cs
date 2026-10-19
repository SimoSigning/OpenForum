using Application.Interfaces.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OpenForum.Controllers
{
    [Authorize]
    public class UserContextTestController : Controller
    {
        private readonly IUserContext userContext;

        public UserContextTestController(IUserContext userContext)
        {
            this.userContext = userContext;
        }

        public IActionResult Index()
        {
            ViewBag.UserId = userContext.UserId;
            ViewBag.IsAuthenticated = userContext.IsAuthenticated;

            return View();
        }
    }
}
