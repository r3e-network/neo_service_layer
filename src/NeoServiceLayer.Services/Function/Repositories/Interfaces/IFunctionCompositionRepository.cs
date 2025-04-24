using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Function.Repositories
{
    /// <summary>
    /// Interface for function composition repository
    /// </summary>
    public interface IFunctionCompositionRepository
    {
        /// <summary>
        /// Gets a function composition by ID
        /// </summary>
        /// <param name="id">Composition ID</param>
        /// <returns>The function composition if found, null otherwise</returns>
        Task<FunctionComposition> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets function compositions by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="limit">Maximum number of compositions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function compositions for the specified account</returns>
        Task<IEnumerable<FunctionComposition>> GetByAccountIdAsync(Guid accountId, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function compositions by name
        /// </summary>
        /// <param name="name">Composition name</param>
        /// <param name="limit">Maximum number of compositions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function compositions with the specified name</returns>
        Task<IEnumerable<FunctionComposition>> GetByNameAsync(string name, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function compositions by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="limit">Maximum number of compositions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function compositions that include the specified function</returns>
        Task<IEnumerable<FunctionComposition>> GetByFunctionIdAsync(Guid functionId, int limit = 100, int offset = 0);

        /// <summary>
        /// Creates a function composition
        /// </summary>
        /// <param name="composition">Composition to create</param>
        /// <returns>The created function composition</returns>
        Task<FunctionComposition> CreateAsync(FunctionComposition composition);

        /// <summary>
        /// Updates a function composition
        /// </summary>
        /// <param name="id">Composition ID</param>
        /// <param name="composition">Updated composition</param>
        /// <returns>The updated function composition</returns>
        Task<FunctionComposition> UpdateAsync(Guid id, FunctionComposition composition);

        /// <summary>
        /// Deletes a function composition
        /// </summary>
        /// <param name="id">Composition ID</param>
        /// <returns>True if the composition was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Gets all function compositions
        /// </summary>
        /// <param name="limit">Maximum number of compositions to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of all function compositions</returns>
        Task<IEnumerable<FunctionComposition>> GetAllAsync(int limit = 100, int offset = 0);
    }
}
