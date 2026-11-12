using ViewModels.MyActivity;

namespace Application.Interfaces.Services.Features
{
    public interface IMyActivityService
    {
        Task<MyActivityIndexView> GetMyActivityIndexViewAsync(
            ActivityFilter filter = ActivityFilter.All,
            int page = 1);

        // Edit ViewModels
        Task<ActivityTopicEditView?> GetEditTopicViewAsync(int id);
        Task<ActivityPostEditView?> GetEditPostViewAsync(int id);
        Task<ActivityCommentEditView?> GetEditCommentViewAsync(int id);

        // Update
        Task UpdateTopicAsync(ActivityTopicEditView model);
        Task UpdatePostAsync(ActivityPostEditView model);
        Task UpdateCommentAsync(ActivityCommentEditView model);

        // Delete
        Task DeleteTopicAsync(int id);
        Task DeletePostAsync(int id);
        Task DeleteCommentAsync(int id);
    }
}
