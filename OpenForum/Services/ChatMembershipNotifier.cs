using Application.Interfaces.Services.Features;
using Microsoft.AspNetCore.SignalR;
using OpenForum.Hubs;

namespace OpenForum.Services
{
    public class ChatMembershipNotifier : IChatMembershipNotifier
    {
        private readonly IHubContext<ChatHub> hubContext;

        public ChatMembershipNotifier(IHubContext<ChatHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        public async Task NotifyMembersChangedAsync(int roomId)
        {
            await hubContext.Clients.Group(ChatHub.GetGroupName(roomId)).SendAsync("RoomMembersChanged", roomId);
        }
    }
}