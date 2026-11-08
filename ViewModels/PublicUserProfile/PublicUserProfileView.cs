using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels.PublicUserProfile
{
    public class PublicUserProfileView
    {
        public string UserId { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? City { get; set; }

        public string? ProfileText { get; set; }

        public string? RelationshipStatus { get; set; }

        public bool CanSendFriendRequest { get; set; }

        public int? FriendshipId { get; set; }
    }
}
