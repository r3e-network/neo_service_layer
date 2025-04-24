using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;

namespace NeoServiceLayer.Services.Storage.Repositories
{
    /// <summary>
    /// Base repository implementation using the Storage service
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="TKey">Key type</typeparam>
    public abstract class BaseRepository<T, TKey> : IRepository<T, TKey> where T : class
    {
        private readonly ILogger _logger;
        private readonly IStorageService _storageService;
        private readonly Guid _accountId;
        private readonly string _entityName;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseRepository{T, TKey}"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="storageService">Storage service</param>
        /// <param name="accountId">Account ID</param>
        /// <param name="entityName">Entity name</param>
        protected BaseRepository(
            ILogger logger,
            IStorageService storageService,
            Guid accountId,
            string entityName)
        {
            _logger = logger;
            _storageService = storageService;
            _accountId = accountId;
            _entityName = entityName;
        }

        /// <summary>
        /// Gets the ID from an entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>Entity ID</returns>
        protected abstract TKey GetId(T entity);

        /// <summary>
        /// Sets the ID on an entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="id">ID to set</param>
        protected abstract void SetId(T entity, TKey id);

        /// <inheritdoc/>
        public virtual async Task<T> GetByIdAsync(TKey id)
        {
            try
            {
                var key = GetKey(id);
                var json = await _storageService.RetrieveKeyValueAsync(_accountId, null, key);
                if (string.IsNullOrEmpty(json))
                {
                    return null;
                }

                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting {EntityName} by ID: {Id}", _entityName, id);
                throw;
            }
        }

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                var result = new List<T>();
                var keys = await _storageService.ListKeysAsync(_accountId);

                foreach (var key in keys)
                {
                    if (key.StartsWith($"{_entityName}/"))
                    {
                        var json = await _storageService.RetrieveKeyValueAsync(_accountId, null, key);
                        if (!string.IsNullOrEmpty(json))
                        {
                            var entity = JsonSerializer.Deserialize<T>(json);
                            result.Add(entity);
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all {EntityName}s", _entityName);
                throw;
            }
        }

        /// <inheritdoc/>
        public virtual async Task<T> CreateAsync(T entity)
        {
            try
            {
                var id = GetId(entity);
                if (EqualityComparer<TKey>.Default.Equals(id, default))
                {
                    id = GenerateId();
                    SetId(entity, id);
                }

                var key = GetKey(id);
                var json = JsonSerializer.Serialize(entity);
                await _storageService.StoreKeyValueAsync(_accountId, null, key, json);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating {EntityName}", _entityName);
                throw;
            }
        }

        /// <inheritdoc/>
        public virtual async Task<T> UpdateAsync(T entity)
        {
            try
            {
                var id = GetId(entity);
                var key = GetKey(id);

                // Check if entity exists
                var exists = await ExistsAsync(id);
                if (!exists)
                {
                    throw new KeyNotFoundException($"{_entityName} with ID {id} not found");
                }

                var json = JsonSerializer.Serialize(entity);
                await _storageService.StoreKeyValueAsync(_accountId, null, key, json);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating {EntityName} with ID: {Id}", _entityName, GetId(entity));
                throw;
            }
        }

        /// <inheritdoc/>
        public virtual async Task<bool> DeleteAsync(TKey id)
        {
            try
            {
                var key = GetKey(id);
                return await _storageService.DeleteKeyValueAsync(_accountId, null, key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting {EntityName} with ID: {Id}", _entityName, id);
                throw;
            }
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ExistsAsync(TKey id)
        {
            try
            {
                var key = GetKey(id);
                var json = await _storageService.RetrieveKeyValueAsync(_accountId, null, key);
                return !string.IsNullOrEmpty(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if {EntityName} with ID: {Id} exists", _entityName, id);
                throw;
            }
        }

        /// <summary>
        /// Gets the storage key for an entity ID
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <returns>Storage key</returns>
        protected virtual string GetKey(TKey id)
        {
            return $"{_entityName}/{id}";
        }

        /// <summary>
        /// Generates a new ID
        /// </summary>
        /// <returns>New ID</returns>
        protected virtual TKey GenerateId()
        {
            if (typeof(TKey) == typeof(Guid))
            {
                return (TKey)(object)Guid.NewGuid();
            }
            else if (typeof(TKey) == typeof(int) || typeof(TKey) == typeof(long))
            {
                return (TKey)(object)DateTime.UtcNow.Ticks;
            }
            else if (typeof(TKey) == typeof(string))
            {
                return (TKey)(object)Guid.NewGuid().ToString();
            }
            else
            {
                throw new NotSupportedException($"ID type {typeof(TKey).Name} is not supported");
            }
        }

        /// <summary>
        /// Stores a file for an entity
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <param name="fileName">File name</param>
        /// <param name="contentType">Content type</param>
        /// <param name="content">File content</param>
        /// <param name="isPublic">Whether the file is public</param>
        /// <returns>File URL</returns>
        protected virtual async Task<string> StoreFileAsync(TKey id, string fileName, string contentType, Stream content, bool isPublic = false)
        {
            try
            {
                var entityFileName = $"{_entityName}/{id}/{fileName}";
                return await _storageService.StoreFileAsync(_accountId, null, entityFileName, contentType, content, isPublic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing file {FileName} for {EntityName} with ID: {Id}", fileName, _entityName, id);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a file for an entity
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <param name="fileName">File name</param>
        /// <returns>File content</returns>
        protected virtual async Task<Stream> RetrieveFileAsync(TKey id, string fileName)
        {
            try
            {
                var entityFileName = $"{_entityName}/{id}/{fileName}";
                return await _storageService.RetrieveFileAsync(_accountId, null, entityFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving file {FileName} for {EntityName} with ID: {Id}", fileName, _entityName, id);
                throw;
            }
        }

        /// <summary>
        /// Deletes a file for an entity
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <param name="fileName">File name</param>
        /// <returns>True if the file was deleted successfully, false otherwise</returns>
        protected virtual async Task<bool> DeleteFileAsync(TKey id, string fileName)
        {
            try
            {
                var entityFileName = $"{_entityName}/{id}/{fileName}";
                return await _storageService.DeleteFileAsync(_accountId, null, entityFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FileName} for {EntityName} with ID: {Id}", fileName, _entityName, id);
                throw;
            }
        }

        /// <summary>
        /// Gets the URL for a file
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <param name="fileName">File name</param>
        /// <returns>File URL</returns>
        protected virtual async Task<string> GetFileUrlAsync(TKey id, string fileName)
        {
            try
            {
                var entityFileName = $"{_entityName}/{id}/{fileName}";
                return await _storageService.GetFileUrlAsync(_accountId, null, entityFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting URL for file {FileName} for {EntityName} with ID: {Id}", fileName, _entityName, id);
                throw;
            }
        }
    }
}
