using System;
using System.Collections.Generic;
using System.Text;

namespace EntityModels
{
    public class Friendship
    {
        public int Id { get; set; }

        // The user who initiated the request.
        public string RequesterUserId { get; set; } = null!;

        // The user who received the request.
        public string AddresseeUserId { get; set; } = null!;

        // Normalized IDs used to prevent duplicate relationships.
        public string UserIdLow { get; set; } = null!;

        public string UserIdHigh { get; set; } = null!;

        public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum FriendshipStatus
    {
        Pending = 0,
        Accepted = 1
    }
}