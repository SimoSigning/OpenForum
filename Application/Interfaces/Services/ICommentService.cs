using System;
using System.Collections.Generic;
using System.Text;
using ViewModels.Comment;
using ViewModels.Post;

namespace Application.Interfaces.Services
{
    public interface ICommentService
    {
        Task<PostDetailsView?> GetPostDetailsViewAsync(int postId);

        Task<PostDetailsView?> GetEditViewAsync(int commentId);

        Task CreateAsync(CommentCreateView model);

        Task UpdateAsync(CommentEditView model);

        Task DeleteAsync(int id);
    }
}
