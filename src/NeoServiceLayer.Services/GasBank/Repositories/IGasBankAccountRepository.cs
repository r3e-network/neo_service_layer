using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.GasBank.Repositories
{
    /// <summary>
    /// Interface for the GasBank account repository
    /// </summary>
    public interface IGasBankAccountRepository
    {
        /// <summary>
        /// Creates a new GasBank account
        /// </summary>
        /// <param name="account">The GasBank account to create</param>
        /// <returns>The created GasBank account</returns>
        Task<GasBankAccount> CreateAsync(GasBankAccount account);

        /// <summary>
        /// Gets a GasBank account by ID
        /// </summary>
        /// <param name="id">The GasBank account ID</param>
        /// <returns>The GasBank account</returns>
        Task<GasBankAccount> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets all GasBank accounts for an account
        /// </summary>
        /// <param name="accountId">The account ID</param>
        /// <returns>The GasBank accounts</returns>
        Task<IEnumerable<GasBankAccount>> GetByAccountIdAsync(Guid accountId);

        /// <summary>
        /// Updates a GasBank account
        /// </summary>
        /// <param name="account">The GasBank account to update</param>
        /// <returns>The updated GasBank account</returns>
        Task<GasBankAccount> UpdateAsync(GasBankAccount account);

        /// <summary>
        /// Deletes a GasBank account
        /// </summary>
        /// <param name="id">The GasBank account ID</param>
        /// <returns>True if successful</returns>
        Task<bool> DeleteAsync(Guid id);
    }
}
