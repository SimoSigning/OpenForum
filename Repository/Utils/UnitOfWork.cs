using EntityContext;
using Microsoft.EntityFrameworkCore.Storage;
using Repository.Interfaces.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository.Utils
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ForumContext context;
        private IDbContextTransaction? transaction;

        public UnitOfWork(ForumContext context)
        {
            this.context = context;
        }

        public async Task BeginTransactionAsync()
        {
            transaction = await context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (transaction == null)
                throw new InvalidOperationException("No active transaction.");

            try
            {
                await transaction.CommitAsync();
            }
            finally
            {
                await transaction.DisposeAsync();
                transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (transaction == null)
                return;

            try
            {
                await transaction.RollbackAsync();
            }
            finally
            {
                await transaction.DisposeAsync();
                transaction = null;
            }
        }
    }
}
