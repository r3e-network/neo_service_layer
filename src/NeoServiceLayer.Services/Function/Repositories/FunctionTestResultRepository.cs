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
    /// Repository for function test results
    /// </summary>
    public class FunctionTestResultRepository : IFunctionTestResultRepository
    {
        private readonly ILogger<FunctionTestResultRepository> _logger;
        private readonly IStorageProvider _storageProvider;
        private readonly string _collectionName = "function_test_results";

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionTestResultRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="storageProvider">Storage provider</param>
        public FunctionTestResultRepository(ILogger<FunctionTestResultRepository> logger, IStorageProvider storageProvider)
        {
            _logger = logger;
            _storageProvider = storageProvider;
        }

        /// <inheritdoc/>
        public async Task<FunctionTestResult> CreateAsync(FunctionTestResult result)
        {
            _logger.LogInformation("Creating function test result for test: {TestId}", result.TestId);

            // Ensure ID is set
            if (result.Id == Guid.Empty)
            {
                result.Id = Guid.NewGuid();
            }

            // Save to store
            await _storageProvider.CreateAsync(_collectionName, result);

            return result;
        }

        /// <inheritdoc/>
        public async Task<FunctionTestResult> UpdateAsync(Guid id, FunctionTestResult result)
        {
            _logger.LogInformation("Updating function test result: {Id}", id);

            // Ensure the ID matches
            result.Id = id;

            // Update in store
            await _storageProvider.UpdateAsync<FunctionTestResult, Guid>(_collectionName, id, result);

            return result;
        }

        /// <inheritdoc/>
        public async Task<FunctionTestResult> UpdateAsync(FunctionTestResult result)
        {
            return await UpdateAsync(result.Id, result);
        }

        /// <inheritdoc/>
        public async Task<FunctionTestResult> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting function test result by ID: {Id}", id);

            // Get from store
            return await _storageProvider.GetByIdAsync<FunctionTestResult, Guid>(_collectionName, id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionTestResult>> GetByTestIdAsync(Guid testId, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting function test results by test ID: {TestId}, limit: {Limit}, offset: {Offset}", testId, limit, offset);

            // Get all results
            var results = await _storageProvider.GetAllAsync<FunctionTestResult>(_collectionName);

            // Filter by test ID and sort by start time descending
            return results
                .Where(r => r.TestId == testId)
                .OrderByDescending(r => r.StartTime)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionTestResult>> GetByFunctionIdAsync(Guid functionId, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting function test results by function ID: {FunctionId}, limit: {Limit}, offset: {Offset}", functionId, limit, offset);

            // Get all results
            var results = await _storageProvider.GetAllAsync<FunctionTestResult>(_collectionName);

            // Filter by function ID and sort by start time descending
            return results
                .Where(r => r.FunctionId == functionId)
                .OrderByDescending(r => r.StartTime)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionTestResult>> GetByStatusAsync(string status, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting function test results by status: {Status}, limit: {Limit}, offset: {Offset}", status, limit, offset);

            // Get all results
            var results = await _storageProvider.GetAllAsync<FunctionTestResult>(_collectionName);

            // Filter by status and sort by start time descending
            return results
                .Where(r => r.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(r => r.StartTime)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionTestResult>> GetByFunctionIdAndStatusAsync(Guid functionId, string status, int limit = 10, int offset = 0)
        {
            _logger.LogInformation("Getting function test results by function ID: {FunctionId} and status: {Status}, limit: {Limit}, offset: {Offset}", functionId, status, limit, offset);

            // Get all results
            var results = await _storageProvider.GetAllAsync<FunctionTestResult>(_collectionName);

            // Filter by function ID and status, and sort by start time descending
            return results
                .Where(r => r.FunctionId == functionId && r.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(r => r.StartTime)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<FunctionTestResult> GetLatestByTestIdAsync(Guid testId)
        {
            _logger.LogInformation("Getting latest function test result by test ID: {TestId}", testId);

            // Get all results
            var results = await _storageProvider.GetAllAsync<FunctionTestResult>(_collectionName);

            // Filter by test ID and get the latest by start time
            return results
                .Where(r => r.TestId == testId)
                .OrderByDescending(r => r.StartTime)
                .FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionTestResult>> GetLatestByFunctionIdAsync(Guid functionId, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting latest function test results by function ID: {FunctionId}, limit: {Limit}, offset: {Offset}", functionId, limit, offset);

            // Get all results
            var results = await _storageProvider.GetAllAsync<FunctionTestResult>(_collectionName);

            // Group by test ID, get the latest result for each test, and apply pagination
            return results
                .Where(r => r.FunctionId == functionId)
                .GroupBy(r => r.TestId)
                .Select(g => g.OrderByDescending(r => r.StartTime).First())
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting function test result: {Id}", id);

            // Delete from store
            return await _storageProvider.DeleteAsync<FunctionTestResult, Guid>(_collectionName, id);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteByTestIdAsync(Guid testId)
        {
            _logger.LogInformation("Deleting function test results by test ID: {TestId}", testId);

            // Get results by test ID
            var results = await GetByTestIdAsync(testId, int.MaxValue, 0);

            // Delete each result
            var success = true;
            foreach (var result in results)
            {
                var deleteResult = await DeleteAsync(result.Id);
                if (!deleteResult)
                {
                    success = false;
                }
            }

            return success;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteByFunctionIdAsync(Guid functionId)
        {
            _logger.LogInformation("Deleting function test results by function ID: {FunctionId}", functionId);

            // Get results by function ID
            var results = await GetByFunctionIdAsync(functionId, int.MaxValue, 0);

            // Delete each result
            var success = true;
            foreach (var result in results)
            {
                var deleteResult = await DeleteAsync(result.Id);
                if (!deleteResult)
                {
                    success = false;
                }
            }

            return success;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionTestResult>> GetBySuiteIdAsync(Guid suiteId, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting function test results by suite ID: {SuiteId}, limit: {Limit}, offset: {Offset}", suiteId, limit, offset);

            // Get all results
            var results = await _storageProvider.GetAllAsync<FunctionTestResult>(_collectionName);

            // Filter by suite ID and sort by start time descending
            return results
                .Where(r => r.SuiteId == suiteId)
                .OrderByDescending(r => r.StartTime)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionTestResult>> GetAllAsync(int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting all function test results, limit: {Limit}, offset: {Offset}", limit, offset);

            // Get all results
            var results = await _storageProvider.GetAllAsync<FunctionTestResult>(_collectionName);

            // Sort by start time descending and apply pagination
            return results
                .OrderByDescending(r => r.StartTime)
                .Skip(offset)
                .Take(limit);
        }
    }
}
