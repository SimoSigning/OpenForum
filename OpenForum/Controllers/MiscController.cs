using Microsoft.AspNetCore.Mvc;

namespace OpenForum.Controllers
{
    public class MiscController : Controller
    {
        public IActionResult About()
        {
            return View();
        }
    }
}
