using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Function.Repositories
{
    /// <summary>
    /// Interface for function test suite repository
    /// </summary>
    public interface IFunctionTestSuiteRepository
    {
        /// <summary>
        /// Gets a function test suite by ID
        /// </summary>
        /// <param name="id">Suite ID</param>
        /// <returns>The function test suite if found, null otherwise</returns>
        Task<FunctionTestSuite> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets function test suites by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="limit">Maximum number of suites to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function test suites for the specified function</returns>
        Task<IEnumerable<FunctionTestSuite>> GetByFunctionIdAsync(Guid functionId, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function test suites by name
        /// </summary>
        /// <param name="name">Suite name</param>
        /// <param name="limit">Maximum number of suites to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function test suites with the specified name</returns>
        Task<IEnumerable<FunctionTestSuite>> GetByNameAsync(string name, int limit = 100, int offset = 0);

        /// <summary>
        /// Creates a function test suite
        /// </summary>
        /// <param name="suite">Suite to create</param>
        /// <returns>The created function test suite</returns>
        Task<FunctionTestSuite> CreateAsync(FunctionTestSuite suite);

        /// <summary>
        /// Updates a function test suite
        /// </summary>
        /// <param name="id">Suite ID</param>
        /// <param name="suite">Updated suite</param>
        /// <returns>The updated function test suite</returns>
        Task<FunctionTestSuite> UpdateAsync(Guid id, FunctionTestSuite suite);

        /// <summary>
        /// Deletes a function test suite
        /// </summary>
        /// <param name="id">Suite ID</param>
        /// <returns>True if the suite was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Gets all function test suites
        /// </summary>
        /// <param name="limit">Maximum number of suites to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of all function test suites</returns>
        Task<IEnumerable<FunctionTestSuite>> GetAllAsync(int limit = 100, int offset = 0);

        /// <summary>
        /// Counts function test suites by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>Number of suites for the specified function</returns>
        Task<int> CountByFunctionIdAsync(Guid functionId);
    }
}
