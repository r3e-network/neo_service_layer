using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for function template repository
    /// </summary>
    public interface IFunctionTemplateRepository
    {
        /// <summary>
        /// Gets all function templates
        /// </summary>
        /// <returns>List of function templates</returns>
        Task<IEnumerable<FunctionTemplate>> GetAllAsync();

        /// <summary>
        /// Gets a function template by ID
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <returns>The function template if found, null otherwise</returns>
        Task<FunctionTemplate> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets function templates by category
        /// </summary>
        /// <param name="category">Category</param>
        /// <returns>List of function templates in the category</returns>
        Task<IEnumerable<FunctionTemplate>> GetByCategoryAsync(string category);

        /// <summary>
        /// Gets function templates by runtime
        /// </summary>
        /// <param name="runtime">Runtime</param>
        /// <returns>List of function templates for the runtime</returns>
        Task<IEnumerable<FunctionTemplate>> GetByRuntimeAsync(string runtime);

        /// <summary>
        /// Gets function templates by tags
        /// </summary>
        /// <param name="tags">List of tags</param>
        /// <returns>List of function templates with the specified tags</returns>
        Task<IEnumerable<FunctionTemplate>> GetByTagsAsync(List<string> tags);

        /// <summary>
        /// Creates a new function template
        /// </summary>
        /// <param name="template">Function template to create</param>
        /// <returns>The created function template</returns>
        Task<FunctionTemplate> CreateAsync(FunctionTemplate template);

        /// <summary>
        /// Updates a function template
        /// </summary>
        /// <param name="template">Function template to update</param>
        /// <returns>The updated function template</returns>
        Task<FunctionTemplate> UpdateAsync(FunctionTemplate template);

        /// <summary>
        /// Deletes a function template
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <returns>True if the template was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);
    }
}
