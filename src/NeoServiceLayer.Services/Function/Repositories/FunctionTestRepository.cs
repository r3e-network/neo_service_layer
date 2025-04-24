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
    /// Repository for function tests
    /// </summary>
    public class FunctionTestRepository : IFunctionTestRepository
    {
        private readonly ILogger<FunctionTestRepository> _logger;
        private readonly IStorageProvider _storageProvider;
        private readonly string _collectionName = "function_tests";

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionTestRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="storageProvider">Storage provider</param>
        public FunctionTestRepository(ILogger<FunctionTestRepository> logger, IStorageProvider storageProvider)
        {
            _logger = logger;
            _storageProvider = storageProvider;
        }

        /// <inheritdoc/>
        public async Task<FunctionTest> CreateAsync(FunctionTest test)
        {
            _logger.LogInformation("Creating function test: {Name} for function {FunctionId}", test.Name, test.FunctionId);

            // Ensure ID is set
            if (test.Id == Guid.Empty)
            {
                test.Id = Guid.NewGuid();
            }

            // Save to store
            await _storageProvider.CreateAsync(_collectionName, test);

            return test;
        }

        /// <inheritdoc/>
        public async Task<FunctionTest> UpdateAsync(Guid id, FunctionTest test)
        {
            _logger.LogInformation("Updating function test: {Id}", id);

            // Ensure the ID matches
            test.Id = id;

            // Update in store
            await _storageProvider.UpdateAsync<FunctionTest, Guid>(_collectionName, id, test);

            return test;
        }

        /// <inheritdoc/>
        public async Task<FunctionTest> UpdateAsync(FunctionTest test)
        {
            return await UpdateAsync(test.Id, test);
        }

        /// <inheritdoc/>
        public async Task<FunctionTest> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting function test by ID: {Id}", id);

            // Get from store
            return await _storageProvider.GetByIdAsync<FunctionTest, Guid>(_collectionName, id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionTest>> GetByFunctionIdAsync(Guid functionId, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting function tests by function ID: {FunctionId}, limit: {Limit}, offset: {Offset}", functionId, limit, offset);

            // Get all tests
            var tests = await _storageProvider.GetAllAsync<FunctionTest>(_collectionName);

            // Filter by function ID and apply pagination
            return tests
                .Where(t => t.FunctionId == functionId)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionTest>> GetByTypeAsync(string type)
        {
            _logger.LogInformation("Getting function tests by type: {Type}", type);

            // Get all tests
            var tests = await _storageProvider.GetAllAsync<FunctionTest>(_collectionName);

            // Filter by type
            return tests.Where(t => t.Type.Equals(type, StringComparison.OrdinalIgnoreCase));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionTest>> GetByTagsAsync(List<string> tags)
        {
            _logger.LogInformation("Getting function tests by tags: {Tags}", string.Join(", ", tags));

            // Get all tests
            var tests = await _storageProvider.GetAllAsync<FunctionTest>(_collectionName);

            // Filter by tags
            return tests.Where(t => t.Tags.Any(tag => tags.Contains(tag, StringComparer.OrdinalIgnoreCase)));
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting function test: {Id}", id);

            // Delete from store
            return await _storageProvider.DeleteAsync<FunctionTest, Guid>(_collectionName, id);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteByFunctionIdAsync(Guid functionId)
        {
            _logger.LogInformation("Deleting function tests by function ID: {FunctionId}", functionId);

            // Get tests by function ID
            var tests = await GetByFunctionIdAsync(functionId);

            // Delete each test
            var success = true;
            foreach (var test in tests)
            {
                var result = await DeleteAsync(test.Id);
                if (!result)
                {
                    success = false;
                }
            }

            return success;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionTest>> GetBySuiteIdAsync(Guid suiteId, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting function tests by suite ID: {SuiteId}, limit: {Limit}, offset: {Offset}", suiteId, limit, offset);

            // Get all tests
            var tests = await _storageProvider.GetAllAsync<FunctionTest>(_collectionName);

            // Filter by suite ID and apply pagination
            return tests
                .Where(t => t.SuiteId == suiteId)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionTest>> GetAllAsync(int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting all function tests, limit: {Limit}, offset: {Offset}", limit, offset);

            // Get all tests
            var tests = await _storageProvider.GetAllAsync<FunctionTest>(_collectionName);

            // Apply pagination
            return tests
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<int> CountByFunctionIdAsync(Guid functionId)
        {
            _logger.LogInformation("Counting function tests by function ID: {FunctionId}", functionId);

            // Get all tests
            var tests = await _storageProvider.GetAllAsync<FunctionTest>(_collectionName);

            // Count tests by function ID
            return tests.Count(t => t.FunctionId == functionId);
        }

        /// <inheritdoc/>
        public async Task<int> CountBySuiteIdAsync(Guid suiteId)
        {
            _logger.LogInformation("Counting function tests by suite ID: {SuiteId}", suiteId);

            // Get all tests
            var tests = await _storageProvider.GetAllAsync<FunctionTest>(_collectionName);

            // Count tests by suite ID
            return tests.Count(t => t.SuiteId == suiteId);
        }
    }
}
