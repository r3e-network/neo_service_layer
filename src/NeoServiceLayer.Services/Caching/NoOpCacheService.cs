using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;

namespace NeoServiceLayer.Services.Caching
{
    /// <summary>
    /// No-op implementation of cache service for when caching is disabled
    /// </summary>
    public class NoOpCacheService : ICacheService
    {
        private readonly ILogger<NoOpCacheService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoOpCacheService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        public NoOpCacheService(ILogger<NoOpCacheService> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public Task<T> GetAsync<T>(string key)
        {
            _logger.LogDebug("NoOpCacheService: GetAsync called for key {Key}", key);
            return Task.FromResult<T>(default);
        }

        /// <inheritdoc/>
        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            _logger.LogDebug("NoOpCacheService: GetOrCreateAsync called for key {Key}", key);
            return await factory();
        }

        /// <inheritdoc/>
        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            _logger.LogDebug("NoOpCacheService: SetAsync called for key {Key}", key);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task RemoveAsync(string key)
        {
            _logger.LogDebug("NoOpCacheService: RemoveAsync called for key {Key}", key);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task RefreshAsync(string key)
        {
            _logger.LogDebug("NoOpCacheService: RefreshAsync called for key {Key}", key);
            return Task.CompletedTask;
        }
    }
}
