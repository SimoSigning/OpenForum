
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViewModels.Comment;

namespace OpenForum.Controllers
{
    public class CommentController : Controller
    {
        private readonly ICommentService commentService;

        public CommentController(ICommentService commentService)
        {
            this.commentService = commentService;
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, int page = 1)
        {
            // id is the post ID.
            CommentIndexView? model =
                await commentService.GetCommentIndexViewAsync(id, page);

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
                // id is the comment ID.
                CommentIndexView? model =
                    await commentService.GetEditViewAsync(id, page);

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
        public async Task<IActionResult> Create(
            CommentCreateView model,
            int page = 1)
        {
            if (!ModelState.IsValid)
            {
                CommentIndexView? viewModel =
                    await commentService.GetCommentIndexViewAsync(
                        model.PostId, page);

                if (viewModel == null)
                    return NotFound();

                viewModel.CreateComment = model;

                ViewData["CurrentPage"] = viewModel.CurrentPage;

                return View("Details", viewModel);
            }

            try
            {
                await commentService.CreateAsync(model);

                return RedirectToAction(
                    nameof(Details),
                    new { id = model.PostId, page });
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
            CommentEditView model,
            int page = 1)
        {
            if (!ModelState.IsValid)
            {
                try
                {
                    CommentIndexView? viewModel =
                        await commentService.GetEditViewAsync(
                            model.Id, page);

                    if (viewModel == null)
                        return NotFound();

                    viewModel.EditComment = model;

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
                await commentService.UpdateAsync(model);

                return RedirectToAction(
                    nameof(Details),
                    new { id = model.PostId, page });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id,
            int postId,
            int page = 1)
        {
            try
            {
                await commentService.DeleteAsync(id);

                return RedirectToAction(
                    nameof(Details),
                    new { id = postId, page });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}
