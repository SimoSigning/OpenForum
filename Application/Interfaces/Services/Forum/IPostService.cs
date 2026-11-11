using System;
using System.Collections.Generic;
using System.Text;
using ViewModels.Post;

namespace Application.Interfaces.Services.Forum
{
    public interface IPostService
    {
        Task<PostIndexView?> GetPostIndexViewAsync(int topicId, int page = 1, PostCreateView? createPost = null);

        Task<PostIndexView?> GetEditViewAsync(int postId, int page = 1, PostEditView? editPost = null);

        Task CreateAsync(PostCreateView model);

        Task UpdateAsync(PostEditView model);

        Task DeleteAsync(int id);
    }
}
