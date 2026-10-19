using Microsoft.AspNetCore.Mvc;

namespace OpenForum.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error")]
        public IActionResult Error()
        {
            return View();
        }
    }
}
