
using Application.Interfaces.Contexts;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Features;
using Application.Interfaces.Services.Forum;
using Application.Interfaces.Services.User;
using EntityModels;
using Repository.interfaces;
using ViewModels.Comment;
using ViewModels.MyActivity;
using ViewModels.Post;
using ViewModels.Topic;

namespace Application.Services.Features
{
    public class MyActivityService : IMyActivityService
    {
        private readonly ITopicRepository topicRepository;
        private readonly IPostRepository postRepository;
        private readonly ICommentRepository commentRepository;
        private readonly ICurrentUserService currentUser;

        private readonly ITopicService topicService;
        private readonly IPostService postService;
        private readonly ICommentService commentService;

        private const int PageSize = 5;
        private const int PreviewLength = 150;

        public MyActivityService(
            ITopicRepository topicRepository,
            IPostRepository postRepository,
            ICommentRepository commentRepository,
            ICurrentUserService currentUser,
            ITopicService topicService,
            IPostService postService,
            ICommentService commentService)
        {
            this.topicRepository = topicRepository;
            this.postRepository = postRepository;
            this.commentRepository = commentRepository;
            this.currentUser = currentUser;

            this.topicService = topicService;
            this.postService = postService;
            this.commentService = commentService;
        }

        public async Task<MyActivityIndexView> GetMyActivityIndexViewAsync(ActivityFilter filter = ActivityFilter.All, int page = 1)
        {
            string userId = currentUser.GetRequiredUserId();

            page = Math.Max(1, page);

            int topicCount = await topicRepository.CountByCreatedByUserIdAsync(userId);

            int postCount = await postRepository.CountByCreatedByUserIdAsync(userId);

            int commentCount = await commentRepository.CountByCreatedByUserIdAsync(userId);

            // Calculate the total count for the selected filter.
            int totalCount = filter switch
            {
                ActivityFilter.Topics => topicCount,
                ActivityFilter.Posts => postCount,
                ActivityFilter.Comments => commentCount,
                _ => topicCount + postCount + commentCount
            };

            // Calculate pagination.
            int totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));

            page = Math.Min(page, totalPages);

            int skip = (page - 1) * PageSize;
            int take = skip + PageSize;

            // Retrieve records according to the selected filter.
            List<ActivityListItemView> activities = new();

            if (filter == ActivityFilter.All || filter == ActivityFilter.Topics)
            {
                IEnumerable<Topic> topics = await topicRepository.GetByCreatedByUserIdAsync(userId, 0, take);

                activities.AddRange(topics.Select(MapTopic));
            }

            if (filter == ActivityFilter.All || filter == ActivityFilter.Posts)
            {
                IEnumerable<Post> posts = await postRepository.GetByCreatedByUserIdAsync(userId, 0, take);

                activities.AddRange(posts.Select(MapPost));
            }

            if (filter == ActivityFilter.All || filter == ActivityFilter.Comments)
            {
                IEnumerable<Comment> comments = await commentRepository.GetByCreatedByUserIdAsync(userId, 0, take);

                activities.AddRange(comments.Select(MapComment));
            }

            // Sort all retrieved activities chronologically.
            List<ActivityListItemView> orderedActivities = activities
                .OrderByDescending(a => a.CreatedAt)
                .ThenByDescending(a => a.Id)
                .ThenBy(a => a.Type)
                .ToList();

            // Apply global pagination.
            List<ActivityListItemView> pagedActivities = orderedActivities
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

        public async Task<ActivityTopicEditView?> GetEditTopicViewAsync(int id)
        {
            string userId = currentUser.GetRequiredUserId();

            Topic? topic = await topicRepository.GetByIdAsync(id);

            if (topic == null)
                return null;

            if (topic.CreatedByUserId != userId)
                throw new UnauthorizedAccessException("You are not allowed to edit this topic.");

            return new ActivityTopicEditView
            {
                Id = topic.Id,
                Title = topic.Title
            };
        }

        public async Task<ActivityPostEditView?> GetEditPostViewAsync(int id)
        {
            string userId = currentUser.GetRequiredUserId();

            Post? post = await postRepository.GetByIdAsync(id);

            if (post == null)
                return null;

            if (post.CreatedByUserId != userId)
                throw new UnauthorizedAccessException("You are not allowed to edit this post.");

            return new ActivityPostEditView
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content
            };
        }

        public async Task<ActivityCommentEditView?> GetEditCommentViewAsync(int id)
        {
            string userId = currentUser.GetRequiredUserId();

            Comment? comment = await commentRepository.GetByIdAsync(id);

            if (comment == null)
                return null;

            if (comment.CreatedByUserId != userId)
                throw new UnauthorizedAccessException("You are not allowed to edit this comment.");

            return new ActivityCommentEditView
            {
                Id = comment.Id,
                Title = comment.Title,
                Content = comment.Content,
                PostId = comment.PostId,
                ParentCommentId = comment.ParentCommentId
            };
        }

        public async Task UpdateTopicAsync(ActivityTopicEditView model)
        {
            TopicEditView topicModel = new()
            {
                Id = model.Id,
                Title = model.Title
            };

            await topicService.UpdateAsync(topicModel);
        }

        public async Task UpdatePostAsync(ActivityPostEditView model)
        {
            PostEditView postModel = new()
            {
                Id = model.Id,
                Title = model.Title,
                Content = model.Content
            };

            await postService.UpdateAsync(postModel);
        }

        public async Task UpdateCommentAsync(ActivityCommentEditView model)
        {
            CommentEditView commentModel = new()
            {
                Id = model.Id,
                Title = model.Title,
                Content = model.Content,
                PostId = model.PostId,
                ParentCommentId = model.ParentCommentId
            };

            await commentService.UpdateAsync(commentModel);
        }

        public async Task DeleteTopicAsync(int id)
        {
            await topicService.DeleteAsync(id);
        }

        public async Task DeletePostAsync(int id)
        {
            await postService.DeleteAsync(id);
        }

        public async Task DeleteCommentAsync(int id)
        {
            await commentService.DeleteAsync(id);
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
                contextTitle = !string.IsNullOrWhiteSpace(comment.ParentComment?.Title) ? comment.ParentComment.Title : comment.ParentComment?.Content ?? string.Empty;
            }
            else
            {
                contextTitle = comment.Post?.Title ?? string.Empty;
            }

            return new ActivityListItemView
            {
                Id = comment.Id,

                Type = isReply ? ActivityType.Reply : ActivityType.Comment,

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
                return string.Empty;

            content = content.Trim();

            if (content.Length <= PreviewLength)
                return content;

            return content[..PreviewLength] + "...";
        }
    }
}
