using System;
using System.Collections.Generic;
using System.Text;
using ViewModels.Discover;

namespace Application.Interfaces.Services.Features
{
    public interface IDiscoverService
    {
        Task<IEnumerable<DiscoverTopicListView>> GetPopularTopicsAsync();

        Task<IEnumerable<DiscoverPostListView>> GetPopularPostsAsync();

        Task<IEnumerable<DiscoverTopicListView>> SearchTopicListAsync(string query);
    }
}
