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
    /// Repository for function test suites
    /// </summary>
    public class FunctionTestSuiteRepository : IFunctionTestSuiteRepository
    {
        private readonly ILogger<FunctionTestSuiteRepository> _logger;
        private readonly IStorageProvider _storageProvider;
        private readonly string _collectionName = "function_test_suites";

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionTestSuiteRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="storageProvider">Storage provider</param>
        public FunctionTestSuiteRepository(ILogger<FunctionTestSuiteRepository> logger, IStorageProvider storageProvider)
        {
            _logger = logger;
            _storageProvider = storageProvider;
        }

        /// <inheritdoc/>
        public async Task<FunctionTestSuite> CreateAsync(FunctionTestSuite suite)
        {
            _logger.LogInformation("Creating function test suite: {Name} for function {FunctionId}", suite.Name, suite.FunctionId);

            // Ensure ID is set
            if (suite.Id == Guid.Empty)
            {
                suite.Id = Guid.NewGuid();
            }

            // Save to store
            await _storageProvider.CreateAsync(_collectionName, suite);

            return suite;
        }

        /// <inheritdoc/>
        public async Task<FunctionTestSuite> UpdateAsync(Guid id, FunctionTestSuite suite)
        {
            _logger.LogInformation("Updating function test suite: {Id}", id);

            // Ensure the ID matches
            suite.Id = id;

            // Update in store
            await _storageProvider.UpdateAsync<FunctionTestSuite, Guid>(_collectionName, id, suite);

            return suite;
        }

        /// <inheritdoc/>
        public async Task<FunctionTestSuite> UpdateAsync(FunctionTestSuite suite)
        {
            return await UpdateAsync(suite.Id, suite);
        }

        /// <inheritdoc/>
        public async Task<FunctionTestSuite> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting function test suite by ID: {Id}", id);

            // Get from store
            return await _storageProvider.GetByIdAsync<FunctionTestSuite, Guid>(_collectionName, id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionTestSuite>> GetByFunctionIdAsync(Guid functionId, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting function test suites by function ID: {FunctionId}, limit: {Limit}, offset: {Offset}", functionId, limit, offset);

            // Get all suites
            var suites = await _storageProvider.GetAllAsync<FunctionTestSuite>(_collectionName);

            // Filter by function ID and apply pagination
            return suites
                .Where(s => s.FunctionId == functionId)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionTestSuite>> GetByTagsAsync(List<string> tags)
        {
            _logger.LogInformation("Getting function test suites by tags: {Tags}", string.Join(", ", tags));

            // Get all suites
            var suites = await _storageProvider.GetAllAsync<FunctionTestSuite>(_collectionName);

            // Filter by tags
            return suites.Where(s => s.Tags.Any(tag => tags.Contains(tag, StringComparer.OrdinalIgnoreCase)));
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting function test suite: {Id}", id);

            // Delete from store
            return await _storageProvider.DeleteAsync<FunctionTestSuite, Guid>(_collectionName, id);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteByFunctionIdAsync(Guid functionId)
        {
            _logger.LogInformation("Deleting function test suites by function ID: {FunctionId}", functionId);

            // Get suites by function ID
            var suites = await GetByFunctionIdAsync(functionId);

            // Delete each suite
            var success = true;
            foreach (var suite in suites)
            {
                var result = await DeleteAsync(suite.Id);
                if (!result)
                {
                    success = false;
                }
            }

            return success;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionTestSuite>> GetByNameAsync(string name, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting function test suites by name: {Name}, limit: {Limit}, offset: {Offset}", name, limit, offset);

            // Get all suites
            var suites = await _storageProvider.GetAllAsync<FunctionTestSuite>(_collectionName);

            // Filter by name and apply pagination
            return suites
                .Where(s => s.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionTestSuite>> GetAllAsync(int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting all function test suites, limit: {Limit}, offset: {Offset}", limit, offset);

            // Get all suites
            var suites = await _storageProvider.GetAllAsync<FunctionTestSuite>(_collectionName);

            // Apply pagination
            return suites
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<int> CountByFunctionIdAsync(Guid functionId)
        {
            _logger.LogInformation("Counting function test suites by function ID: {FunctionId}", functionId);

            // Get all suites
            var suites = await _storageProvider.GetAllAsync<FunctionTestSuite>(_collectionName);

            // Count suites by function ID
            return suites.Count(s => s.FunctionId == functionId);
        }
    }
}
