using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for function dependency repository
    /// </summary>
    public interface IFunctionDependencyRepository
    {
        /// <summary>
        /// Creates a new function dependency
        /// </summary>
        /// <param name="dependency">Function dependency to create</param>
        /// <returns>The created function dependency</returns>
        Task<FunctionDependency> CreateAsync(FunctionDependency dependency);

        /// <summary>
        /// Updates a function dependency
        /// </summary>
        /// <param name="dependency">Function dependency to update</param>
        /// <returns>The updated function dependency</returns>
        Task<FunctionDependency> UpdateAsync(FunctionDependency dependency);

        /// <summary>
        /// Gets a function dependency by ID
        /// </summary>
        /// <param name="id">Dependency ID</param>
        /// <returns>The function dependency if found, null otherwise</returns>
        Task<FunctionDependency> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets function dependencies by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>List of function dependencies</returns>
        Task<IEnumerable<FunctionDependency>> GetByFunctionIdAsync(Guid functionId);

        /// <summary>
        /// Gets function dependencies by name and version
        /// </summary>
        /// <param name="name">Dependency name</param>
        /// <param name="version">Dependency version</param>
        /// <returns>List of function dependencies</returns>
        Task<IEnumerable<FunctionDependency>> GetByNameAndVersionAsync(string name, string version);

        /// <summary>
        /// Gets function dependencies by type
        /// </summary>
        /// <param name="type">Dependency type</param>
        /// <returns>List of function dependencies</returns>
        Task<IEnumerable<FunctionDependency>> GetByTypeAsync(string type);

        /// <summary>
        /// Deletes a function dependency
        /// </summary>
        /// <param name="id">Dependency ID</param>
        /// <returns>True if the dependency was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Deletes function dependencies by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>True if the dependencies were deleted successfully, false otherwise</returns>
        Task<bool> DeleteByFunctionIdAsync(Guid functionId);
    }
}
