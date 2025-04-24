using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for function composition repository
    /// </summary>
    public interface IFunctionCompositionRepository
    {
        /// <summary>
        /// Creates a new function composition
        /// </summary>
        /// <param name="composition">Function composition to create</param>
        /// <returns>The created function composition</returns>
        Task<FunctionComposition> CreateAsync(FunctionComposition composition);

        /// <summary>
        /// Updates a function composition
        /// </summary>
        /// <param name="composition">Function composition to update</param>
        /// <returns>The updated function composition</returns>
        Task<FunctionComposition> UpdateAsync(FunctionComposition composition);

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
        /// <returns>List of function compositions</returns>
        Task<IEnumerable<FunctionComposition>> GetByAccountIdAsync(Guid accountId);

        /// <summary>
        /// Gets function compositions by tags
        /// </summary>
        /// <param name="tags">Tags</param>
        /// <returns>List of function compositions</returns>
        Task<IEnumerable<FunctionComposition>> GetByTagsAsync(List<string> tags);

        /// <summary>
        /// Deletes a function composition
        /// </summary>
        /// <param name="id">Composition ID</param>
        /// <returns>True if the composition was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Deletes function compositions by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>True if the compositions were deleted successfully, false otherwise</returns>
        Task<bool> DeleteByAccountIdAsync(Guid accountId);
    }
}
