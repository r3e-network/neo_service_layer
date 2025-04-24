using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Function.Repositories
{
    /// <summary>
    /// Repository for function dependencies
    /// </summary>
    public class FunctionDependencyRepository : IFunctionDependencyRepository
    {
        private readonly ILogger<FunctionDependencyRepository> _logger;
        private readonly IStorageProvider _storageProvider;
        private readonly string _collectionName = "function_dependencies";

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionDependencyRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="storageProvider">Storage provider</param>
        public FunctionDependencyRepository(ILogger<FunctionDependencyRepository> logger, IStorageProvider storageProvider)
        {
            _logger = logger;
            _storageProvider = storageProvider;
        }

        /// <inheritdoc/>
        public async Task<FunctionDependency> CreateAsync(FunctionDependency dependency)
        {
            _logger.LogInformation("Creating function dependency: {Name}@{Version} for function {FunctionId}", dependency.Name, dependency.Version, dependency.FunctionId);

            // Ensure ID is set
            if (dependency.Id == Guid.Empty)
            {
                dependency.Id = Guid.NewGuid();
            }

            // Save to store
            await _storageProvider.CreateAsync(_collectionName, dependency);

            return dependency;
        }

        /// <inheritdoc/>
        public async Task<FunctionDependency> UpdateAsync(FunctionDependency dependency)
        {
            _logger.LogInformation("Updating function dependency: {Id}", dependency.Id);

            // Update in store
            await _storageProvider.UpdateAsync<FunctionDependency, Guid>(_collectionName, dependency.Id, dependency);

            return dependency;
        }

        /// <inheritdoc/>
        public async Task<FunctionDependency> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting function dependency by ID: {Id}", id);

            // Get from store
            return await _storageProvider.GetByIdAsync<FunctionDependency, Guid>(_collectionName, id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionDependency>> GetByFunctionIdAsync(Guid functionId)
        {
            _logger.LogInformation("Getting function dependencies by function ID: {FunctionId}", functionId);

            // Get all dependencies
            var dependencies = await _storageProvider.GetAllAsync<FunctionDependency>(_collectionName);

            // Filter by function ID
            return dependencies.Where(d => d.FunctionId == functionId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionDependency>> GetByNameAndVersionAsync(string name, string version)
        {
            _logger.LogInformation("Getting function dependencies by name and version: {Name}@{Version}", name, version);

            // Get all dependencies
            var dependencies = await _storageProvider.GetAllAsync<FunctionDependency>(_collectionName);

            // Filter by name and version
            return dependencies.Where(d => d.Name == name && d.Version == version);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionDependency>> GetByTypeAsync(string type)
        {
            _logger.LogInformation("Getting function dependencies by type: {Type}", type);

            // Get all dependencies
            var dependencies = await _storageProvider.GetAllAsync<FunctionDependency>(_collectionName);

            // Filter by type
            return dependencies.Where(d => d.Type == type);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting function dependency: {Id}", id);

            // Delete from store
            return await _storageProvider.DeleteAsync<FunctionDependency, Guid>(_collectionName, id);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteByFunctionIdAsync(Guid functionId)
        {
            _logger.LogInformation("Deleting function dependencies by function ID: {FunctionId}", functionId);

            // Get dependencies by function ID
            var dependencies = await GetByFunctionIdAsync(functionId);

            // Delete each dependency
            var success = true;
            foreach (var dependency in dependencies)
            {
                var result = await DeleteAsync(dependency.Id);
                if (!result)
                {
                    success = false;
                }
            }

            return success;
        }
    }
}
