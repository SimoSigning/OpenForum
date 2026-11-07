using Application.Interfaces.Services.Features;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViewModels.Discover;

namespace OpenForum.Controllers
{
    public class DiscoverController : Controller
    {
        private readonly IDiscoverService discoverService;

        public DiscoverController(IDiscoverService discoverService)
        {
            this.discoverService = discoverService;
        }

        [HttpGet]
        public async Task<IActionResult> PopularTopics()
        {
            IEnumerable<DiscoverTopicListView> model = await discoverService.GetPopularTopicsAsync();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> PopularPosts()
        {
            IEnumerable<DiscoverPostListView> model = await discoverService.GetPopularPostsAsync();

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Search(string? query)
        {
            IEnumerable<DiscoverTopicListView> model =
                Enumerable.Empty<DiscoverTopicListView>();

            if (!string.IsNullOrWhiteSpace(query))
            {
                model = await discoverService.SearchTopicListAsync(query);
            }

            return View(model);
        }
    }
}