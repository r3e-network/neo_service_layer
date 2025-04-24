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
    /// Repository for function compositions
    /// </summary>
    public class FunctionCompositionRepository : IFunctionCompositionRepository
    {
        private readonly ILogger<FunctionCompositionRepository> _logger;
        private readonly IStorageProvider _storageProvider;
        private readonly string _collectionName = "function_compositions";

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionCompositionRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="storageProvider">Storage provider</param>
        public FunctionCompositionRepository(ILogger<FunctionCompositionRepository> logger, IStorageProvider storageProvider)
        {
            _logger = logger;
            _storageProvider = storageProvider;
        }

        /// <inheritdoc/>
        public async Task<FunctionComposition> CreateAsync(FunctionComposition composition)
        {
            _logger.LogInformation("Creating function composition: {Name} for account {AccountId}", composition.Name, composition.AccountId);

            // Ensure ID is set
            if (composition.Id == Guid.Empty)
            {
                composition.Id = Guid.NewGuid();
            }

            // Save to store
            await _storageProvider.CreateAsync(_collectionName, composition);

            return composition;
        }

        /// <inheritdoc/>
        public async Task<FunctionComposition> UpdateAsync(Guid id, FunctionComposition composition)
        {
            _logger.LogInformation("Updating function composition: {Id}", id);

            // Ensure the ID matches
            composition.Id = id;

            // Update in store
            await _storageProvider.UpdateAsync<FunctionComposition, Guid>(_collectionName, id, composition);

            return composition;
        }

        /// <inheritdoc/>
        public async Task<FunctionComposition> UpdateAsync(FunctionComposition composition)
        {
            return await UpdateAsync(composition.Id, composition);
        }

        /// <inheritdoc/>
        public async Task<FunctionComposition> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting function composition by ID: {Id}", id);

            // Get from store
            return await _storageProvider.GetByIdAsync<FunctionComposition, Guid>(_collectionName, id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionComposition>> GetByAccountIdAsync(Guid accountId, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting function compositions by account ID: {AccountId}, limit: {Limit}, offset: {Offset}", accountId, limit, offset);

            // Get all compositions
            var compositions = await _storageProvider.GetAllAsync<FunctionComposition>(_collectionName);

            // Filter by account ID and apply pagination
            return compositions
                .Where(c => c.AccountId == accountId)
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionComposition>> GetByTagsAsync(List<string> tags)
        {
            _logger.LogInformation("Getting function compositions by tags: {Tags}", string.Join(", ", tags));

            // Get all compositions
            var compositions = await _storageProvider.GetAllAsync<FunctionComposition>(_collectionName);

            // Filter by tags
            return compositions.Where(c => c.Tags.Any(t => tags.Contains(t, StringComparer.OrdinalIgnoreCase)));
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting function composition: {Id}", id);

            // Delete from store
            return await _storageProvider.DeleteAsync<FunctionComposition, Guid>(_collectionName, id);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteByAccountIdAsync(Guid accountId)
        {
            _logger.LogInformation("Deleting function compositions by account ID: {AccountId}", accountId);

            // Get compositions by account ID
            var compositions = await GetByAccountIdAsync(accountId);

            // Delete each composition
            var success = true;
            foreach (var composition in compositions)
            {
                var result = await DeleteAsync(composition.Id);
                if (!result)
                {
                    success = false;
                }
            }

            return success;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionComposition>> GetByNameAsync(string name, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting function compositions by name: {Name}, limit: {Limit}, offset: {Offset}", name, limit, offset);

            // Get all compositions
            var compositions = await _storageProvider.GetAllAsync<FunctionComposition>(_collectionName);

            // Filter by name and apply pagination
            return compositions
                .Where(c => c.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionComposition>> GetByFunctionIdAsync(Guid functionId, int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting function compositions by function ID: {FunctionId}, limit: {Limit}, offset: {Offset}", functionId, limit, offset);

            // Get all compositions
            var compositions = await _storageProvider.GetAllAsync<FunctionComposition>(_collectionName);

            // Filter by function ID in steps and apply pagination
            return compositions
                .Where(c => c.Steps != null && c.Steps.Any(s => s.FunctionId == functionId))
                .Skip(offset)
                .Take(limit);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionComposition>> GetAllAsync(int limit = 100, int offset = 0)
        {
            _logger.LogInformation("Getting all function compositions, limit: {Limit}, offset: {Offset}", limit, offset);

            // Get all compositions
            var compositions = await _storageProvider.GetAllAsync<FunctionComposition>(_collectionName);

            // Apply pagination
            return compositions
                .Skip(offset)
                .Take(limit);
        }
    }
}
