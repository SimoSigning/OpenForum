
using Application.Interfaces.Services;
using Application.Interfaces.Services.Features;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViewModels.MyActivity;

namespace OpenForum.Controllers
{
    [Authorize]
    public class MyActivityController : Controller
    {
        private readonly IMyActivityService myActivityService;

        public MyActivityController(IMyActivityService myActivityService)
        {
            this.myActivityService = myActivityService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(ActivityFilter filter = ActivityFilter.All, int page = 1)
        {
            MyActivityIndexView model = await myActivityService.GetMyActivityIndexViewAsync(filter, page);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditTopic(int id, ActivityFilter filter = ActivityFilter.All, int page = 1)
        {
            try
            {
                ActivityTopicEditView? model = await myActivityService.GetEditTopicViewAsync(id);

                if (model == null)
                    return NotFound();

                ViewData["Filter"] = filter;
                ViewData["Page"] = page;

                return View(model);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTopic(ActivityTopicEditView model, ActivityFilter filter = ActivityFilter.All, int page = 1)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Filter"] = filter;
                ViewData["Page"] = page;

                return View("EditTopic", model);
            }

            try
            {
                await myActivityService.UpdateTopicAsync(model);

                return RedirectToAction(nameof(Index), new { filter, page });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTopic(int id, ActivityFilter filter = ActivityFilter.All, int page = 1)
        {
            try
            {
                await myActivityService.DeleteTopicAsync(id);

                return RedirectToAction(nameof(Index), new { filter, page });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditPost(int id, ActivityFilter filter = ActivityFilter.All, int page = 1)
        {
            try
            {
                ActivityPostEditView? model = await myActivityService.GetEditPostViewAsync(id);

                if (model == null)
                    return NotFound();

                ViewData["Filter"] = filter;
                ViewData["Page"] = page;

                return View(model);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePost(ActivityPostEditView model, ActivityFilter filter = ActivityFilter.All, int page = 1)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Filter"] = filter;
                ViewData["Page"] = page;

                return View("EditPost", model);
            }

            try
            {
                await myActivityService.UpdatePostAsync(model);

                return RedirectToAction(nameof(Index), new { filter, page });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int id, ActivityFilter filter = ActivityFilter.All, int page = 1)
        {
            try
            {
                await myActivityService.DeletePostAsync(id);

                return RedirectToAction(nameof(Index), new { filter, page });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }


        [HttpGet]
        public async Task<IActionResult> EditComment(int id, ActivityFilter filter = ActivityFilter.All, int page = 1)
        {
            try
            {
                ActivityCommentEditView? model = await myActivityService.GetEditCommentViewAsync(id);

                if (model == null)
                    return NotFound();

                ViewData["Filter"] = filter;
                ViewData["Page"] = page;

                return View(model);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateComment(ActivityCommentEditView model, ActivityFilter filter = ActivityFilter.All, int page = 1)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Filter"] = filter;
                ViewData["Page"] = page;

                return View("EditComment", model);
            }

            try
            {
                await myActivityService.UpdateCommentAsync(model);

                return RedirectToAction(nameof(Index), new { filter, page });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int id, ActivityFilter filter = ActivityFilter.All, int page = 1)
        {
            try
            {
                await myActivityService.DeleteCommentAsync(id);

                return RedirectToAction(nameof(Index), new { filter, page });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}
