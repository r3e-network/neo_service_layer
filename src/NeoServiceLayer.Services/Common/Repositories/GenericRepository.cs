using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;

namespace NeoServiceLayer.Services.Common.Repositories
{
    /// <summary>
    /// Generic repository implementation
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <typeparam name="TKey">Key type</typeparam>
    public class GenericRepository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class
    {
        private readonly ILogger _logger;
        private readonly IDatabaseService _databaseService;
        private readonly string _collectionName;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericRepository{TEntity, TKey}"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="databaseService">Database service</param>
        /// <param name="collectionName">Collection name</param>
        public GenericRepository(ILogger logger, IDatabaseService databaseService, string collectionName)
        {
            _logger = logger;
            _databaseService = databaseService;
            _collectionName = collectionName;
        }

        /// <inheritdoc/>
        public async Task<TEntity> GetByIdAsync(TKey id)
        {
            _logger.LogInformation("Getting entity by ID: {Id}", id);
            var provider = await _databaseService.GetDefaultProviderAsync();
            return await provider.GetByIdAsync<TEntity, string>(_collectionName, id.ToString());
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            _logger.LogInformation("Getting all entities");
            var provider = await _databaseService.GetDefaultProviderAsync();
            var entities = await provider.GetAllAsync<TEntity>(_collectionName);
            return entities ?? new List<TEntity>();
        }

        /// <inheritdoc/>
        public async Task<TEntity> AddAsync(TEntity entity)
        {
            _logger.LogInformation("Adding entity");
            var provider = await _databaseService.GetDefaultProviderAsync();
            return await provider.CreateAsync<TEntity>(_collectionName, entity);
        }

        /// <inheritdoc/>
        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            _logger.LogInformation("Updating entity");
            var id = GetEntityId(entity);
            var provider = await _databaseService.GetDefaultProviderAsync();
            return await provider.UpdateAsync<TEntity, string>(_collectionName, id.ToString(), entity);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(TKey id)
        {
            _logger.LogInformation("Deleting entity: {Id}", id);
            var provider = await _databaseService.GetDefaultProviderAsync();
            return await provider.DeleteAsync<TEntity, string>(_collectionName, id.ToString());
        }

        /// <summary>
        /// Gets the entity ID
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>Entity ID</returns>
        private TKey GetEntityId(TEntity entity)
        {
            // Try to get the ID property using reflection
            var idProperty = typeof(TEntity).GetProperty("Id");
            if (idProperty != null)
            {
                return (TKey)idProperty.GetValue(entity);
            }

            throw new InvalidOperationException("Entity does not have an Id property");
        }
    }
}
