using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Interfaces;

namespace NeoServiceLayer.API.IntegrationTests
{
    /// <summary>
    /// In-memory storage provider for integration tests
    /// </summary>
    public class InMemoryStorageProvider : IStorageProvider
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _storage = new();

        /// <summary>
        /// Gets the name of the storage provider
        /// </summary>
        public string Name => "InMemoryStorage";

        /// <summary>
        /// Gets the type of the storage provider
        /// </summary>
        public string Type => "InMemory";

        /// <inheritdoc/>
        public Task<bool> DeleteAsync(string collection, string key)
        {
            if (_storage.TryGetValue(collection, out var collectionDict))
            {
                return Task.FromResult(collectionDict.TryRemove(key, out _));
            }

            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<string> GetAsync(string collection, string key)
        {
            if (_storage.TryGetValue(collection, out var collectionDict) &&
                collectionDict.TryGetValue(key, out var value))
            {
                return Task.FromResult(value);
            }

            return Task.FromResult<string>(null);
        }

        /// <inheritdoc/>
        public Task<List<string>> GetAllAsync(string collection)
        {
            if (_storage.TryGetValue(collection, out var collectionDict))
            {
                return Task.FromResult(new List<string>(collectionDict.Values));
            }

            return Task.FromResult(new List<string>());
        }

        /// <inheritdoc/>
        public Task<List<string>> GetByPrefixAsync(string collection, string prefix)
        {
            var result = new List<string>();

            if (_storage.TryGetValue(collection, out var collectionDict))
            {
                foreach (var kvp in collectionDict)
                {
                    if (kvp.Key.StartsWith(prefix))
                    {
                        result.Add(kvp.Value);
                    }
                }
            }

            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task<bool> SaveAsync(string collection, string key, string value)
        {
            var collectionDict = _storage.GetOrAdd(collection, _ => new ConcurrentDictionary<string, string>());
            collectionDict[key] = value;
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> ExistsAsync(string collection, string key)
        {
            if (_storage.TryGetValue(collection, out var collectionDict))
            {
                return Task.FromResult(collectionDict.ContainsKey(key));
            }

            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<List<string>> GetKeysAsync(string collection)
        {
            if (_storage.TryGetValue(collection, out var collectionDict))
            {
                return Task.FromResult(new List<string>(collectionDict.Keys));
            }

            return Task.FromResult(new List<string>());
        }

        /// <inheritdoc/>
        public Task<List<string>> GetKeysByPrefixAsync(string collection, string prefix)
        {
            var result = new List<string>();

            if (_storage.TryGetValue(collection, out var collectionDict))
            {
                foreach (var key in collectionDict.Keys)
                {
                    if (key.StartsWith(prefix))
                    {
                        result.Add(key);
                    }
                }
            }

            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task<bool> SaveBatchAsync(string collection, Dictionary<string, string> keyValues)
        {
            var collectionDict = _storage.GetOrAdd(collection, _ => new ConcurrentDictionary<string, string>());

            foreach (var kvp in keyValues)
            {
                collectionDict[kvp.Key] = kvp.Value;
            }

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> DeleteBatchAsync(string collection, List<string> keys)
        {
            if (_storage.TryGetValue(collection, out var collectionDict))
            {
                foreach (var key in keys)
                {
                    collectionDict.TryRemove(key, out _);
                }
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<Dictionary<string, string>> GetBatchAsync(string collection, List<string> keys)
        {
            var result = new Dictionary<string, string>();

            if (_storage.TryGetValue(collection, out var collectionDict))
            {
                foreach (var key in keys)
                {
                    if (collectionDict.TryGetValue(key, out var value))
                    {
                        result[key] = value;
                    }
                }
            }

            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task<bool> ClearCollectionAsync(string collection)
        {
            return Task.FromResult(_storage.TryRemove(collection, out _));
        }

        /// <inheritdoc/>
        public Task<bool> InitializeAsync()
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> HealthCheckAsync()
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<T> CreateAsync<T>(string collection, T entity) where T : class
        {
            var id = GetEntityId(entity);
            var json = JsonSerializer.Serialize(entity);
            var collectionDict = _storage.GetOrAdd(collection, _ => new ConcurrentDictionary<string, string>());
            collectionDict[id.ToString()] = json;
            return Task.FromResult(entity);
        }

        /// <inheritdoc/>
        public Task<T> GetByIdAsync<T, TKey>(string collection, TKey id) where T : class
        {
            if (_storage.TryGetValue(collection, out var collectionDict) &&
                collectionDict.TryGetValue(id.ToString(), out var json))
            {
                return Task.FromResult(JsonSerializer.Deserialize<T>(json));
            }

            return Task.FromResult<T>(null);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<T>> GetByFilterAsync<T>(string collection, Func<T, bool> filter) where T : class
        {
            if (_storage.TryGetValue(collection, out var collectionDict))
            {
                var entities = collectionDict.Values
                    .Select(json => JsonSerializer.Deserialize<T>(json))
                    .Where(filter);
                return Task.FromResult(entities);
            }

            return Task.FromResult(Enumerable.Empty<T>());
        }

        /// <inheritdoc/>
        public Task<IEnumerable<T>> GetAllAsync<T>(string collection) where T : class
        {
            if (_storage.TryGetValue(collection, out var collectionDict))
            {
                var entities = collectionDict.Values
                    .Select(json => JsonSerializer.Deserialize<T>(json));
                return Task.FromResult(entities);
            }

            return Task.FromResult(Enumerable.Empty<T>());
        }

        /// <inheritdoc/>
        public Task<T> UpdateAsync<T, TKey>(string collection, TKey id, T entity) where T : class
        {
            if (_storage.TryGetValue(collection, out var collectionDict))
            {
                var json = JsonSerializer.Serialize(entity);
                collectionDict[id.ToString()] = json;
                return Task.FromResult(entity);
            }

            return Task.FromResult<T>(null);
        }

        /// <inheritdoc/>
        public Task<bool> DeleteAsync<T, TKey>(string collection, TKey id) where T : class
        {
            if (_storage.TryGetValue(collection, out var collectionDict))
            {
                return Task.FromResult(collectionDict.TryRemove(id.ToString(), out _));
            }

            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<int> CountAsync<T>(string collection, Func<T, bool> filter = null) where T : class
        {
            if (_storage.TryGetValue(collection, out var collectionDict))
            {
                if (filter == null)
                {
                    return Task.FromResult(collectionDict.Count);
                }

                var count = collectionDict.Values
                    .Select(json => JsonSerializer.Deserialize<T>(json))
                    .Count(filter);
                return Task.FromResult(count);
            }

            return Task.FromResult(0);
        }

        /// <inheritdoc/>
        public Task<bool> CollectionExistsAsync(string collection)
        {
            return Task.FromResult(_storage.ContainsKey(collection));
        }

        /// <inheritdoc/>
        public Task<bool> CreateCollectionAsync(string collection)
        {
            _storage.TryAdd(collection, new ConcurrentDictionary<string, string>());
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> DeleteCollectionAsync(string collection)
        {
            return Task.FromResult(_storage.TryRemove(collection, out _));
        }

        private string GetEntityId<T>(T entity)
        {
            // Try to get the Id property
            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty != null)
            {
                var id = idProperty.GetValue(entity);
                if (id != null)
                {
                    return id.ToString();
                }
            }

            // If no Id property, use a GUID
            return Guid.NewGuid().ToString();
        }
    }
}
