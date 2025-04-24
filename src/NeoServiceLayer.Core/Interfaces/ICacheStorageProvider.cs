using System.Threading.Tasks;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for cache storage providers
    /// </summary>
    public interface ICacheStorageProvider : IStorageProvider
    {
        /// <summary>
        /// Gets a value from the cache
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="collection">Collection name</param>
        /// <param name="key">Cache key</param>
        /// <returns>The value if found, default(T) otherwise</returns>
        Task<T> GetAsync<T>(string collection, string key) where T : class;

        /// <summary>
        /// Stores a value in the cache
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="collection">Collection name</param>
        /// <param name="key">Cache key</param>
        /// <param name="value">Value to store</param>
        /// <returns>True if the value was stored, false otherwise</returns>
        Task<bool> StoreAsync<T>(string collection, string key, T value) where T : class;

        /// <summary>
        /// Deletes a value from the cache
        /// </summary>
        /// <param name="collection">Collection name</param>
        /// <param name="key">Cache key</param>
        /// <returns>True if the value was deleted, false otherwise</returns>
        Task<bool> DeleteAsync(string collection, string key);
    }
}
