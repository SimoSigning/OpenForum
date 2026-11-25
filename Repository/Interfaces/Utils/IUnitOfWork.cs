using System;
using System.Collections.Generic;
using System.Text;

namespace Repository.Interfaces.Utils
{
    public interface IUnitOfWork
    {
        Task BeginTransactionAsync();

        Task CommitTransactionAsync();

        Task RollbackTransactionAsync();
    }
}
