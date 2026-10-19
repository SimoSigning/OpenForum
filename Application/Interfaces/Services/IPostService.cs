using System;
using System.Collections.Generic;
using System.Text;
using ViewModels.Post;
using ViewModels.Topic;

namespace Application.Interfaces.Services
{
    public interface IPostService
    {
        Task<TopicDetailsView?> GetTopicDetailsViewAsync(int topicId);

        Task<TopicDetailsView?> GetEditViewAsync(int postId);

        Task CreateAsync(PostCreateView model);

        Task UpdateAsync(PostEditView model);

        Task DeleteAsync(int id);
    }
}
