using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels.Messages
{
    public class SentMessagesView
    {
        public IEnumerable<MessageItemView> Messages { get; set; } = Enumerable.Empty<MessageItemView>();
    }
}
