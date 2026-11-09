using Application.Interfaces.Services.Features;
using Microsoft.AspNetCore.Mvc;

namespace OpenForum.ViewComponents
{
    public class UnreadMessagesViewComponent : ViewComponent
    {
        private readonly IMessageService messageService;

        public UnreadMessagesViewComponent(
            IMessageService messageService)
        {
            this.messageService = messageService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int unreadCount =
                await messageService.GetUnreadCountAsync();

            return View(unreadCount);
        }
    }
}