using System;
using System.Collections.Generic;
using System.Text;
using ViewModels.Topic;

namespace Application.Interfaces.Services
{
    public interface ITopicService
    {
        Task<TopicIndexView> GetTopicIndexViewAsync(int page = 1);

        Task<TopicIndexView?> GetEditViewAsync(int id, int page = 1);

        Task CreateAsync(TopicCreateView model);

        Task UpdateAsync(TopicEditView model);

        Task DeleteAsync(int id);
    }
}
