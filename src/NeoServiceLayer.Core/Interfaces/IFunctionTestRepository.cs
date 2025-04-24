using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for function test repository
    /// </summary>
    public interface IFunctionTestRepository
    {
        /// <summary>
        /// Creates a new function test
        /// </summary>
        /// <param name="test">Function test to create</param>
        /// <returns>The created function test</returns>
        Task<FunctionTest> CreateAsync(FunctionTest test);

        /// <summary>
        /// Updates a function test
        /// </summary>
        /// <param name="test">Function test to update</param>
        /// <returns>The updated function test</returns>
        Task<FunctionTest> UpdateAsync(FunctionTest test);

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
        /// <returns>List of function tests</returns>
        Task<IEnumerable<FunctionTest>> GetByFunctionIdAsync(Guid functionId);

        /// <summary>
        /// Gets function tests by type
        /// </summary>
        /// <param name="type">Test type</param>
        /// <returns>List of function tests</returns>
        Task<IEnumerable<FunctionTest>> GetByTypeAsync(string type);

        /// <summary>
        /// Gets function tests by tags
        /// </summary>
        /// <param name="tags">Test tags</param>
        /// <returns>List of function tests</returns>
        Task<IEnumerable<FunctionTest>> GetByTagsAsync(List<string> tags);

        /// <summary>
        /// Deletes a function test
        /// </summary>
        /// <param name="id">Test ID</param>
        /// <returns>True if the test was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Deletes function tests by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>True if the tests were deleted successfully, false otherwise</returns>
        Task<bool> DeleteByFunctionIdAsync(Guid functionId);
    }
}
