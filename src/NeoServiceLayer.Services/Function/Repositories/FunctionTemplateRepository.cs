using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Function.Repositories
{
    /// <summary>
    /// Repository for function templates
    /// </summary>
    public class FunctionTemplateRepository : IFunctionTemplateRepository
    {
        private readonly ILogger<FunctionTemplateRepository> _logger;
        private readonly IStorageProvider _storageProvider;
        private readonly string _collectionName = "function_templates";

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionTemplateRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="storageProvider">Storage provider</param>
        public FunctionTemplateRepository(ILogger<FunctionTemplateRepository> logger, IStorageProvider storageProvider)
        {
            _logger = logger;
            _storageProvider = storageProvider;
        }

        /// <inheritdoc/>
        public async Task<FunctionTemplate> CreateAsync(FunctionTemplate template)
        {
            _logger.LogInformation("Creating function template: {Name}", template.Name);

            // Set default values
            template.Id = Guid.NewGuid();
            template.CreatedAt = DateTime.UtcNow;
            template.UpdatedAt = DateTime.UtcNow;

            // Save to store
            await _storageProvider.CreateAsync(_collectionName, template);

            return template;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting function template: {Id}", id);

            // Delete from store
            return await _storageProvider.DeleteAsync<FunctionTemplate, Guid>(_collectionName, id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionTemplate>> GetAllAsync(int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting all function templates, limit: {Limit}, offset: {Offset}", limit, offset);

            // Get all from store
            var templates = await _storageProvider.GetAllAsync<FunctionTemplate>(_collectionName);

            // Apply pagination
            return templates.Skip(offset).Take(limit);
        }

        /// <inheritdoc/>
        public async Task<FunctionTemplate> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting function template by ID: {Id}", id);

            // Get from store
            return await _storageProvider.GetByIdAsync<FunctionTemplate, Guid>(_collectionName, id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionTemplate>> GetByCategoryAsync(string category, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting function templates by category: {Category}, limit: {Limit}, offset: {Offset}", category, limit, offset);

            // Get all templates
            var templates = await _storageProvider.GetAllAsync<FunctionTemplate>(_collectionName);

            // Filter by category and apply pagination
            return templates
                .Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionTemplate>> GetByRuntimeAsync(string runtime, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting function templates by runtime: {Runtime}, limit: {Limit}, offset: {Offset}", runtime, limit, offset);

            // Get all templates
            var templates = await _storageProvider.GetAllAsync<FunctionTemplate>(_collectionName);

            // Filter by runtime and apply pagination
            return templates
                .Where(t => t.Runtime.Equals(runtime, StringComparison.OrdinalIgnoreCase))
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionTemplate>> GetByTagsAsync(List<string> tags, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting function templates by tags: {Tags}, limit: {Limit}, offset: {Offset}", string.Join(", ", tags), limit, offset);

            // Get all templates
            var templates = await _storageProvider.GetAllAsync<FunctionTemplate>(_collectionName);

            // Filter by tags and apply pagination
            return templates
                .Where(t => t.Tags.Any(tag => tags.Contains(tag, StringComparer.OrdinalIgnoreCase)))
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<FunctionTemplate> UpdateAsync(Guid id, FunctionTemplate template)
        {
            _logger.LogInformation("Updating function template: {Id}", id);

            // Ensure the ID matches
            template.Id = id;

            // Update timestamp
            template.UpdatedAt = DateTime.UtcNow;

            // Update in store
            await _storageProvider.UpdateAsync<FunctionTemplate, Guid>(_collectionName, id, template);

            return template;
        }

        /// <inheritdoc/>
        public async Task<FunctionTemplate> UpdateAsync(FunctionTemplate template)
        {
            return await UpdateAsync(template.Id, template);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionTemplate>> GetByNameAsync(string name, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting function templates by name: {Name}, limit: {Limit}, offset: {Offset}", name, limit, offset);

            // Get all templates
            var templates = await _storageProvider.GetAllAsync<FunctionTemplate>(_collectionName);

            // Filter by name and apply pagination
            return templates
                .Where(t => t.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionTemplate>> SearchAsync(string query, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Searching function templates: {Query}, limit: {Limit}, offset: {Offset}", query, limit, offset);

            // Get all templates
            var templates = await _storageProvider.GetAllAsync<FunctionTemplate>(_collectionName);

            // If query is empty, return all templates with pagination
            if (string.IsNullOrWhiteSpace(query))
            {
                return templates.Skip(offset).Take(limit);
            }

            // Search in name, description, category, runtime, and tags
            return templates
                .Where(t =>
                    t.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    (t.Description != null && t.Description.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                    t.Category.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    t.Runtime.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    t.Tags.Any(tag => tag.Contains(query, StringComparison.OrdinalIgnoreCase)))
                .Skip(offset)
                .Take(limit);
        }
    }
}
