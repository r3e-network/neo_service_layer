using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeoServiceLayer.Core.Interfaces;

namespace NeoServiceLayer.Services.Caching
{
    /// <summary>
    /// Service for caching data
    /// </summary>
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<CacheService> _logger;
        private readonly CacheConfiguration _cacheConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheService"/> class
        /// </summary>
        /// <param name="cache">Distributed cache</param>
        /// <param name="logger">Logger</param>
        /// <param name="cacheConfig">Cache configuration</param>
        public CacheService(
            IDistributedCache cache,
            ILogger<CacheService> logger,
            IOptions<CacheConfiguration> cacheConfig)
        {
            _cache = cache;
            _logger = logger;
            _cacheConfig = cacheConfig.Value;
        }

        /// <inheritdoc/>
        public async Task<T> GetAsync<T>(string key)
        {
            try
            {
                _logger.LogDebug("Getting item from cache with key: {Key}", key);

                var cachedValue = await _cache.GetStringAsync(key);
                if (string.IsNullOrEmpty(cachedValue))
                {
                    _logger.LogDebug("Cache miss for key: {Key}", key);
                    return default;
                }

                _logger.LogDebug("Cache hit for key: {Key}", key);
                return JsonSerializer.Deserialize<T>(cachedValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting item from cache with key {Key}: {Message}", key, ex.Message);
                return default;
            }
        }

        /// <inheritdoc/>
        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            try
            {
                // Try to get from cache first
                var cachedValue = await GetAsync<T>(key);
                if (cachedValue != null)
                {
                    return cachedValue;
                }

                // Cache miss, create item
                _logger.LogDebug("Creating item for cache with key: {Key}", key);
                var item = await factory();

                // Cache the item
                if (item != null)
                {
                    await SetAsync(key, item, expiration);
                }

                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetOrCreate for key {Key}: {Message}", key, ex.Message);
                
                // If cache fails, still try to get the item directly
                return await factory();
            }
        }

        /// <inheritdoc/>
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                _logger.LogDebug("Setting item in cache with key: {Key}", key);

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromSeconds(_cacheConfig.DefaultExpirationSeconds)
                };

                var serializedValue = JsonSerializer.Serialize(value);
                await _cache.SetStringAsync(key, serializedValue, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting item in cache with key {Key}: {Message}", key, ex.Message);
            }
        }

        /// <inheritdoc/>
        public async Task RemoveAsync(string key)
        {
            try
            {
                _logger.LogDebug("Removing item from cache with key: {Key}", key);
                await _cache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from cache with key {Key}: {Message}", key, ex.Message);
            }
        }

        /// <inheritdoc/>
        public async Task RefreshAsync(string key)
        {
            try
            {
                _logger.LogDebug("Refreshing item in cache with key: {Key}", key);
                await _cache.RefreshAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing item in cache with key {Key}: {Message}", key, ex.Message);
            }
        }
    }
}
