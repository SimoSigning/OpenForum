
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
        public async Task<IActionResult> Index()
        {
            var model = await topicService.GetTopicIndexViewAsync();

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var model = await topicService.GetEditViewAsync(id);

                if (model == null)
                    return NotFound();

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
        public async Task<IActionResult> Create(TopicCreateView model)
        {
            if (!ModelState.IsValid)
            {
                var viewModel = await topicService.GetTopicIndexViewAsync();
                viewModel.CreateTopic = model;

                return View("Index", viewModel);
            }

            try
            {
                await topicService.CreateAsync(model);
                return RedirectToAction(nameof(Index));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(TopicEditView model)
        {
            if (!ModelState.IsValid)
            {
                try
                {
                    var viewModel = await topicService.GetEditViewAsync(model.Id);

                    if (viewModel == null)
                        return NotFound();

                    viewModel.EditTopic = model;

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
                return RedirectToAction(nameof(Index));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await topicService.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}
