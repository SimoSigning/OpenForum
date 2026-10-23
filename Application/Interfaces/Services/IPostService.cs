using System;
using System.Collections.Generic;
using System.Text;
using ViewModels.Post;

namespace Application.Interfaces.Services
{
    public interface IPostService
    {
        Task<PostIndexView?> GetPostIndexViewAsync(int topicId, int page = 1);

        Task<PostIndexView?> GetEditViewAsync(int postId, int page = 1);

        Task CreateAsync(PostCreateView model);

        Task UpdateAsync(PostEditView model);

        Task DeleteAsync(int id);
    }
}
