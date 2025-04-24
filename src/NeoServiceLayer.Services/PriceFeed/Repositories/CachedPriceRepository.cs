using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Core.Utilities;

namespace NeoServiceLayer.Services.PriceFeed.Repositories
{
    /// <summary>
    /// Cached implementation of the price repository
    /// </summary>
    public class CachedPriceRepository : IPriceRepository
    {
        private readonly ILogger<CachedPriceRepository> _logger;
        private readonly IPriceRepository _repository;
        private readonly ICacheStorageProvider _cacheProvider;
        private readonly TimeSpan _cacheExpiration;
        private const string CachePrefix = "price_cache:";

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedPriceRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="repository">Underlying price repository</param>
        /// <param name="cacheProvider">Cache storage provider</param>
        /// <param name="cacheExpirationMinutes">Cache expiration in minutes</param>
        public CachedPriceRepository(
            ILogger<CachedPriceRepository> logger,
            IPriceRepository repository,
            ICacheStorageProvider cacheProvider,
            int cacheExpirationMinutes = 5)
        {
            _logger = logger;
            _repository = repository;
            _cacheProvider = cacheProvider;
            _cacheExpiration = TimeSpan.FromMinutes(cacheExpirationMinutes);
        }

        /// <inheritdoc/>
        public async Task<Price> CreateAsync(Price price)
        {
            _logger.LogInformation("Creating price: {Id}, Symbol: {Symbol}, BaseCurrency: {BaseCurrency}, Value: {Value}",
                price.Id, price.Symbol, price.BaseCurrency, price.Value);

            // Create price in the repository
            var result = await _repository.CreateAsync(price);

            // Invalidate cache for the symbol and base currency
            await InvalidateCacheAsync(price.Symbol, price.BaseCurrency);

            return result;
        }

        /// <inheritdoc/>
        public async Task<Price> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting price by ID: {Id}", id);

            // Try to get from cache
            var cacheKey = GetCacheKey($"id:{id}");
            var cachedPrice = await GetFromCacheAsync<Price>(cacheKey);
            if (cachedPrice != null)
            {
                _logger.LogInformation("Cache hit for price ID: {Id}", id);
                return cachedPrice;
            }

            // Get from repository
            var price = await _repository.GetByIdAsync(id);
            if (price != null)
            {
                // Store in cache
                await StoreInCacheAsync(cacheKey, price);
            }

            return price;
        }

