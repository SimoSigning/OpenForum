
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
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
            PostIndexView? model = await postService.GetPostIndexViewAsync(id);

            if (model == null)
                return NotFound();

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                PostIndexView? model = await postService.GetEditViewAsync(id);

                if (model == null)
                    return NotFound();

                return View("Details", model);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PostCreateView model)
        {
            if (!ModelState.IsValid)
            {
                PostIndexView? viewModel = await postService.GetPostIndexViewAsync(model.TopicId);

                if (viewModel == null)
                    return NotFound();

                viewModel.CreatePost = model;

                return View("Details", viewModel);
            }

            try
            {
                await postService.CreateAsync(model);

                return RedirectToAction(
                    nameof(Details),
                    new { id = model.TopicId });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(PostEditView model)
        {
            if (!ModelState.IsValid)
            {
                try
                {
                    PostIndexView? viewModel = await postService.GetEditViewAsync(model.Id);

                    if (viewModel == null)
                        return NotFound();

                    viewModel.EditPost = model;

                    return View("Details", viewModel);
                }
                catch (UnauthorizedAccessException)
                {
                    return Forbid();
                }
            }

            try
            {
                await postService.UpdateAsync(model);

                return RedirectToAction(
                    nameof(Details),
                    new { id = model.TopicId });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int topicId)
        {
            try
            {
                await postService.DeleteAsync(id);

                return RedirectToAction(
                    nameof(Details),
                    new { id = topicId });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}
