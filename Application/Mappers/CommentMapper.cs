using Application.Interfaces.Mappers;
using EntityModels;
using System;
using System.Collections.Generic;
using System.Text;
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
                Title = comment.Title,
                Content = comment.Content,
                Creator = comment.CreatedByUserName,
                CreatedAt = comment.CreatedAt,
                ParentCommentId = comment.ParentCommentId
            };
        }

        public CommentEditView ToEditView(Comment comment)
        {
            return new CommentEditView
            {
                Id = comment.Id,
                PostId = comment.PostId,
                Title = comment.Title,
                Content = comment.Content,
                ParentCommentId = comment.ParentCommentId,
            };
        }

        public Comment ToEntity(CommentCreateView model)
        {
            bool isReply = model.ParentCommentId.HasValue;

            return new Comment
            {
                PostId = model.PostId,
                ParentCommentId = model.ParentCommentId,
                Title = isReply ? string.Empty : model.Title?.Trim() ?? string.Empty,
                Content = model.Content,
                CreatedAt = DateTime.UtcNow
            };
        }

    }
}
