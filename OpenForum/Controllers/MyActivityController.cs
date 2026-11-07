using Application.Interfaces.Services;
using Application.Interfaces.Services.Features;
using Application.Interfaces.Services.Forum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViewModels.Comment;
using ViewModels.MyActivity;
using ViewModels.Post;
using ViewModels.Topic;

namespace OpenForum.Controllers
{
    [Authorize]
    public class MyActivityController : Controller
    {
        private readonly IMyActivityService myActivityService;
        private readonly ITopicService topicService;
        private readonly IPostService postService;
        private readonly ICommentService commentService;

        public MyActivityController(
            IMyActivityService myActivityService,
            ITopicService topicService,
            IPostService postService,
            ICommentService commentService)
        {
            this.myActivityService = myActivityService;
            this.topicService = topicService;
            this.postService = postService;
            this.commentService = commentService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(ActivityFilter filter = ActivityFilter.All, int page = 1)
        {
            MyActivityIndexView model = await myActivityService.GetMyActivityIndexViewAsync(filter, page);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditTopic(
            int id,
            ActivityFilter filter = ActivityFilter.All,
            int page = 1)
        {
            try
            {
                TopicIndexView? result =
                    await topicService.GetEditViewAsync(id);

                if (result?.EditTopic == null)
                    return NotFound();

                ActivityTopicEditView model = new()
                {
                    Id = result.EditTopic.Id,
                    Title = result.EditTopic.Title
                };

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
        public async Task<IActionResult> UpdateTopic(
            ActivityTopicEditView model,
            ActivityFilter filter = ActivityFilter.All,
            int page = 1)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Filter"] = filter;
                ViewData["Page"] = page;
                return View("EditTopic", model);
            }

            try
            {
                TopicEditView topicModel = new()
                {
                    Id = model.Id,
                    Title = model.Title
                };

                await topicService.UpdateAsync(topicModel);

                return RedirectToAction(
                    nameof(Index),
                    new { filter, page });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTopic(
            int id,
            ActivityFilter filter = ActivityFilter.All,
            int page = 1)
        {
            try
            {
                await topicService.DeleteAsync(id);

                return RedirectToAction(
                    nameof(Index),
                    new { filter, page });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // -------------------------
        // Edit post
        // -------------------------

        [HttpGet]
        public async Task<IActionResult> EditPost(
            int id,
            ActivityFilter filter = ActivityFilter.All,
            int page = 1)
        {
            try
            {
                PostIndexView? result =
                    await postService.GetEditViewAsync(id);

                if (result?.EditPost == null)
                    return NotFound();

                ActivityPostEditView model = new()
                {
                    Id = result.EditPost.Id,
                    Title = result.EditPost.Title,
                    Content = result.EditPost.Content
                };

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
        public async Task<IActionResult> UpdatePost(
            ActivityPostEditView model,
            ActivityFilter filter = ActivityFilter.All,
            int page = 1)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Filter"] = filter;
                ViewData["Page"] = page;
                return View("EditPost", model);
            }

            try
            {
                PostEditView postModel = new()
                {
                    Id = model.Id,
                    Title = model.Title,
                    Content = model.Content
                };

                await postService.UpdateAsync(postModel);

                return RedirectToAction(
                    nameof(Index),
                    new { filter, page });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(
            int id,
            ActivityFilter filter = ActivityFilter.All,
            int page = 1)
        {
            try
            {
                await postService.DeleteAsync(id);

                return RedirectToAction(
                    nameof(Index),
                    new { filter, page });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // -------------------------
        // Edit comment or reply
        // -------------------------

        [HttpGet]
        public async Task<IActionResult> EditComment(
            int id,
            ActivityFilter filter = ActivityFilter.All,
            int page = 1)
        {
            try
            {
                CommentIndexView? result =
                    await commentService.GetEditViewAsync(id);

                if (result?.EditComment == null)
                    return NotFound();

                ActivityCommentEditView model = new()
                {
                    Id = result.EditComment.Id,
                    Title = result.EditComment.Title,
                    Content = result.EditComment.Content,
                    PostId = result.EditComment.PostId,
                    ParentCommentId = result.EditComment.ParentCommentId
                };

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
        public async Task<IActionResult> UpdateComment(
            ActivityCommentEditView model,
            ActivityFilter filter = ActivityFilter.All,
            int page = 1)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Filter"] = filter;
                ViewData["Page"] = page;
                return View("EditComment", model);
            }

            try
            {
                CommentEditView commentModel = new()
                {
                    Id = model.Id,
                    Title = model.Title,
                    Content = model.Content,
                    PostId = model.PostId,
                    ParentCommentId = model.ParentCommentId
                };

                await commentService.UpdateAsync(commentModel);

                return RedirectToAction(
                    nameof(Index),
                    new { filter, page });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(
            int id,
            ActivityFilter filter = ActivityFilter.All,
            int page = 1)
        {
            try
            {
                await commentService.DeleteAsync(id);

                return RedirectToAction(
                    nameof(Index),
                    new { filter, page });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}