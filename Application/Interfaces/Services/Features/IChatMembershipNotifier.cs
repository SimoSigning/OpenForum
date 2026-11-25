using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services.Features
{
    public interface IChatMembershipNotifier
    {
        Task NotifyMembersChangedAsync(int roomId);
    }
}
