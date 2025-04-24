using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Function.Repositories
{
    /// <summary>
    /// Interface for function repository
    /// </summary>
    public interface IFunctionRepository
    {
        /// <summary>
        /// Creates a new function
        /// </summary>
        /// <param name="function">Function to create</param>
        /// <returns>The created function</returns>
        Task<Core.Models.Function> CreateAsync(Core.Models.Function function);

        /// <summary>
        /// Gets a function by ID
        /// </summary>
        /// <param name="id">Function ID</param>
        /// <returns>The function if found, null otherwise</returns>
        Task<Core.Models.Function> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets a function by name and account ID
        /// </summary>
        /// <param name="name">Function name</param>
        /// <param name="accountId">Account ID</param>
        /// <returns>The function if found, null otherwise</returns>
        Task<Core.Models.Function> GetByNameAndAccountIdAsync(string name, Guid accountId);

        /// <summary>
        /// Gets functions by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>List of functions owned by the account</returns>
        Task<IEnumerable<Core.Models.Function>> GetByAccountIdAsync(Guid accountId);

        /// <summary>
        /// Updates a function
        /// </summary>
        /// <param name="function">Function to update</param>
        /// <returns>The updated function</returns>
        Task<Core.Models.Function> UpdateAsync(Core.Models.Function function);

        /// <summary>
        /// Deletes a function
        /// </summary>
        /// <param name="id">Function ID</param>
        /// <returns>True if function was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Gets all functions
        /// </summary>
        /// <returns>List of all functions</returns>
        Task<IEnumerable<Core.Models.Function>> GetAllAsync();
    }
}
