using Application.Interfaces.Services.Features;
using Application.Interfaces.Services.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace OpenForum.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService chatService;

        public ChatHub(IChatService chatService)
        {
            this.chatService = chatService;
        }

        public async Task JoinRoom(int roomId)
        {
            if (roomId <= 0)
                throw new ArgumentException("Invalid room ID.");

            // GetRoomAsync verifies membership and authorization.
            await chatService.GetRoomAsync(roomId);

            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                GetGroupName(roomId));
        }

        public async Task LeaveRoom(int roomId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                GetGroupName(roomId));
        }

        public async Task SendMessage(int roomId, string content)
        {
            if (roomId <= 0)
                throw new ArgumentException("Invalid room ID.");

            // The service validates membership, content and persistence.
            var message = await chatService.SendMessageAsync(
                roomId,
                content);

            await Clients.Group(GetGroupName(roomId))
                .SendAsync("ReceiveMessage", message);
        }

        private static string GetGroupName(int roomId)
        {
            return $"chatroom_{roomId}";
        }
    }
}