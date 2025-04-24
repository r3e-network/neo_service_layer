using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for function test service
    /// </summary>
    public interface IFunctionTestService
    {
        /// <summary>
        /// Creates a new function test
        /// </summary>
        /// <param name="test">Function test to create</param>
        /// <returns>The created function test</returns>
        Task<FunctionTest> CreateTestAsync(FunctionTest test);

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
        Task<FunctionTest> UpdateTestAsync(FunctionTest test);

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
        Task<FunctionTest> GetTestByIdAsync(Guid id);

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
        Task<IEnumerable<FunctionTest>> GetTestsByFunctionIdAsync(Guid functionId);

        /// <summary>
        /// Gets function tests by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>List of function tests</returns>
        Task<IEnumerable<FunctionTest>> GetByFunctionIdAsync(Guid functionId);

        /// <summary>
        /// Deletes a function test
        /// </summary>
        /// <param name="id">Test ID</param>
        /// <returns>True if the test was deleted successfully, false otherwise</returns>
        Task<bool> DeleteTestAsync(Guid id);

        /// <summary>
        /// Deletes a function test
        /// </summary>
        /// <param name="id">Test ID</param>
        /// <returns>True if the test was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Runs a function test
        /// </summary>
        /// <param name="testId">Test ID</param>
        /// <param name="functionVersion">Function version</param>
        /// <returns>The test result</returns>
        Task<FunctionTestResult> RunTestAsync(Guid testId, string functionVersion = null);

        /// <summary>
        /// Runs function tests by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="functionVersion">Function version</param>
        /// <returns>List of test results</returns>
        Task<IEnumerable<FunctionTestResult>> RunTestsByFunctionIdAsync(Guid functionId, string functionVersion = null);

        /// <summary>
        /// Gets a function test result by ID
        /// </summary>
        /// <param name="id">Result ID</param>
        /// <returns>The function test result if found, null otherwise</returns>
        Task<FunctionTestResult> GetTestResultByIdAsync(Guid id);

        /// <summary>
        /// Gets the latest test result for a function test
        /// </summary>
        /// <param name="testId">Test ID</param>
        /// <returns>The latest test result if found, null otherwise</returns>
        Task<FunctionTestResult> GetLatestTestResultAsync(Guid testId);

        /// <summary>
        /// Gets function test results by test ID
        /// </summary>
        /// <param name="testId">Test ID</param>
        /// <param name="limit">Maximum number of results to return</param>
        /// <param name="offset">Number of results to skip</param>
        /// <returns>List of function test results</returns>
        Task<IEnumerable<FunctionTestResult>> GetTestResultsByTestIdAsync(Guid testId, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets function test results by test ID
        /// </summary>
        /// <param name="testId">Test ID</param>
        /// <param name="limit">Maximum number of results to return</param>
        /// <param name="offset">Number of results to skip</param>
        /// <returns>List of function test results</returns>
        Task<IEnumerable<FunctionTestResult>> GetTestResultsAsync(Guid testId, int limit = 10, int offset = 0);

        /// <summary>
        /// Gets function test results by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="limit">Maximum number of results to return</param>
        /// <param name="offset">Number of results to skip</param>
        /// <returns>List of function test results</returns>
        Task<IEnumerable<FunctionTestResult>> GetTestResultsByFunctionIdAsync(Guid functionId, int limit = 10, int offset = 0);

        /// <summary>
        /// Creates a new function test suite
        /// </summary>
        /// <param name="suite">Function test suite to create</param>
        /// <returns>The created function test suite</returns>
        Task<FunctionTestSuite> CreateTestSuiteAsync(FunctionTestSuite suite);

        /// <summary>
        /// Updates a function test suite
        /// </summary>
        /// <param name="suite">Function test suite to update</param>
        /// <returns>The updated function test suite</returns>
        Task<FunctionTestSuite> UpdateTestSuiteAsync(FunctionTestSuite suite);

        /// <summary>
        /// Gets a function test suite by ID
        /// </summary>
        /// <param name="id">Suite ID</param>
        /// <returns>The function test suite if found, null otherwise</returns>
        Task<FunctionTestSuite> GetTestSuiteByIdAsync(Guid id);

        /// <summary>
        /// Gets function test suites by function ID
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>List of function test suites</returns>
        Task<IEnumerable<FunctionTestSuite>> GetTestSuitesByFunctionIdAsync(Guid functionId);

        /// <summary>
        /// Deletes a function test suite
        /// </summary>
        /// <param name="id">Suite ID</param>
        /// <returns>True if the suite was deleted successfully, false otherwise</returns>
        Task<bool> DeleteTestSuiteAsync(Guid id);

        /// <summary>
        /// Runs a function test suite
        /// </summary>
        /// <param name="suiteId">Suite ID</param>
        /// <param name="functionVersion">Function version</param>
        /// <returns>List of test results</returns>
        Task<IEnumerable<FunctionTestResult>> RunTestSuiteAsync(Guid suiteId, string functionVersion = null);

        /// <summary>
        /// Adds a test to a test suite
        /// </summary>
        /// <param name="suiteId">Suite ID</param>
        /// <param name="testId">Test ID</param>
        /// <returns>The updated function test suite</returns>
        Task<FunctionTestSuite> AddTestToSuiteAsync(Guid suiteId, Guid testId);

        /// <summary>
        /// Removes a test from a test suite
        /// </summary>
        /// <param name="suiteId">Suite ID</param>
        /// <param name="testId">Test ID</param>
        /// <returns>The updated function test suite</returns>
        Task<FunctionTestSuite> RemoveTestFromSuiteAsync(Guid suiteId, Guid testId);

        /// <summary>
        /// Generates tests for a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>List of generated function tests</returns>
        Task<IEnumerable<FunctionTest>> GenerateTestsAsync(Guid functionId);

        /// <summary>
        /// Validates a function test
        /// </summary>
        /// <param name="test">Function test to validate</param>
        /// <returns>List of validation errors</returns>
        Task<IEnumerable<string>> ValidateTestAsync(FunctionTest test);

        /// <summary>
        /// Gets test coverage for a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>Test coverage information</returns>
        Task<object> GetTestCoverageAsync(Guid functionId);

        /// <summary>
        /// Imports tests from a file
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="fileContent">File content</param>
        /// <param name="fileType">File type (e.g., "json", "yaml")</param>
        /// <returns>List of imported function tests</returns>
        Task<IEnumerable<FunctionTest>> ImportTestsAsync(Guid functionId, string fileContent, string fileType);

        /// <summary>
        /// Exports tests to a file
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="fileType">File type (e.g., "json", "yaml")</param>
        /// <returns>Exported file content</returns>
        Task<string> ExportTestsAsync(Guid functionId, string fileType);
    }
}
