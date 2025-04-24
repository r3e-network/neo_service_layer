using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Services.Common.Repositories;

namespace NeoServiceLayer.Services.Account.Repositories
{
    /// <summary>
    /// Interface for account repository
    /// </summary>
    public interface IAccountRepository : IRepository<Core.Models.Account, Guid>
    {
        // IRepository<Core.Models.Account, Guid> methods are inherited

        /// <summary>
        /// Gets an account by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>The account if found, null otherwise</returns>
        Task<Core.Models.Account> GetByUsernameAsync(string username);

        /// <summary>
        /// Gets an account by email address
        /// </summary>
        /// <param name="email">Email address</param>
        /// <returns>The account if found, null otherwise</returns>
        Task<Core.Models.Account> GetByEmailAsync(string email);

        /// <summary>
        /// Gets an account by Neo N3 address
        /// </summary>
        /// <param name="neoAddress">Neo N3 address</param>
        /// <returns>The account if found, null otherwise</returns>
        Task<Core.Models.Account> GetByNeoAddressAsync(string neoAddress);

        // Additional methods specific to AccountRepository
    }
}
