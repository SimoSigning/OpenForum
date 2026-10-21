using System;
using System.Collections.Generic;
using System.Text;
using ViewModels.Comment;

namespace Application.Interfaces.Services
{
    public interface ICommentService
    {
        Task<CommentIndexView?> GetCommentIndexViewAsync(int postId);

        Task<CommentIndexView?> GetEditViewAsync(int commentId);

        Task CreateAsync(CommentCreateView model);

        Task UpdateAsync(CommentEditView model);

        Task DeleteAsync(int id);
    }
}
