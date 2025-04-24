using System;
using System.Threading.Tasks;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for cache service
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Gets an item from the cache
        /// </summary>
        /// <typeparam name="T">Type of the item</typeparam>
        /// <param name="key">Cache key</param>
        /// <returns>The cached item, or default if not found</returns>
        Task<T> GetAsync<T>(string key);

        /// <summary>
        /// Gets an item from the cache, or creates it if it doesn't exist
        /// </summary>
        /// <typeparam name="T">Type of the item</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="factory">Function to create the item if not found in cache</param>
        /// <param name="expiration">Optional expiration time</param>
        /// <returns>The cached or created item</returns>
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);

        /// <summary>
        /// Sets an item in the cache
        /// </summary>
        /// <typeparam name="T">Type of the item</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="value">Item to cache</param>
        /// <param name="expiration">Optional expiration time</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

        /// <summary>
        /// Removes an item from the cache
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task RemoveAsync(string key);

        /// <summary>
        /// Refreshes an item in the cache, extending its expiration
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task RefreshAsync(string key);
    }
}
