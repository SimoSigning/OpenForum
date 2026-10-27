using System;
using System.Collections.Generic;
using System.Text;
using ViewModels.Comment;

namespace Application.Interfaces.Services
{
    public interface ICommentService
    {
        Task<CommentIndexView?> GetCommentIndexViewAsync(
            int postId,
            int page = 1);

        Task<CommentIndexView?> GetEditViewAsync(
            int commentId,
            int page = 1);

        Task CreateAsync(CommentCreateView model);

        Task UpdateAsync(CommentEditView model);

        Task DeleteAsync(int id);

    }
}
