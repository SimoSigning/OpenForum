using EntityModels;
using System;
using System.Collections.Generic;
using System.Text;
using ViewModels.Discover;

namespace Application.Interfaces.Mappers.Features
{
    public interface IDiscoverMapper
    {
        DiscoverTopicListView ToDiscoverTopicListView(Topic topic);

        DiscoverPostListView ToDiscoverPostListView(Post post);
    }
}
