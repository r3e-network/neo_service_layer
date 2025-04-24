using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace NeoServiceLayer.Services.Caching
{
    /// <summary>
    /// Adapter for using IMemoryCache as IDistributedCache
    /// </summary>
    public class MemoryCacheAdapter : IDistributedCache
    {
        private readonly IMemoryCache _memoryCache;
        private readonly MemoryDistributedCacheOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryCacheAdapter"/> class
        /// </summary>
        /// <param name="memoryCache">Memory cache</param>
        /// <param name="optionsAccessor">Options accessor</param>
        public MemoryCacheAdapter(IMemoryCache memoryCache, IOptions<MemoryDistributedCacheOptions> optionsAccessor)
        {
            _memoryCache = memoryCache;
            _options = optionsAccessor.Value;
        }

        /// <inheritdoc/>
        public byte[] Get(string key)
        {
            return _memoryCache.Get<byte[]>(key);
        }

        /// <inheritdoc/>
        public Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            return Task.FromResult(Get(key));
        }

        /// <inheritdoc/>
        public void Refresh(string key)
        {
            // Memory cache doesn't support refresh, so we just get the item to reset sliding expiration
            _memoryCache.TryGetValue(key, out _);
        }

        /// <inheritdoc/>
        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            Refresh(key);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void Remove(string key)
        {
            _memoryCache.Remove(key);
        }

        /// <inheritdoc/>
        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            Remove(key);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            var memoryCacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = options.AbsoluteExpiration,
                AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow,
                SlidingExpiration = options.SlidingExpiration
            };

            _memoryCache.Set(key, value, memoryCacheEntryOptions);
        }

        /// <inheritdoc/>
        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            Set(key, value, options);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Options for memory distributed cache
    /// </summary>
    public class MemoryDistributedCacheOptions
    {
        /// <summary>
        /// Gets or sets the expiration scan frequency in seconds
        /// </summary>
        public int ExpirationScanFrequencyInSeconds { get; set; } = 60;
    }
}