        /// <inheritdoc/>
        public async Task<Price> GetLatestPriceAsync(string symbol, string baseCurrency)
        {
            _logger.LogInformation("Getting latest price for Symbol: {Symbol}, BaseCurrency: {BaseCurrency}", symbol, baseCurrency);

            // Try to get from cache
            var cacheKey = GetCacheKey($"latest:{symbol}:{baseCurrency}");
            var cachedPrice = await GetFromCacheAsync<Price>(cacheKey);
            if (cachedPrice != null)
            {
                _logger.LogInformation("Cache hit for latest price: {Symbol}, {BaseCurrency}", symbol, baseCurrency);
                return cachedPrice;
            }

            // Get from repository
            var price = await _repository.GetLatestPriceAsync(symbol, baseCurrency);
            if (price != null)
            {
                // Store in cache
                await StoreInCacheAsync(cacheKey, price);
            }

            return price;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Price>> GetPricesInRangeAsync(string symbol, string baseCurrency, DateTime startTime, DateTime endTime)
        {
            _logger.LogInformation("Getting prices in range for Symbol: {Symbol}, BaseCurrency: {BaseCurrency}, StartTime: {StartTime}, EndTime: {EndTime}",
                symbol, baseCurrency, startTime, endTime);

            // Try to get from cache
            var cacheKey = GetCacheKey($"range:{symbol}:{baseCurrency}:{startTime:yyyyMMddHHmmss}:{endTime:yyyyMMddHHmmss}");
            var cachedPrices = await GetFromCacheAsync<List<Price>>(cacheKey);
            if (cachedPrices != null)
            {
                _logger.LogInformation("Cache hit for prices in range: {Symbol}, {BaseCurrency}", symbol, baseCurrency);
                return cachedPrices;
            }

            // Get from repository
            var prices = await _repository.GetPricesInRangeAsync(symbol, baseCurrency, startTime, endTime);
            var pricesList = prices.ToList();
            if (pricesList.Any())
            {
                // Store in cache
                await StoreInCacheAsync(cacheKey, pricesList);
            }

            return pricesList;
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, Price>> GetAllLatestPricesAsync(string baseCurrency)
        {
            _logger.LogInformation("Getting all latest prices for BaseCurrency: {BaseCurrency}", baseCurrency);

            // Try to get from cache
            var cacheKey = GetCacheKey($"all_latest:{baseCurrency}");
            var cachedPrices = await GetFromCacheAsync<Dictionary<string, Price>>(cacheKey);
            if (cachedPrices != null)
            {
                _logger.LogInformation("Cache hit for all latest prices: {BaseCurrency}", baseCurrency);
                return cachedPrices;
            }

            // Get from repository
            var prices = await _repository.GetAllLatestPricesAsync(baseCurrency);
            if (prices.Any())
            {
                // Store in cache
                await StoreInCacheAsync(cacheKey, prices);
            }

            return prices;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetSupportedSymbolsAsync()
        {
            _logger.LogInformation("Getting supported symbols");

            // Try to get from cache
            var cacheKey = GetCacheKey("supported_symbols");
            var cachedSymbols = await GetFromCacheAsync<List<string>>(cacheKey);
            if (cachedSymbols != null)
            {
                _logger.LogInformation("Cache hit for supported symbols");
                return cachedSymbols;
            }

            // Get from repository
            var symbols = await _repository.GetSupportedSymbolsAsync();
            var symbolsList = symbols.ToList();
            if (symbolsList.Any())
            {
                // Store in cache
                await StoreInCacheAsync(cacheKey, symbolsList);
            }

            return symbolsList;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetSupportedBaseCurrenciesAsync()
        {
            _logger.LogInformation("Getting supported base currencies");

            // Try to get from cache
            var cacheKey = GetCacheKey("supported_base_currencies");
            var cachedBaseCurrencies = await GetFromCacheAsync<List<string>>(cacheKey);
            if (cachedBaseCurrencies != null)
            {
                _logger.LogInformation("Cache hit for supported base currencies");
                return cachedBaseCurrencies;
            }

            // Get from repository
            var baseCurrencies = await _repository.GetSupportedBaseCurrenciesAsync();
            var baseCurrenciesList = baseCurrencies.ToList();
            if (baseCurrenciesList.Any())
            {
                // Store in cache
                await StoreInCacheAsync(cacheKey, baseCurrenciesList);
            }

            return baseCurrenciesList;
        }

        /// <summary>
        /// Gets a value from the cache
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="key">Cache key</param>
        /// <returns>Value if found, null otherwise</returns>
        private async Task<T> GetFromCacheAsync<T>(string key) where T : class
        {
            try
            {
                var json = await _cacheProvider.GetAsync<string>("prices", key);
                if (string.IsNullOrEmpty(json))
                {
                    return null;
                }

                return JsonUtility.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting value from cache: {Key}", key);
                return null;
            }
        }

        /// <summary>
        /// Stores a value in the cache
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="value">Value to store</param>
        /// <returns>Task representing the asynchronous operation</returns>
        private async Task StoreInCacheAsync<T>(string key, T value) where T : class
        {
            try
            {
                var json = JsonUtility.Serialize(value);
                await _cacheProvider.StoreAsync("prices", key, json);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error storing value in cache: {Key}", key);
            }
        }

        /// <summary>
        /// Invalidates the cache for a symbol and base currency
        /// </summary>
        /// <param name="symbol">Asset symbol</param>
        /// <param name="baseCurrency">Base currency</param>
        /// <returns>Task representing the asynchronous operation</returns>
        private async Task InvalidateCacheAsync(string symbol, string baseCurrency)
        {
            try
            {
                // Invalidate latest price cache
                var latestCacheKey = GetCacheKey($"latest:{symbol}:{baseCurrency}");
                await _cacheProvider.DeleteAsync("prices", latestCacheKey);

                // Invalidate all latest prices cache
                var allLatestCacheKey = GetCacheKey($"all_latest:{baseCurrency}");
                await _cacheProvider.DeleteAsync("prices", allLatestCacheKey);

                // Invalidate supported symbols and base currencies cache
                var supportedSymbolsCacheKey = GetCacheKey("supported_symbols");
                await _cacheProvider.DeleteAsync("prices", supportedSymbolsCacheKey);

                var supportedBaseCurrenciesCacheKey = GetCacheKey("supported_base_currencies");
                await _cacheProvider.DeleteAsync("prices", supportedBaseCurrenciesCacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error invalidating cache for Symbol: {Symbol}, BaseCurrency: {BaseCurrency}", symbol, baseCurrency);
            }
        }

        /// <summary>
        /// Gets a cache key with the prefix
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Cache key</returns>
        private string GetCacheKey(string key)
        {
            return $"{CachePrefix}{key}";
        }
    }
}
