using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Enums;
using NeoServiceLayer.Core.Models;
using System.IO;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for function management service
    /// </summary>
    public interface IFunctionService
    {
        /// <summary>
        /// Creates a new function
        /// </summary>
        /// <param name="name">Name for the function</param>
        /// <param name="description">Description of the function</param>
        /// <param name="runtime">Runtime for the function</param>
        /// <param name="sourceCode">Source code of the function</param>
        /// <param name="entryPoint">Entry point for the function</param>
        /// <param name="accountId">Account ID that owns the function</param>
        /// <param name="maxExecutionTime">Maximum execution time in milliseconds</param>
        /// <param name="maxMemory">Maximum memory usage in megabytes</param>
        /// <param name="secretIds">List of secret IDs that the function has access to</param>
        /// <param name="environmentVariables">Environment variables for the function</param>
        /// <returns>The created function</returns>
        Task<Function> CreateFunctionAsync(string name, string description, FunctionRuntime runtime, string sourceCode, string entryPoint, Guid accountId, int maxExecutionTime = 30000, int maxMemory = 128, List<Guid> secretIds = null, Dictionary<string, string> environmentVariables = null);

        /// <summary>
        /// Gets a function by ID
        /// </summary>
        /// <param name="id">Function ID</param>
        /// <returns>The function if found, null otherwise</returns>
        Task<Function> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets a function by ID (alias for GetByIdAsync)
        /// </summary>
        /// <param name="id">Function ID</param>
        /// <returns>The function if found, null otherwise</returns>
        Task<Function> GetFunctionAsync(Guid id);

        /// <summary>
        /// Gets a function by ID string
        /// </summary>
        /// <param name="id">Function ID string</param>
        /// <returns>The function if found, null otherwise</returns>
        Task<Function> GetFunctionAsync(string id);

        /// <summary>
        /// Gets a function by name and account ID
        /// </summary>
        /// <param name="name">Function name</param>
        /// <param name="accountId">Account ID</param>
        /// <returns>The function if found, null otherwise</returns>
        Task<Function> GetByNameAsync(string name, Guid accountId);

        /// <summary>
        /// Gets functions by account ID
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <returns>List of functions owned by the account</returns>
        Task<IEnumerable<Function>> GetByAccountIdAsync(Guid accountId);

        /// <summary>
        /// Gets functions by runtime
        /// </summary>
        /// <param name="runtime">Runtime</param>
        /// <returns>List of functions with the specified runtime</returns>
        Task<IEnumerable<Function>> GetByRuntimeAsync(FunctionRuntime runtime);

        /// <summary>
        /// Updates a function
        /// </summary>
        /// <param name="function">Function to update</param>
        /// <returns>The updated function</returns>
        Task<Function> UpdateAsync(Function function);

        /// <summary>
        /// Updates the source code of a function
        /// </summary>
        /// <param name="id">Function ID</param>
        /// <param name="sourceCode">New source code for the function</param>
        /// <returns>The updated function</returns>
        Task<Function> UpdateSourceCodeAsync(Guid id, string sourceCode);

        /// <summary>
        /// Updates the environment variables of a function
        /// </summary>
        /// <param name="id">Function ID</param>
        /// <param name="environmentVariables">New environment variables for the function</param>
        /// <returns>The updated function</returns>
        Task<Function> UpdateEnvironmentVariablesAsync(Guid id, Dictionary<string, string> environmentVariables);

        /// <summary>
        /// Updates the secret access of a function
        /// </summary>
        /// <param name="id">Function ID</param>
        /// <param name="secretIds">New list of secret IDs that the function has access to</param>
        /// <returns>The updated function</returns>
        Task<Function> UpdateSecretAccessAsync(Guid id, List<Guid> secretIds);

        /// <summary>
        /// Executes a function
        /// </summary>
        /// <param name="id">Function ID</param>
        /// <param name="parameters">Parameters for the function execution</param>
        /// <returns>Result of the function execution</returns>
        Task<object> ExecuteAsync(Guid id, Dictionary<string, object> parameters = null);

        /// <summary>
        /// Executes a function in response to an event
        /// </summary>
        /// <param name="id">Function ID</param>
        /// <param name="eventData">Event data</param>
        /// <returns>Result of the function execution</returns>
        Task<object> ExecuteForEventAsync(Guid id, Event eventData);

        /// <summary>
        /// Gets the execution history of a function
        /// </summary>
        /// <param name="id">Function ID</param>
        /// <param name="startTime">Start time for the history</param>
        /// <param name="endTime">End time for the history</param>
        /// <returns>List of execution records for the function</returns>
        Task<IEnumerable<object>> GetExecutionHistoryAsync(Guid id, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Activates a function
        /// </summary>
        /// <param name="id">Function ID</param>
        /// <returns>The activated function</returns>
        Task<Function> ActivateAsync(Guid id);

        /// <summary>
        /// Deactivates a function
        /// </summary>
        /// <param name="id">Function ID</param>
        /// <returns>The deactivated function</returns>
        Task<Function> DeactivateAsync(Guid id);

        /// <summary>
        /// Deletes a function
        /// </summary>
        /// <param name="id">Function ID</param>
        /// <returns>True if the function was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Gets all function templates
        /// </summary>
        /// <returns>List of function templates</returns>
        Task<IEnumerable<FunctionTemplate>> GetTemplatesAsync();

        /// <summary>
        /// Gets a function template by ID
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <returns>The function template if found, null otherwise</returns>
        Task<FunctionTemplate> GetTemplateByIdAsync(Guid id);

        /// <summary>
        /// Gets function templates by category
        /// </summary>
        /// <param name="category">Category</param>
        /// <returns>List of function templates in the category</returns>
        Task<IEnumerable<FunctionTemplate>> GetTemplatesByCategoryAsync(string category);

        /// <summary>
        /// Gets function templates by runtime
        /// </summary>
        /// <param name="runtime">Runtime</param>
        /// <returns>List of function templates for the runtime</returns>
        Task<IEnumerable<FunctionTemplate>> GetTemplatesByRuntimeAsync(string runtime);

        /// <summary>
        /// Gets function templates by tags
        /// </summary>
        /// <param name="tags">List of tags</param>
        /// <returns>List of function templates with the specified tags</returns>
        Task<IEnumerable<FunctionTemplate>> GetTemplatesByTagsAsync(List<string> tags);

        /// <summary>
        /// Creates a function from a template
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="name">Name for the function</param>
        /// <param name="description">Description of the function</param>
        /// <param name="accountId">Account ID that owns the function</param>
        /// <param name="environmentVariables">Environment variables for the function</param>
        /// <param name="secretIds">List of secret IDs that the function has access to</param>
        /// <param name="maxExecutionTime">Maximum execution time in milliseconds</param>
        /// <param name="maxMemory">Maximum memory usage in megabytes</param>
        /// <returns>The created function</returns>
        Task<Function> CreateFromTemplateAsync(Guid templateId, string name, string description, Guid accountId, Dictionary<string, string> environmentVariables = null, List<Guid> secretIds = null, int maxExecutionTime = 30000, int maxMemory = 128);

        /// <summary>
        /// Creates a new function template
        /// </summary>
        /// <param name="template">Function template to create</param>
        /// <returns>The created function template</returns>
        Task<FunctionTemplate> CreateTemplateAsync(FunctionTemplate template);

        /// <summary>
        /// Updates a function template
        /// </summary>
        /// <param name="template">Function template to update</param>
        /// <returns>The updated function template</returns>
        Task<FunctionTemplate> UpdateTemplateAsync(FunctionTemplate template);

        /// <summary>
        /// Deletes a function template
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <returns>True if the template was deleted successfully, false otherwise</returns>
        Task<bool> DeleteTemplateAsync(Guid id);

        /// <summary>
        /// Processes an uploaded function package
        /// </summary>
        /// <param name="filePath">Path to the uploaded file</param>
        /// <param name="fileName">Original file name</param>
        /// <param name="accountId">Account ID that owns the function</param>
        /// <returns>List of created functions</returns>
        Task<IEnumerable<Function>> ProcessUploadAsync(string filePath, string fileName, Guid accountId);

        /// <summary>
        /// Creates a function from a ZIP file
        /// </summary>
        /// <param name="zipStream">ZIP file stream</param>
        /// <param name="name">Name for the function</param>
        /// <param name="description">Description of the function</param>
        /// <param name="runtime">Runtime for the function</param>
        /// <param name="entryPoint">Entry point for the function</param>
        /// <param name="accountId">Account ID that owns the function</param>
        /// <param name="environmentVariables">Environment variables for the function</param>
        /// <param name="secretIds">List of secret IDs that the function has access to</param>
        /// <param name="maxExecutionTime">Maximum execution time in milliseconds</param>
        /// <param name="maxMemory">Maximum memory usage in megabytes</param>
        /// <returns>The created function</returns>
        Task<Function> CreateFromZipAsync(Stream zipStream, string name, string description, FunctionRuntime runtime, string entryPoint, Guid accountId, Dictionary<string, string> environmentVariables = null, List<Guid> secretIds = null, int maxExecutionTime = 30000, int maxMemory = 128);
    }
}
