
using Application.Interfaces.Services.Features;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OpenForum.Hubs;
using OpenForum.Services;
using System.Security.Claims;
using ViewModels.Chat;

namespace OpenForum.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IChatService chatService;
        private readonly IHubContext<ChatHub> chatHub;
        private readonly ChatConnectionTracker connectionTracker;

        public ChatController(
            IChatService chatService,
            IHubContext<ChatHub> chatHub,
            ChatConnectionTracker connectionTracker)
        {
            this.chatService = chatService;
            this.chatHub = chatHub;
            this.connectionTracker = connectionTracker;
        }

        // --------------------------------
        // Chat interface
        // --------------------------------

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // --------------------------------
        // Room retrieval
        // --------------------------------

        [HttpGet]
        public async Task<IActionResult> MyRooms()
        {
            try
            {
                IEnumerable<ChatRoomView> rooms = await chatService.GetMyRoomsAsync();

                return Json(rooms);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet]
        public async Task<IActionResult> MiscellaneousRooms()
        {
            try
            {
                IEnumerable<ChatRoomView> rooms = await chatService.GetMiscellaneousRoomsAsync();

                return Json(rooms);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Room(int roomId)
        {
            try
            {
                ChatRoomView room = await chatService.GetRoomAsync(roomId);

                return Json(room);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet]
        public async Task<IActionResult> RoomMembers(int roomId)
        {
            try
            {
                IEnumerable<ChatMemberView> members = await chatService.GetRoomMembersAsync(roomId);

                return Json(members);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Messages(int roomId)
        {
            try
            {
                IEnumerable<ChatMessageView> messages = await chatService.GetLatestMessagesAsync(roomId);

                return Json(messages);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }


        [HttpGet]
        public async Task<IActionResult> OpenRoom(int roomId)
        {
            try
            {
                ChatRoomView room = await chatService.GetRoomAsync(roomId);

                return View("Room", room);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }


        // --------------------------------
        // Private rooms
        // --------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePrivateRoom(string name, IEnumerable<string> invitedUserIds)
        {
            try
            {
                int roomId = await chatService.CreatePrivateRoomAsync(name, invitedUserIds);

                return Json(new { roomId });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Friends()
        {
            try
            {
                var friends = await chatService.GetFriendsForChatAsync();

                return Json(friends);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet]
        public async Task<IActionResult> AvailableFriendsToInvite(int roomId)
        {
            try
            {
                var friends = await chatService.GetAvailableFriendsToInviteAsync(roomId);

                return Json(friends);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InviteFriend(int roomId, string invitedUserId)
        {
            try
            {
                await chatService.InviteFriendAsync(roomId, invitedUserId);

                return Ok();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // --------------------------------
        // Membership
        // --------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> JoinRoom(int roomId)
        {
            try
            {
                await chatService.JoinMiscellaneousRoomAsync(roomId);

                return Ok();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveRoom(int roomId)
        {
            try
            {
                // Identify the authenticated user.
                string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                    return Forbid();

                // First remove the user's database membership.
                await chatService.LeaveRoomAsync(roomId);

                // Find every active SignalR connection belonging to this user.
                IReadOnlyList<string> connections = connectionTracker.GetConnections(userId);

                string groupName = ChatHub.GetGroupName(roomId);

                foreach (string connectionId in connections)
                {
                    // Remove the connection from the SignalR group.
                    await chatHub.Groups.RemoveFromGroupAsync(connectionId, groupName);

                    // Also update our connection tracker.
                    connectionTracker.RemoveRoom(connectionId, roomId);
                }

                return Ok();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        // --------------------------------
        // Topic rooms
        // --------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TopicRoom(int topicId)
        {
            try
            {
                int roomId = await chatService.GetOrCreateTopicRoomAsync(topicId);

                return Json(new { roomId });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> EnterTopicRoom(int topicId)
        {
            try
            {
                int roomId = await chatService.GetOrCreateTopicRoomAsync(topicId);

                return RedirectToAction(nameof(OpenRoom), new { roomId });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
        }
    }
}
