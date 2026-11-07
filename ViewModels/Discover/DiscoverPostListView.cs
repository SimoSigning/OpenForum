using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels.Discover
{
    public class DiscoverPostListView
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string Creator { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
