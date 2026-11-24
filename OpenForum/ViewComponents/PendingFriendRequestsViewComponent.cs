using Application.Interfaces.Services.Features;
using Microsoft.AspNetCore.Mvc;

namespace OpenForum.ViewComponents
{
    public class PendingFriendRequestsViewComponent : ViewComponent
    {
        private readonly IFriendshipService friendshipService;

        public PendingFriendRequestsViewComponent(
            IFriendshipService friendshipService)
        {
            this.friendshipService = friendshipService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int pendingCount =
                await friendshipService.GetIncomingPendingRequestCountAsync();

            return View(pendingCount);
        }
    }
}