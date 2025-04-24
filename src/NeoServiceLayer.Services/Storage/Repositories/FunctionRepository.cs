using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;

namespace NeoServiceLayer.Services.Storage.Repositories
{
    /// <summary>
    /// Repository for function-specific entities
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="TKey">Key type</typeparam>
    public abstract class FunctionRepository<T, TKey> : BaseRepository<T, TKey> where T : class
    {
        private readonly ILogger _logger;
        private readonly IStorageService _storageService;
        private readonly Guid _accountId;
        private readonly Guid _functionId;
        private readonly string _entityName;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionRepository{T, TKey}"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="storageService">Storage service</param>
        /// <param name="accountId">Account ID</param>
        /// <param name="functionId">Function ID</param>
        /// <param name="entityName">Entity name</param>
        protected FunctionRepository(
            ILogger logger,
            IStorageService storageService,
            Guid accountId,
            Guid functionId,
            string entityName)
            : base(logger, storageService, accountId, entityName)
        {
            _logger = logger;
            _storageService = storageService;
            _accountId = accountId;
            _functionId = functionId;
            _entityName = entityName;
        }

        /// <inheritdoc/>
        public override async Task<T> GetByIdAsync(TKey id)
        {
            try
            {
                var key = GetKey(id);
                var json = await _storageService.RetrieveKeyValueAsync(_accountId, _functionId, key);
                if (string.IsNullOrEmpty(json))
                {
                    return null;
                }

                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting {EntityName} by ID: {Id} for function: {FunctionId}", _entityName, id, _functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                var result = new List<T>();
                var keys = await _storageService.ListKeysAsync(_accountId, _functionId);

                foreach (var key in keys)
                {
                    if (key.StartsWith($"{_entityName}/"))
                    {
                        var json = await _storageService.RetrieveKeyValueAsync(_accountId, _functionId, key);
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
                _logger.LogError(ex, "Error getting all {EntityName}s for function: {FunctionId}", _entityName, _functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public override async Task<T> CreateAsync(T entity)
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
                await _storageService.StoreKeyValueAsync(_accountId, _functionId, key, json);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating {EntityName} for function: {FunctionId}", _entityName, _functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public override async Task<T> UpdateAsync(T entity)
        {
            try
            {
                var id = GetId(entity);
                var key = GetKey(id);

                // Check if entity exists
                var exists = await ExistsAsync(id);
                if (!exists)
                {
                    throw new KeyNotFoundException($"{_entityName} with ID {id} not found for function: {_functionId}");
                }

                var json = JsonSerializer.Serialize(entity);
                await _storageService.StoreKeyValueAsync(_accountId, _functionId, key, json);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating {EntityName} with ID: {Id} for function: {FunctionId}", _entityName, GetId(entity), _functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public override async Task<bool> DeleteAsync(TKey id)
        {
            try
            {
                var key = GetKey(id);
                return await _storageService.DeleteKeyValueAsync(_accountId, _functionId, key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting {EntityName} with ID: {Id} for function: {FunctionId}", _entityName, id, _functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public override async Task<bool> ExistsAsync(TKey id)
        {
            try
            {
                var key = GetKey(id);
                var json = await _storageService.RetrieveKeyValueAsync(_accountId, _functionId, key);
                return !string.IsNullOrEmpty(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if {EntityName} with ID: {Id} exists for function: {FunctionId}", _entityName, id, _functionId);
                throw;
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
        protected async Task<string> StoreFunctionFileAsync(TKey id, string fileName, string contentType, Stream content, bool isPublic = false)
        {
            try
            {
                var entityFileName = $"{_entityName}/{id}/{fileName}";
                return await _storageService.StoreFileAsync(_accountId, _functionId, entityFileName, contentType, content, isPublic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing file {FileName} for {EntityName} with ID: {Id} for function: {FunctionId}", fileName, _entityName, id, _functionId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a file for an entity
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <param name="fileName">File name</param>
        /// <returns>File content</returns>
        protected async Task<Stream> RetrieveFunctionFileAsync(TKey id, string fileName)
        {
            try
            {
                var entityFileName = $"{_entityName}/{id}/{fileName}";
                return await _storageService.RetrieveFileAsync(_accountId, _functionId, entityFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving file {FileName} for {EntityName} with ID: {Id} for function: {FunctionId}", fileName, _entityName, id, _functionId);
                throw;
            }
        }

        /// <summary>
        /// Deletes a file for an entity
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <param name="fileName">File name</param>
        /// <returns>True if the file was deleted successfully, false otherwise</returns>
        protected async Task<bool> DeleteFunctionFileAsync(TKey id, string fileName)
        {
            try
            {
                var entityFileName = $"{_entityName}/{id}/{fileName}";
                return await _storageService.DeleteFileAsync(_accountId, _functionId, entityFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FileName} for {EntityName} with ID: {Id} for function: {FunctionId}", fileName, _entityName, id, _functionId);
                throw;
            }
        }

        /// <summary>
        /// Gets the URL for a file
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <param name="fileName">File name</param>
        /// <returns>File URL</returns>
        protected async Task<string> GetFunctionFileUrlAsync(TKey id, string fileName)
        {
            try
            {
                var entityFileName = $"{_entityName}/{id}/{fileName}";
                return await _storageService.GetFileUrlAsync(_accountId, _functionId, entityFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting URL for file {FileName} for {EntityName} with ID: {Id} for function: {FunctionId}", fileName, _entityName, id, _functionId);
                throw;
            }
        }
    }
}
