
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViewModels.Topic;

namespace OpenForum.Controllers
{
    public class TopicController : Controller
    {
        private readonly ITopicService topicService;

        public TopicController(ITopicService topicService)
        {
            this.topicService = topicService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            TopicIndexView model =
                await topicService.GetTopicIndexViewAsync(page);

            ViewData["CurrentPage"] = model.CurrentPage;

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id, int page = 1)
        {
            try
            {
                TopicIndexView? model =
                    await topicService.GetEditViewAsync(id, page);

                if (model == null)
                    return NotFound();

                ViewData["CurrentPage"] = model.CurrentPage;

                return View("Index", model);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
        TopicCreateView model,
        int page = 1)
        {
            if (!ModelState.IsValid)
            {
                TopicIndexView viewModel =
                    await topicService.GetTopicIndexViewAsync(page);

                viewModel.CreateTopic = model;

                ViewData["CurrentPage"] = viewModel.CurrentPage;

                return View("Index", viewModel);
            }

            try
            {
                await topicService.CreateAsync(model);

                return RedirectToAction(nameof(Index), new { page });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
    TopicEditView model,
    int page = 1)
        {
            if (!ModelState.IsValid)
            {
                try
                {
                    TopicIndexView? viewModel =
                        await topicService.GetEditViewAsync(model.Id, page);

                    if (viewModel == null)
                        return NotFound();

                    viewModel.EditTopic = model;

                    ViewData["CurrentPage"] = viewModel.CurrentPage;

                    return View("Index", viewModel);
                }
                catch (UnauthorizedAccessException)
                {
                    return Forbid();
                }
            }

            try
            {
                await topicService.UpdateAsync(model);

                return RedirectToAction(nameof(Index), new { page });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int page = 1)
        {
            try
            {
                await topicService.DeleteAsync(id);

                return RedirectToAction(nameof(Index), new { page });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}
