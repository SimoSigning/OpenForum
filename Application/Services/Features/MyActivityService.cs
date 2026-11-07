using Application.Interfaces.Contexts;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Features;
using Application.Interfaces.Services.User;
using Application.Services.User;
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


        public async Task<MyActivityIndexView> GetMyActivityIndexViewAsync(ActivityFilter filter = ActivityFilter.All, int page = 1)
        {
            var userId = currentUser.GetRequiredUserId();

            page = Math.Max(1, page);

            // Retrieve the total counts.
            var topicCount = await topicRepository.CountByCreatedByUserIdAsync(userId);

            var postCount = await postRepository.CountByCreatedByUserIdAsync(userId);

            var commentCount = await commentRepository.CountByCreatedByUserIdAsync(userId);

            // Calculate the total count for the selected filter.
            var totalCount = filter switch
            {
                ActivityFilter.Topics => topicCount,
                ActivityFilter.Posts => postCount,
                ActivityFilter.Comments => commentCount,
                _ => topicCount + postCount + commentCount
            };

            // Calculate pagination.
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));

            page = Math.Min(page, totalPages);

            var skip = (page - 1) * PageSize;
            var take = skip + PageSize;

            // Retrieve records according to the selected filter.
            var activities = new List<ActivityListItemView>();

            if (filter == ActivityFilter.All || filter == ActivityFilter.Topics)
            {
                var topics = await topicRepository.GetByCreatedByUserIdAsync(userId, 0, take);
                activities.AddRange(topics.Select(MapTopic));
            }

            if (filter == ActivityFilter.All || filter == ActivityFilter.Posts)
            {
                var posts = await postRepository.GetByCreatedByUserIdAsync(userId, 0, take);
                activities.AddRange(posts.Select(MapPost));
            }

            if (filter == ActivityFilter.All || filter == ActivityFilter.Comments)
            {
                var comments = await commentRepository.GetByCreatedByUserIdAsync(userId, 0, take);
                activities.AddRange(comments.Select(MapComment));
            }

            // Sort all retrieved activities chronologically.
            var orderedActivities = activities
                .OrderByDescending(a => a.CreatedAt)
                .ThenByDescending(a => a.Id)
                .ThenBy(a => a.Type)
                .ToList();

            // Apply global pagination.
            var pagedActivities = orderedActivities
                .Skip(skip)
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

        private static List<ActivityListItemView> ApplyFilter(List<ActivityListItemView> activities, ActivityFilter filter)
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