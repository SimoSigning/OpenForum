using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels.Friends
{
    public class UserRelationshipView
    {
        public int? FriendshipId { get; set; }

        public string? RelationshipStatus { get; set; }

        public bool CanSendRequest { get; set; }
    }
}
