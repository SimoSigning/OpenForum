using EntityModels;
using System;
using System.Collections.Generic;
using System.Text;
using ViewModels.Post;

namespace Application.Interfaces.Mappers
{
    public interface IPostMapper
    {
        PostListItemView ToListItemView(Post post);

        PostEditView ToEditView(Post post);

        Post ToEntity(PostCreateView model);

        Post ToEntity(PostEditView model);
    }
}
