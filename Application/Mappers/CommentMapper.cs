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
                Content = comment.Content,
                CreatedAt = comment.CreatedAt
            };
        }

        public CommentEditView ToEditView(Comment comment)
        {
            return new CommentEditView
            {
                Id = comment.Id,
                PostId = comment.PostId,
                Content = comment.Content
            };
        }

        public Comment ToEntity(CommentCreateView model)
        {
            return new Comment
            {
                PostId = model.PostId,
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
                Content = model.Content
            };
        }
    }
}
