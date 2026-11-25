
using Application.Interfaces.Services.Features;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OpenForum.Services;
using ViewModels.Chat;

namespace OpenForum.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService chatService;
        private readonly ChatConnectionTracker connectionTracker;

        public ChatHub(
            IChatService chatService,
            ChatConnectionTracker connectionTracker)
        {
            this.chatService = chatService;
            this.connectionTracker = connectionTracker;
        }

        public override async Task OnConnectedAsync()
        {
            string? userId = Context.UserIdentifier;

            if (!string.IsNullOrEmpty(userId))
            {
                connectionTracker.AddConnection(userId, Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string? userId = Context.UserIdentifier;

            if (!string.IsNullOrEmpty(userId))
            {
                // Atomically remove this connection and find rooms
                // where this was the user's last active connection.
                List<int> roomsToCleanUp = connectionTracker.RemoveConnectionAndGetLastRooms(userId, Context.ConnectionId);

                foreach (int roomId in roomsToCleanUp)
                {
                    // The service will only remove membership from
                    // Miscellaneous and Topic rooms.
                    await chatService.RemoveTemporaryMembershipAsync(roomId, userId);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinRoom(int roomId)
        {
            if (roomId <= 0)
                throw new ArgumentException("Invalid room ID.");

            await chatService.GetRoomAsync(roomId);

            connectionTracker.AddRoom(Context.ConnectionId, roomId);

            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(roomId));
            }
            catch
            {
                connectionTracker.RemoveRoom(Context.ConnectionId, roomId);
                throw;
            }
        }

        public async Task LeaveRoom(int roomId)
        {
            if (roomId <= 0)
                throw new ArgumentException("Invalid room ID.");

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(roomId));

            connectionTracker.RemoveRoom(Context.ConnectionId, roomId);
        }

        public async Task SendMessage(int roomId, string content)
        {
            if (roomId <= 0)
                throw new ArgumentException("Invalid room ID.");

            ChatMessageView message = await chatService.SendMessageAsync(roomId, content);

            await Clients.Group(GetGroupName(roomId)).SendAsync("ReceiveMessage", message);
        }

        public static string GetGroupName(int roomId)
        {
            return $"chatroom_{roomId}";
        }
    }
}
