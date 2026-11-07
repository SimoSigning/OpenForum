using EntityModels;
using System;
using System.Collections.Generic;
using System.Text;
using ViewModels.Comment;
using ViewModels.MyActivity;
using ViewModels.Post;
using ViewModels.Topic;

namespace Application.Interfaces.Services.Features
{
    public interface IMyActivityService
    {
        Task<MyActivityIndexView> GetMyActivityIndexViewAsync(
            ActivityFilter filter = ActivityFilter.All,
            int page = 1);
    }
}
