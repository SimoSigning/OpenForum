using Application.Interfaces.Services;
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
            var model = await commentService.GetPostDetailsViewAsync(id);

            if (model == null)
                return NotFound();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await commentService.GetEditViewAsync(id);

            if (model == null)
                return NotFound();

            return View("Details", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CommentCreateView model)
        {
            if (!ModelState.IsValid)
            {
                var viewModel = await commentService.GetPostDetailsViewAsync(model.PostId);

                if (viewModel == null)
                    return NotFound();

                viewModel.CreateComment = model;

                return View("Details", viewModel);
            }

            await commentService.CreateAsync(model);

            return RedirectToAction(nameof(Details), new { id = model.PostId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(CommentEditView model)
        {
            if (!ModelState.IsValid)
            {
                var viewModel = await commentService.GetEditViewAsync(model.Id);

                if (viewModel == null)
                    return NotFound();

                viewModel.EditComment = model;

                return View("Details", viewModel);
            }

            await commentService.UpdateAsync(model);

            return RedirectToAction(nameof(Details), new { id = model.PostId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int postId)
        {
            await commentService.DeleteAsync(id);

            return RedirectToAction(nameof(Details), new { id = postId });
        }
    }
}
