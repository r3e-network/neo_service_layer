using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for function test suite repository
    /// </summary>
    public interface IFunctionTestSuiteRepository
    {
        /// <summary>
        /// Creates a new function test suite
        /// </summary>
        /// <param name="suite">Function test suite to create</param>
        /// <returns>The created function test suite</returns>
        Task<FunctionTestSuite> CreateAsync(FunctionTestSuite suite);

        /// <summary>
        /// Updates a function test suite
        /// </summary>
        /// <param name="suite">Function test suite to update</param>
        /// <returns>The updated function test suite</returns>
        Task<FunctionTestSuite> UpdateAsync(FunctionTestSuite suite);

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
        /// <returns>List of function test suites</returns>
        Task<IEnumerable<FunctionTestSuite>> GetByFunctionIdAsync(Guid functionId);

        /// <summary>
        /// Gets function test suites by tags
        /// </summary>
        /// <param name="tags">Suite tags</param>
        /// <returns>List of function test suites</returns>
        Task<IEnumerable<FunctionTestSuite>> GetByTagsAsync(List<string> tags);

        /// <summary>
        /// Deletes a function test suite
        /// </summary>
        /// <param name="id">Suite ID</param>
        /// <returns>True if the suite was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Deletes function test suites by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>True if the suites were deleted successfully, false otherwise</returns>
        Task<bool> DeleteByFunctionIdAsync(Guid functionId);
    }
}
