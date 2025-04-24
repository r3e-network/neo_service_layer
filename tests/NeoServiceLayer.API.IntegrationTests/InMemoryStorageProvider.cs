using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    }
}
