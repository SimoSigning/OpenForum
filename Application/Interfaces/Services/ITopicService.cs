using System;
using System.Collections.Generic;
using System.Text;
using ViewModels.Topic;

namespace Application.Interfaces.Services
{
    public interface ITopicService
    {
        Task<TopicIndexView> GetTopicIndexViewAsync();

        Task<TopicIndexView?> GetEditViewAsync(int id);

        Task CreateAsync(TopicCreateView model);

        Task UpdateAsync(TopicEditView model);

        Task DeleteAsync(int id);
    }
}
