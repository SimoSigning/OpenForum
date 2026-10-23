using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels.Post
{
    public class PostIndexView
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Creator { get; set; } = string.Empty;

        public IEnumerable<PostListItemView> Posts { get; set; } = Enumerable.Empty<PostListItemView>();

        public PostCreateView CreatePost { get; set; } = new();

        public PostEditView? EditPost { get; set; } = null;

        public int CurrentPage { get; set; } = 1;

        public int TotalPages { get; set; } = 1;
    }
}
