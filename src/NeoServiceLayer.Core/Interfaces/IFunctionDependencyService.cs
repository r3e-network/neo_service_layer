using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for function dependency service
    /// </summary>
    public interface IFunctionDependencyService
    {
        /// <summary>
        /// Adds a dependency to a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="name">Dependency name</param>
        /// <param name="version">Dependency version</param>
        /// <param name="type">Dependency type</param>
        /// <returns>The added function dependency</returns>
        Task<FunctionDependency> AddDependencyAsync(Guid functionId, string name, string version, string type);

        /// <summary>
        /// Updates a function dependency
        /// </summary>
        /// <param name="id">Dependency ID</param>
        /// <param name="version">New dependency version</param>
        /// <returns>The updated function dependency</returns>
        Task<FunctionDependency> UpdateDependencyAsync(Guid id, string version);

        /// <summary>
        /// Removes a dependency from a function
        /// </summary>
        /// <param name="id">Dependency ID</param>
        /// <returns>True if the dependency was removed successfully, false otherwise</returns>
        Task<bool> RemoveDependencyAsync(Guid id);

        /// <summary>
        /// Gets dependencies for a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>List of function dependencies</returns>
        Task<IEnumerable<FunctionDependency>> GetDependenciesAsync(Guid functionId);

        /// <summary>
        /// Gets a function dependency by ID
        /// </summary>
        /// <param name="id">Dependency ID</param>
        /// <returns>The function dependency if found, null otherwise</returns>
        Task<FunctionDependency> GetDependencyAsync(Guid id);

        /// <summary>
        /// Installs dependencies for a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>True if the dependencies were installed successfully, false otherwise</returns>
        Task<bool> InstallDependenciesAsync(Guid functionId);

        /// <summary>
        /// Checks for dependency updates for a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>List of available dependency updates</returns>
        Task<IEnumerable<FunctionDependencyUpdate>> CheckForUpdatesAsync(Guid functionId);

        /// <summary>
        /// Updates all dependencies for a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>List of updated dependencies</returns>
        Task<IEnumerable<FunctionDependency>> UpdateAllDependenciesAsync(Guid functionId);

        /// <summary>
        /// Parses dependencies from a package file
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="packageFileContent">Package file content</param>
        /// <param name="packageFileType">Package file type (e.g., "package.json", "requirements.txt", "csproj")</param>
        /// <returns>List of parsed dependencies</returns>
        Task<IEnumerable<FunctionDependency>> ParseDependenciesAsync(Guid functionId, string packageFileContent, string packageFileType);

        /// <summary>
        /// Generates a package file for a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="packageFileType">Package file type (e.g., "package.json", "requirements.txt", "csproj")</param>
        /// <returns>The generated package file content</returns>
        Task<string> GeneratePackageFileAsync(Guid functionId, string packageFileType);

        /// <summary>
        /// Validates dependencies for a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>List of dependency validation results</returns>
        Task<IEnumerable<FunctionDependencyValidationResult>> ValidateDependenciesAsync(Guid functionId);
    }
}
