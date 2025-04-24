using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Function.Repositories
{
    /// <summary>
    /// Interface for function test repository
    /// </summary>
    public interface IFunctionTestRepository
    {
        /// <summary>
        /// Gets a function test by ID
        /// </summary>
        /// <param name="id">Test ID</param>
        /// <returns>The function test if found, null otherwise</returns>
        Task<FunctionTest> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets function tests by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="limit">Maximum number of tests to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function tests for the specified function</returns>
        Task<IEnumerable<FunctionTest>> GetByFunctionIdAsync(Guid functionId, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function tests by suite ID
        /// </summary>
        /// <param name="suiteId">Suite ID</param>
        /// <param name="limit">Maximum number of tests to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function tests for the specified suite</returns>
        Task<IEnumerable<FunctionTest>> GetBySuiteIdAsync(Guid suiteId, int limit = 100, int offset = 0);

        /// <summary>
        /// Creates a function test
        /// </summary>
        /// <param name="test">Test to create</param>
        /// <returns>The created function test</returns>
        Task<FunctionTest> CreateAsync(FunctionTest test);

        /// <summary>
        /// Updates a function test
        /// </summary>
        /// <param name="id">Test ID</param>
        /// <param name="test">Updated test</param>
        /// <returns>The updated function test</returns>
        Task<FunctionTest> UpdateAsync(Guid id, FunctionTest test);

        /// <summary>
        /// Deletes a function test
        /// </summary>
        /// <param name="id">Test ID</param>
        /// <returns>True if the test was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Gets all function tests
        /// </summary>
        /// <param name="limit">Maximum number of tests to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of all function tests</returns>
        Task<IEnumerable<FunctionTest>> GetAllAsync(int limit = 100, int offset = 0);

        /// <summary>
        /// Counts function tests by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>Number of tests for the specified function</returns>
        Task<int> CountByFunctionIdAsync(Guid functionId);

        /// <summary>
        /// Counts function tests by suite ID
        /// </summary>
        /// <param name="suiteId">Suite ID</param>
        /// <returns>Number of tests for the specified suite</returns>
        Task<int> CountBySuiteIdAsync(Guid suiteId);
    }
}
