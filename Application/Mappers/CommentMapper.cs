
using Application.Interfaces.Mappers;
using EntityModels;
using ViewModels.Comment;

namespace Application.Mappers
{
    public class CommentMapper : ICommentMapper
    {
        public CommentListItemView ToListItemView(Comment comment)
        {
            return new CommentListItemView
            {
                Id = comment.Id,
                Title = string.IsNullOrWhiteSpace(comment.Title)
                    ? null
                    : comment.Title,
                Content = comment.Content,
                Creator = comment.CreatedByUserName,
                CreatedAt = comment.CreatedAt
            };
        }

        public CommentEditView ToEditView(Comment comment)
        {
            return new CommentEditView
            {
                Id = comment.Id,
                PostId = comment.PostId,
                Title = comment.ParentCommentId.HasValue
                    ? null
                    : comment.Title,
                Content = comment.Content
            };
        }

        public Comment ToEntity(CommentCreateView model)
        {
            return new Comment
            {
                PostId = model.PostId,
                ParentCommentId = model.ParentCommentId,
                Title = model.ParentCommentId.HasValue
                    ? string.Empty
                    : model.Title ?? string.Empty,
                Content = model.Content,
                CreatedAt = DateTime.UtcNow
            };
        }

        public Comment ToEntity(CommentEditView model)
        {
            return new Comment
            {
                Id = model.Id,
                PostId = model.PostId,
                Title = model.Title ?? string.Empty,
                Content = model.Content
            };
        }
    }
}
