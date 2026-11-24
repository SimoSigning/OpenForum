using Application.Interfaces.Services.Features;
using Microsoft.AspNetCore.Mvc;

namespace OpenForum.ViewComponents
{
    public class UnseenChatRoomsViewComponent : ViewComponent
    {
        private readonly IChatService chatService;

        public UnseenChatRoomsViewComponent(
            IChatService chatService)
        {
            this.chatService = chatService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int unseenCount =
                await chatService.GetUnseenPrivateRoomCountAsync();

            return View(unseenCount);
        }
    }
}