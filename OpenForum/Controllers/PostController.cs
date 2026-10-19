using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using ViewModels.Post;

namespace OpenForum.Controllers
{
    public class PostController : Controller
    {
        private readonly IPostService postService;

        public PostController(IPostService postService)
        {
            this.postService = postService;
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var model = await postService.GetTopicDetailsViewAsync(id);

            if (model == null)
                return NotFound();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await postService.GetEditViewAsync(id);

            if (model == null)
                return NotFound();

            return View("Details", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PostCreateView model)
        {
            if (!ModelState.IsValid)
            {
                var viewModel = await postService.GetTopicDetailsViewAsync(model.TopicId);

                if (viewModel == null)
                    return NotFound();

                viewModel.CreatePost = model;

                return View("Details", viewModel);
            }

            await postService.CreateAsync(model);

            return RedirectToAction(nameof(Details), new { id = model.TopicId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(PostEditView model)
        {
            if (!ModelState.IsValid)
            {
                var viewModel = await postService.GetEditViewAsync(model.Id);

                if (viewModel == null)
                    return NotFound();

                viewModel.EditPost = model;

                return View("Details", viewModel);
            }

            await postService.UpdateAsync(model);

            return RedirectToAction(nameof(Details), new { id = model.TopicId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int topicId)
        {
            await postService.DeleteAsync(id);

            return RedirectToAction(nameof(Details), new { id = topicId });
        }
    }
}
