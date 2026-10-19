using Application.Interfaces.Mappers;
using Application.Interfaces.Services;
using Repository.interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using ViewModels.Comment;
using ViewModels.Post;

namespace Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository commentRepository;
        private readonly IPostRepository postRepository;
        private readonly ICommentMapper commentMapper;

        public CommentService(
            ICommentRepository commentRepository,
            IPostRepository postRepository,
            ICommentMapper commentMapper)
        {
            this.commentRepository = commentRepository;
            this.postRepository = postRepository;
            this.commentMapper = commentMapper;
        }


        public async Task<PostDetailsView?> GetPostDetailsViewAsync(int postId)
        {
            var post = await postRepository.GetByIdAsync(postId);

            if (post == null)
                return null;


            var comments = await commentRepository.GetByPostIdAsync(postId);


            return new PostDetailsView
            {
                Id = post.Id,

                TopicId = post.TopicId,

                Content = post.Content,

                CreatedAt = post.CreatedAt,

                Comments = comments.Select(commentMapper.ToListItemView),

                CreateComment = new CommentCreateView
                {
                    PostId = postId
                },

                EditComment = null
            };
        }


        public async Task<PostDetailsView?> GetEditViewAsync(int commentId)
        {
            var comment = await commentRepository.GetByIdAsync(commentId);

            if (comment == null)
                return null;


            var post = await postRepository.GetByIdAsync(comment.PostId);

            if (post == null)
                return null;


            var comments = await commentRepository.GetByPostIdAsync(post.Id);


            return new PostDetailsView
            {
                Id = post.Id,

                TopicId = post.TopicId,

                Content = post.Content,

                CreatedAt = post.CreatedAt,

                Comments = comments.Select(commentMapper.ToListItemView),

                CreateComment = new CommentCreateView
                {
                    PostId = post.Id
                },

                EditComment = commentMapper.ToEditView(comment)
            };
        }


        public async Task CreateAsync(CommentCreateView model)
        {
            var comment = commentMapper.ToEntity(model);

            await commentRepository.AddAsync(comment);
        }


        public async Task UpdateAsync(CommentEditView model)
        {
            var comment = await commentRepository.GetByIdAsync(model.Id);

            if (comment == null)
                return;


            comment.Content = model.Content;

            await commentRepository.UpdateAsync(comment);
        }


        public async Task DeleteAsync(int id)
        {
            await commentRepository.DeleteAsync(id);
        }
    }
}
