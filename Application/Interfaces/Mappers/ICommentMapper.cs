using EntityModels;
using System;
using System.Collections.Generic;
using System.Text;
using ViewModels.Comment;

namespace Application.Interfaces.Mappers
{
    public interface ICommentMapper
    {
        CommentListItemView ToListItemView(Comment comment);

        CommentEditView ToEditView(Comment comment);

        Comment ToEntity(CommentCreateView model);

        Comment ToEntity(CommentEditView model);
    }
}
