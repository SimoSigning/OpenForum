
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
            // A top-level comment requires a title.
            if (!model.ParentCommentId.HasValue &&
                string.IsNullOrWhiteSpace(model.Title))
            {
                ModelState.AddModelError(
                    nameof(model.Title),
                    "A title is required for a comment.");
            }

            if (!ModelState.IsValid)
            {
                return await ReturnCreateViewAsync(model, page);
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
            catch (ArgumentException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The comment or reply target is invalid.");

                return await ReturnCreateViewAsync(model, page);
            }
        }

        private async Task<IActionResult> ReturnCreateViewAsync(
            CommentCreateView submittedModel,
            int page)
        {
            CommentIndexView? viewModel =
                await commentService.GetCommentIndexViewAsync(
                    submittedModel.PostId,
                    page);

            if (viewModel == null)
                return NotFound();

            // Preserve top-level form values only for top-level comments.
            // Reply submissions belong to their inline reply form.
            if (!submittedModel.ParentCommentId.HasValue)
            {
                viewModel.CreateComment = submittedModel;
            }

            // Let Details.cshtml identify which reply form was submitted.
            ViewData["ReplyParentCommentId"] =
                submittedModel.ParentCommentId;

            ViewData["CurrentPage"] = viewModel.CurrentPage;

            ViewData["ReplyContent"] = submittedModel.Content;
            ViewData["ReplyError"] = ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage)
                .FirstOrDefault(message => !string.IsNullOrWhiteSpace(message));

            return View("Details", viewModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            CommentEditView model,
            int page = 1)
        {
            if (!model.Id.Equals(0) &&
                string.IsNullOrWhiteSpace(model.Title))
            {
                // The title requirement depends on whether the existing
                // comment is top-level or a reply. The service retains
                // the existing hierarchy and updates the content.
                // Full conditional title validation can be refined once
                // the edit form is updated.
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    CommentIndexView? viewModel =
                        await commentService.GetEditViewAsync(model.Id, page);

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
