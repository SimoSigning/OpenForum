using System;
using System.Collections.Generic;
using System.Text;
using ViewModels.Post;

namespace Application.Interfaces.Services
{
    public interface IPostService
    {
        Task<PostIndexView?> GetPostIndexViewAsync(int topicId);

        Task<PostIndexView?> GetEditViewAsync(int postId);

        Task CreateAsync(PostCreateView model);

        Task UpdateAsync(PostEditView model);

        Task DeleteAsync(int id);
    }
}
