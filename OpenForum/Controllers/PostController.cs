
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
        public async Task<IActionResult> Details(int id, int page = 1)
        {
            PostIndexView? model = await postService.GetPostIndexViewAsync(id, page);

            if (model == null)
                return NotFound();

            ViewData["CurrentPage"] = model.CurrentPage;

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id, int page = 1)
        {
            try
            {
                PostIndexView? model = await postService.GetEditViewAsync(id, page);

                if (model == null)
                    return NotFound();

                ViewData["CurrentPage"] = model.CurrentPage;

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
        public async Task<IActionResult> Create(PostCreateView model, int page = 1)
        {
            if (!ModelState.IsValid)
            {
                PostIndexView? viewModel = await postService.GetPostIndexViewAsync(model.TopicId, page);

                if (viewModel == null)
                    return NotFound();

                viewModel.CreatePost = model;

                ViewData["CurrentPage"] = viewModel.CurrentPage;

                return View("Details", viewModel);
            }

            try
            {
                await postService.CreateAsync(model);

                return RedirectToAction(
                    nameof(Details),
                    new { id = model.TopicId, page });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(PostEditView model, int page = 1)
        {
            if (!ModelState.IsValid)
            {
                try
                {
                    PostIndexView? viewModel =await postService.GetEditViewAsync(model.Id, page);

                    if (viewModel == null)
                        return NotFound();

                    viewModel.EditPost = model;

                    ViewData["CurrentPage"] = viewModel.CurrentPage;

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

                return RedirectToAction(nameof(Details), new { id = model.TopicId, page });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int topicId, int page = 1)
        {
            try
            {
                await postService.DeleteAsync(id);

                return RedirectToAction(nameof(Details), new { id = topicId, page });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}
