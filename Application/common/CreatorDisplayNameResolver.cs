using System;
using System.Collections.Generic;
using System.Text;

namespace Application.common
{
    namespace Application.Common
    {
        public static class CreatorDisplayNameResolver
        {
            public static string Resolve(string? currentUserId, string creatorUserId, string creatorUserName)
            {
                return currentUserId != null && string.Equals(currentUserId, creatorUserId, StringComparison.Ordinal) ? "Me" : creatorUserName;
            }
        }
    }
}
