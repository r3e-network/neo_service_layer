using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Secrets.Repositories
{
    /// <summary>
    /// Interface for secrets repository
    /// </summary>
    public interface ISecretsRepository
    {
        /// <summary>
        /// Creates a new secret
        /// </summary>
        /// <param name="secret">Secret to create</param>
        /// <returns>The created secret</returns>
        Task<Secret> CreateAsync(Secret secret);

        /// <summary>
        /// Gets a secret by ID
        /// </summary>
        /// <param name="id">Secret ID</param>
        /// <returns>The secret if found, null otherwise</returns>
        Task<Secret> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets a secret by name and account ID
        /// </summary>
        /// <param name="name">Secret name</param>
        /// <param name="accountId">Account ID</param>
        /// <returns>The secret if found, null otherwise</returns>
        Task<Secret> GetByNameAsync(string name, Guid accountId);

        /// <summary>
        /// Gets secrets by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>List of secrets owned by the account</returns>
        Task<IEnumerable<Secret>> GetByAccountIdAsync(Guid accountId);

        /// <summary>
        /// Gets secrets accessible by a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>List of secrets accessible by the function</returns>
        Task<IEnumerable<Secret>> GetByFunctionIdAsync(Guid functionId);

        /// <summary>
        /// Updates a secret
        /// </summary>
        /// <param name="secret">Secret to update</param>
        /// <returns>The updated secret</returns>
        Task<Secret> UpdateAsync(Secret secret);

        /// <summary>
        /// Deletes a secret
        /// </summary>
        /// <param name="id">Secret ID</param>
        /// <returns>True if secret was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);
    }
}
