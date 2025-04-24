using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for function repository
    /// </summary>
    public interface IFunctionRepository
    {
        /// <summary>
        /// Gets a function by ID
        /// </summary>
        /// <param name="id">Function ID</param>
        /// <returns>The function if found, null otherwise</returns>
        Task<Models.Function?> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets functions by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="limit">Maximum number of functions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of functions for the account</returns>
        Task<IEnumerable<Models.Function>> GetByAccountIdAsync(Guid accountId, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets functions by name
        /// </summary>
        /// <param name="name">Function name</param>
        /// <param name="limit">Maximum number of functions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of functions with the specified name</returns>
        Task<IEnumerable<Models.Function>> GetByNameAsync(string name, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets a function by name and account ID
        /// </summary>
        /// <param name="name">Function name</param>
        /// <param name="accountId">Account ID</param>
        /// <returns>The function if found, null otherwise</returns>
        Task<Models.Function?> GetByNameAndAccountIdAsync(string name, Guid accountId);

        /// <summary>
        /// Gets functions by runtime
        /// </summary>
        /// <param name="runtime">Function runtime</param>
        /// <param name="limit">Maximum number of functions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of functions with the specified runtime</returns>
        Task<IEnumerable<Models.Function>> GetByRuntimeAsync(string runtime, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets functions by tags
        /// </summary>
        /// <param name="tags">Function tags</param>
        /// <param name="limit">Maximum number of functions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of functions with the specified tags</returns>
        Task<IEnumerable<Models.Function>> GetByTagsAsync(List<string> tags, int limit = 100, int offset = 0);

        /// <summary>
        /// Creates a function
        /// </summary>
        /// <param name="function">Function to create</param>
        /// <returns>The created function</returns>
        Task<Models.Function> CreateAsync(Models.Function function);

        /// <summary>
        /// Updates a function
        /// </summary>
        /// <param name="id">Function ID</param>
        /// <param name="function">Updated function</param>
        /// <returns>The updated function</returns>
        Task<Models.Function> UpdateAsync(Guid id, Models.Function function);

        /// <summary>
        /// Deletes a function
        /// </summary>
        /// <param name="id">Function ID</param>
        /// <returns>True if the function was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Checks if a function exists
        /// </summary>
        /// <param name="id">Function ID</param>
        /// <returns>True if the function exists, false otherwise</returns>
        Task<bool> ExistsAsync(Guid id);

        /// <summary>
        /// Gets all functions
        /// </summary>
        /// <param name="limit">Maximum number of functions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of all functions</returns>
        Task<IEnumerable<Models.Function>> GetAllAsync(int limit = 100, int offset = 0);

        /// <summary>
        /// Counts functions
        /// </summary>
        /// <returns>Number of functions</returns>
        Task<int> CountAsync();

        /// <summary>
        /// Counts functions by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>Number of functions for the account</returns>
        Task<int> CountByAccountIdAsync(Guid accountId);

        /// <summary>
        /// Counts functions by runtime
        /// </summary>
        /// <param name="runtime">Function runtime</param>
        /// <returns>Number of functions with the specified runtime</returns>
        Task<int> CountByRuntimeAsync(string runtime);

        /// <summary>
        /// Counts functions by tags
        /// </summary>
        /// <param name="tags">Function tags</param>
        /// <returns>Number of functions with the specified tags</returns>
        Task<int> CountByTagsAsync(List<string> tags);
    }
}
