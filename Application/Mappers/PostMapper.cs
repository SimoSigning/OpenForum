using Application.Interfaces.Mappers;
using EntityModels;
using System;
using System.Collections.Generic;
using System.Text;
using ViewModels.Post;

namespace Application.Mappers
{
    public class PostMapper : IPostMapper
    {
        public PostListItemView ToListItemView(Post post)
        {
            return new PostListItemView
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                Creator = post.CreatedByUserName,
                CreatedAt = post.CreatedAt
            };
        }

        public PostEditView ToEditView(Post post)
        {
            return new PostEditView
            {
                Id = post.Id,
                TopicId = post.TopicId,
                Title = post.Title,
                Content = post.Content
            };
        }
        public Post ToEntity(PostCreateView model)
        {
            return new Post
            {
                TopicId = model.TopicId,
                Title = model.Title,
                Content = model.Content,
                CreatedAt = DateTime.UtcNow
            };
        }

        public Post ToEntity(PostEditView model)
        {
            return new Post
            {
                Id = model.Id,
                TopicId = model.TopicId,
                Title = model.Title,
                Content = model.Content
            };
        }
    }
}
