using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services
{
    public interface ILikeService
    {
        Task TogglePostLikeAsync(int postId);

        Task ToggleCommentLikeAsync(int commentId);
    }
}
