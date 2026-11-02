using EntityModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repository.interfaces
{
    public interface IUserProfileRepository
    {
        Task<UserProfile?> GetByUserIdAsync(string userId);

        Task AddAsync(UserProfile profile);

        Task UpdateAsync(UserProfile profile);
    }
}
