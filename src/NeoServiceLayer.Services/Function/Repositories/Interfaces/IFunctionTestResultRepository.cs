using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Function.Repositories
{
    /// <summary>
    /// Interface for function test result repository
    /// </summary>
    public interface IFunctionTestResultRepository
    {
        /// <summary>
        /// Gets a function test result by ID
        /// </summary>
        /// <param name="id">Result ID</param>
        /// <returns>The function test result if found, null otherwise</returns>
        Task<FunctionTestResult> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets function test results by test ID
        /// </summary>
        /// <param name="testId">Test ID</param>
        /// <param name="limit">Maximum number of results to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function test results for the specified test</returns>
        Task<IEnumerable<FunctionTestResult>> GetByTestIdAsync(Guid testId, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function test results by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="limit">Maximum number of results to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function test results for the specified function</returns>
        Task<IEnumerable<FunctionTestResult>> GetByFunctionIdAsync(Guid functionId, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function test results by suite ID
        /// </summary>
        /// <param name="suiteId">Suite ID</param>
        /// <param name="limit">Maximum number of results to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function test results for the specified suite</returns>
        Task<IEnumerable<FunctionTestResult>> GetBySuiteIdAsync(Guid suiteId, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function test results by status
        /// </summary>
        /// <param name="status">Result status</param>
        /// <param name="limit">Maximum number of results to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function test results with the specified status</returns>
        Task<IEnumerable<FunctionTestResult>> GetByStatusAsync(string status, int limit = 100, int offset = 0);

        /// <summary>
        /// Creates a function test result
        /// </summary>
        /// <param name="result">Result to create</param>
        /// <returns>The created function test result</returns>
        Task<FunctionTestResult> CreateAsync(FunctionTestResult result);

        /// <summary>
        /// Updates a function test result
        /// </summary>
        /// <param name="id">Result ID</param>
        /// <param name="result">Updated result</param>
        /// <returns>The updated function test result</returns>
        Task<FunctionTestResult> UpdateAsync(Guid id, FunctionTestResult result);

        /// <summary>
        /// Deletes a function test result
        /// </summary>
        /// <param name="id">Result ID</param>
        /// <returns>True if the result was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Gets all function test results
        /// </summary>
        /// <param name="limit">Maximum number of results to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of all function test results</returns>
        Task<IEnumerable<FunctionTestResult>> GetAllAsync(int limit = 100, int offset = 0);

        /// <summary>
        /// Gets the latest function test result for a test
        /// </summary>
        /// <param name="testId">Test ID</param>
        /// <returns>The latest function test result for the specified test</returns>
        Task<FunctionTestResult> GetLatestByTestIdAsync(Guid testId);

        /// <summary>
        /// Gets the latest function test results for a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="limit">Maximum number of results to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>The latest function test results for the specified function</returns>
        Task<IEnumerable<FunctionTestResult>> GetLatestByFunctionIdAsync(Guid functionId, int limit = 100, int offset = 0);
    }
}
