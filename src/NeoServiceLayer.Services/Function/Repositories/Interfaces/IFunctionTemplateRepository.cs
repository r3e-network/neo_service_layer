using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Function.Repositories
{
    /// <summary>
    /// Interface for function template repository
    /// </summary>
    public interface IFunctionTemplateRepository
    {
        /// <summary>
        /// Gets a function template by ID
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <returns>The function template if found, null otherwise</returns>
        Task<FunctionTemplate> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets function templates by name
        /// </summary>
        /// <param name="name">Template name</param>
        /// <param name="limit">Maximum number of templates to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function templates with the specified name</returns>
        Task<IEnumerable<FunctionTemplate>> GetByNameAsync(string name, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function templates by runtime
        /// </summary>
        /// <param name="runtime">Template runtime</param>
        /// <param name="limit">Maximum number of templates to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function templates with the specified runtime</returns>
        Task<IEnumerable<FunctionTemplate>> GetByRuntimeAsync(string runtime, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function templates by category
        /// </summary>
        /// <param name="category">Template category</param>
        /// <param name="limit">Maximum number of templates to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function templates with the specified category</returns>
        Task<IEnumerable<FunctionTemplate>> GetByCategoryAsync(string category, int limit = 100, int offset = 0);

        /// <summary>
        /// Gets function templates by tags
        /// </summary>
        /// <param name="tags">Template tags</param>
        /// <param name="limit">Maximum number of templates to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function templates with the specified tags</returns>
        Task<IEnumerable<FunctionTemplate>> GetByTagsAsync(List<string> tags, int limit = 100, int offset = 0);

        /// <summary>
        /// Creates a function template
        /// </summary>
        /// <param name="template">Template to create</param>
        /// <returns>The created function template</returns>
        Task<FunctionTemplate> CreateAsync(FunctionTemplate template);

        /// <summary>
        /// Updates a function template
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <param name="template">Updated template</param>
        /// <returns>The updated function template</returns>
        Task<FunctionTemplate> UpdateAsync(Guid id, FunctionTemplate template);

        /// <summary>
        /// Deletes a function template
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <returns>True if the template was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Gets all function templates
        /// </summary>
        /// <param name="limit">Maximum number of templates to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of all function templates</returns>
        Task<IEnumerable<FunctionTemplate>> GetAllAsync(int limit = 100, int offset = 0);

        /// <summary>
        /// Searches for function templates
        /// </summary>
        /// <param name="query">Search query</param>
        /// <param name="limit">Maximum number of templates to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of function templates matching the search query</returns>
        Task<IEnumerable<FunctionTemplate>> SearchAsync(string query, int limit = 100, int offset = 0);
    }
}
