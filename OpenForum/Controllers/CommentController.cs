
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
        public async Task<IActionResult> Details(int id)
        {
            CommentIndexView? model = await commentService.GetCommentIndexViewAsync(id);

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
                CommentIndexView? model = await commentService.GetEditViewAsync(id);

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
        public async Task<IActionResult> Create(CommentCreateView model)
        {
            if (!ModelState.IsValid)
            {
                CommentIndexView? viewModel = await commentService.GetCommentIndexViewAsync(model.PostId);

                if (viewModel == null)
                    return NotFound();

                viewModel.CreateComment = model;

                return View("Details", viewModel);
            }

            try
            {
                await commentService.CreateAsync(model);

                return RedirectToAction(
                    nameof(Details),
                    new { id = model.PostId });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(CommentEditView model)
        {
            if (!ModelState.IsValid)
            {
                try
                {
                    CommentIndexView? viewModel = await commentService.GetEditViewAsync(model.Id);

                    if (viewModel == null)
                        return NotFound();

                    viewModel.EditComment = model;

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
                    new { id = model.PostId });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int postId)
        {
            try
            {
                await commentService.DeleteAsync(id);

                return RedirectToAction(
                    nameof(Details),
                    new { id = postId });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}
