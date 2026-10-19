using Application.Interfaces.Mappers;
using Application.Interfaces.Services;
using Repository.interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using ViewModels.Post;
using ViewModels.Topic;

namespace Application.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository postRepository;
        private readonly ITopicRepository topicRepository;
        private readonly IPostMapper postMapper;


        public PostService(
            IPostRepository postRepository,
            ITopicRepository topicRepository,
            IPostMapper postMapper)
        {
            this.postRepository = postRepository;
            this.topicRepository = topicRepository;
            this.postMapper = postMapper;
        }


        public async Task<TopicDetailsView?> GetTopicDetailsViewAsync(int topicId)
        {
            var topic = await topicRepository.GetByIdAsync(topicId);

            if (topic == null)
                return null;


            var posts = await postRepository.GetByTopicIdAsync(topicId);


            return new TopicDetailsView
            {
                Id = topic.Id,

                Title = topic.Title,

                Posts = posts.Select(postMapper.ToListItemView),

                CreatePost = new PostCreateView
                {
                    TopicId = topicId
                },

                EditPost = null
            };
        }


        public async Task<TopicDetailsView?> GetEditViewAsync(int postId)
        {
            var post = await postRepository.GetByIdAsync(postId);

            if (post == null)
                return null;


            var topic = await topicRepository.GetByIdAsync(post.TopicId);

            if (topic == null)
                return null;


            var posts = await postRepository.GetByTopicIdAsync(post.TopicId);


            return new TopicDetailsView
            {
                Id = topic.Id,

                Title = topic.Title,

                Posts = posts.Select(postMapper.ToListItemView),

                CreatePost = new PostCreateView
                {
                    TopicId = topic.Id
                },

                EditPost = postMapper.ToEditView(post)
            };
        }


        public async Task CreateAsync(PostCreateView model)
        {
            var post = postMapper.ToEntity(model);

            await postRepository.AddAsync(post);
        }


        public async Task UpdateAsync(PostEditView model)
        {
            var post = await postRepository.GetByIdAsync(model.Id);

            if (post == null)
                return;


            post.Content = model.Content;

            await postRepository.UpdateAsync(post);
        }


        public async Task DeleteAsync(int id)
        {
            await postRepository.DeleteAsync(id);
        }
    }
}
