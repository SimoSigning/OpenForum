using Application.Interfaces.Contexts;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Features;
using Application.Interfaces.Services.User;
using EntityModels;
using Repository.interfaces;
using ViewModels.MyActivity;

namespace Application.Services.Features
{
    public class MyActivityService : IMyActivityService
    {
        private readonly ITopicRepository topicRepository;
        private readonly IPostRepository postRepository;
        private readonly ICommentRepository commentRepository;
        private readonly ICurrentUserService currentUser;

        private const int PageSize = 5;
        private const int PreviewLength = 150;

        public MyActivityService(
            ITopicRepository topicRepository,
            IPostRepository postRepository,
            ICommentRepository commentRepository,
            ICurrentUserService currentUser)
        {
            this.topicRepository = topicRepository;
            this.postRepository = postRepository;
            this.commentRepository = commentRepository;
            this.currentUser = currentUser;
        }

        public async Task<MyActivityIndexView> GetMyActivityIndexViewAsync(
            ActivityFilter filter = ActivityFilter.All,
            int page = 1)
        {
            string userId = currentUser.GetRequiredUserId();

            page = Math.Max(1, page);

            // Retrieve all content created by the current user.
            IEnumerable<Topic> topics =
                await topicRepository.GetByCreatedByUserIdAsync(userId);

            IEnumerable<Post> posts =
                await postRepository.GetByCreatedByUserIdAsync(userId);

            IEnumerable<Comment> comments =
                await commentRepository.GetByCreatedByUserIdAsync(userId);

            // Convert all content types into activity items.
            List<ActivityListItemView> activities = new();

            activities.AddRange(topics.Select(MapTopic));

            activities.AddRange(posts.Select(MapPost));

            activities.AddRange(comments.Select(MapComment));

            // Apply the selected filter.
            activities = ApplyFilter(activities, filter);

            // Sort all activity chronologically.
            activities = activities
                .OrderByDescending(a => a.CreatedAt)
                .ThenByDescending(a => a.Id)
                .ThenBy(a => a.Type)
                .ToList();

            int totalCount = activities.Count;

            int totalPages = Math.Max(
                1,
                (int)Math.Ceiling(totalCount / (double)PageSize));

            // Keep the requested page within valid bounds.
            if (page > totalPages)
            {
                page = totalPages;
            }

            // Apply pagination to the combined activity list.
            List<ActivityListItemView> pagedActivities = activities
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            return new MyActivityIndexView
            {
                Activities = pagedActivities,
                Filter = filter,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount
            };
        }

        private static List<ActivityListItemView> ApplyFilter(
            List<ActivityListItemView> activities,
            ActivityFilter filter)
        {
            return filter switch
            {
                ActivityFilter.Topics => activities
                    .Where(a => a.Type == ActivityType.Topic)
                    .ToList(),

                ActivityFilter.Posts => activities
                    .Where(a => a.Type == ActivityType.Post)
                    .ToList(),

                ActivityFilter.Comments => activities
                    .Where(a =>
                        a.Type == ActivityType.Comment ||
                        a.Type == ActivityType.Reply)
                    .ToList(),

                _ => activities
            };
        }

        private static ActivityListItemView MapTopic(Topic topic)
        {
            return new ActivityListItemView
            {
                Id = topic.Id,
                Type = ActivityType.Topic,
                Title = topic.Title,
                CreatedAt = topic.CreatedAt
            };
        }

        private static ActivityListItemView MapPost(Post post)
        {
            return new ActivityListItemView
            {
                Id = post.Id,
                Type = ActivityType.Post,
                Title = post.Title,
                ContentPreview = CreatePreview(post.Content),
                CreatedAt = post.CreatedAt,
                TopicId = post.TopicId,
                ContextTitle = post.Topic?.Title ?? string.Empty
            };
        }

        private static ActivityListItemView MapComment(Comment comment)
        {
            bool isReply = comment.ParentCommentId.HasValue;

            string contextTitle;

            if (isReply)
            {
                contextTitle = !string.IsNullOrWhiteSpace(comment.ParentComment?.Title)
                    ? comment.ParentComment.Title
                    : comment.ParentComment?.Content ?? string.Empty;
            }
            else
            {
                contextTitle = comment.Post?.Title ?? string.Empty;
            }

            return new ActivityListItemView
            {
                Id = comment.Id,
                Type = isReply
                    ? ActivityType.Reply
                    : ActivityType.Comment,

                Title = comment.Title,
                ContentPreview = CreatePreview(comment.Content),
                CreatedAt = comment.CreatedAt,

                PostId = comment.PostId,
                TopicId = comment.Post?.TopicId,
                ParentCommentId = comment.ParentCommentId,

                ContextTitle = CreatePreview(contextTitle)
            };
        }

        private static string CreatePreview(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return string.Empty;
            }

            content = content.Trim();

            if (content.Length <= PreviewLength)
            {
                return content;
            }

            return content[..PreviewLength] + "...";
        }
    }
}