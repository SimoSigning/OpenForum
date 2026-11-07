
using Application.Interfaces.Services.Features;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViewModels.Friends;

namespace OpenForum.Controllers
{
    [Authorize]
    public class FriendshipController : Controller
    {
        private readonly IFriendshipService friendshipService;

        public FriendshipController(IFriendshipService friendshipService)
        {
            this.friendshipService = friendshipService;
        }

        [HttpGet]
        public async Task<IActionResult> Search(string? query, int page = 1)
        {
            UserSearchView viewModel = await friendshipService.SearchUsersAsync(query ?? string.Empty, page);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendRequest(string addresseeUserId, string? query)
        {
            try
            {
                await friendshipService.SendRequestAsync(addresseeUserId);
                TempData["SuccessMessage"] = "Friend request sent.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (KeyNotFoundException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Search), new { query });
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            FriendsIndexView viewModel = await friendshipService.GetFriendsIndexViewAsync();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptRequest(int friendshipId)
        {
            try
            {
                await friendshipService.AcceptRequestAsync(friendshipId);
                TempData["SuccessMessage"] = "Friend request accepted.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (UnauthorizedAccessException)
            {
                TempData["ErrorMessage"] = "You are not allowed to accept this request.";
            }
            catch (KeyNotFoundException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeclineRequest(int friendshipId)
        {
            try
            {
                await friendshipService.DeclineRequestAsync(friendshipId);
                TempData["SuccessMessage"] = "Friend request declined.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (UnauthorizedAccessException)
            {
                TempData["ErrorMessage"] = "You are not allowed to decline this request.";
            }
            catch (KeyNotFoundException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFriend(int friendshipId)
        {
            try
            {
                await friendshipService.RemoveFriendAsync(friendshipId);
                TempData["SuccessMessage"] = "Friend removed.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (UnauthorizedAccessException)
            {
                TempData["ErrorMessage"] = "You are not allowed to remove this friendship.";
            }
            catch (KeyNotFoundException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
