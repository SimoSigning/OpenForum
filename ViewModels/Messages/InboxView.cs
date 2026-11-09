using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels.Messages
{
    public class InboxView
    {
        public IEnumerable<MessageItemView> Messages { get; set; } = Enumerable.Empty<MessageItemView>();

        public int UnreadCount { get; set; }
    }
}
