using Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services
{
    public interface ILikeService
    {
        Task<LikeToggleResult> TogglePostLikeAsync(int postId);

        Task<LikeToggleResult> ToggleCommentLikeAsync(int commentId);

        Task<List<string>> GetPostLikerUsernamesAsync(int postId);

        Task<List<string>> GetCommentLikerUsernamesAsync(int commentId);
    }
}