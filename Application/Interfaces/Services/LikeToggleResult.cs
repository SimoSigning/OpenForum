using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services
{
    public class LikeToggleResult
    {
        public bool IsLiked { get; set; }

        public int LikeCount { get; set; }
    }
}
