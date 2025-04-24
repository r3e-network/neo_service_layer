using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for function test result repository
    /// </summary>
    public interface IFunctionTestResultRepository
    {
        /// <summary>
        /// Creates a new function test result
        /// </summary>
        /// <param name="result">Function test result to create</param>
        /// <returns>The created function test result</returns>
        Task<FunctionTestResult> CreateAsync(FunctionTestResult result);

        /// <summary>
        /// Updates a function test result
        /// </summary>
        /// <param name="result">Function test result to update</param>
        /// <returns>The updated function test result</returns>
        Task<FunctionTestResult> UpdateAsync(FunctionTestResult result);

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
        /// <param name="offset">Number of results to skip</param>
        /// <returns>List of function test results</returns>
        Task<IEnumerable<FunctionTestResult>> GetByTestIdAsync(Guid testId, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets function test results by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="limit">Maximum number of results to return</param>
        /// <param name="offset">Number of results to skip</param>
        /// <returns>List of function test results</returns>
        Task<IEnumerable<FunctionTestResult>> GetByFunctionIdAsync(Guid functionId, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets function test results by status
        /// </summary>
        /// <param name="status">Result status</param>
        /// <param name="limit">Maximum number of results to return</param>
        /// <param name="offset">Number of results to skip</param>
        /// <returns>List of function test results</returns>
        Task<IEnumerable<FunctionTestResult>> GetByStatusAsync(string status, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets function test results by function ID and status
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="status">Result status</param>
        /// <param name="limit">Maximum number of results to return</param>
        /// <param name="offset">Number of results to skip</param>
        /// <returns>List of function test results</returns>
        Task<IEnumerable<FunctionTestResult>> GetByFunctionIdAndStatusAsync(Guid functionId, string status, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets the latest function test result by test ID
        /// </summary>
        /// <param name="testId">Test ID</param>
        /// <returns>The latest function test result if found, null otherwise</returns>
        Task<FunctionTestResult> GetLatestByTestIdAsync(Guid testId);

        /// <summary>
        /// Gets the latest function test results by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>List of the latest function test results</returns>
        Task<IEnumerable<FunctionTestResult>> GetLatestByFunctionIdAsync(Guid functionId);

        /// <summary>
        /// Deletes a function test result
        /// </summary>
        /// <param name="id">Result ID</param>
        /// <returns>True if the result was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Deletes function test results by test ID
        /// </summary>
        /// <param name="testId">Test ID</param>
        /// <returns>True if the results were deleted successfully, false otherwise</returns>
        Task<bool> DeleteByTestIdAsync(Guid testId);

        /// <summary>
        /// Deletes function test results by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>True if the results were deleted successfully, false otherwise</returns>
        Task<bool> DeleteByFunctionIdAsync(Guid functionId);
    }
}
