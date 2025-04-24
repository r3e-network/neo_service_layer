using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for function environment repository
    /// </summary>
    public interface IFunctionEnvironmentRepository
    {
        /// <summary>
        /// Creates a new function environment
        /// </summary>
        /// <param name="environment">Function environment to create</param>
        /// <returns>The created function environment</returns>
        Task<FunctionEnvironment> CreateAsync(FunctionEnvironment environment);

        /// <summary>
        /// Updates a function environment
        /// </summary>
        /// <param name="environment">Function environment to update</param>
        /// <returns>The updated function environment</returns>
        Task<FunctionEnvironment> UpdateAsync(FunctionEnvironment environment);

        /// <summary>
        /// Gets a function environment by ID
        /// </summary>
        /// <param name="id">Environment ID</param>
        /// <returns>The function environment if found, null otherwise</returns>
        Task<FunctionEnvironment> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets function environments by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>List of function environments</returns>
        Task<IEnumerable<FunctionEnvironment>> GetByAccountIdAsync(Guid accountId);

        /// <summary>
        /// Gets function environments by type
        /// </summary>
        /// <param name="type">Environment type</param>
        /// <returns>List of function environments</returns>
        Task<IEnumerable<FunctionEnvironment>> GetByTypeAsync(string type);

        /// <summary>
        /// Gets function environments by network
        /// </summary>
        /// <param name="network">Environment network</param>
        /// <returns>List of function environments</returns>
        Task<IEnumerable<FunctionEnvironment>> GetByNetworkAsync(string network);

        /// <summary>
        /// Deletes a function environment
        /// </summary>
        /// <param name="id">Environment ID</param>
        /// <returns>True if the environment was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);
    }
}
