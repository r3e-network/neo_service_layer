using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Core.Repositories;
using NeoServiceLayer.Core.Utilities;

namespace NeoServiceLayer.Services.PriceFeed.Repositories
{
    /// <summary>
    /// Implementation of the price repository
    /// </summary>
    public class PriceRepository : IPriceRepository
    {
        private readonly ILogger<PriceRepository> _logger;
        private readonly IGenericRepository<Price, Guid> _repository;
        private readonly IStorageProvider _storageProvider;
        private const string CollectionName = "prices";

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceRepository"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="storageProvider">Storage provider</param>
        public PriceRepository(
            ILogger<PriceRepository> logger,
            IStorageProvider storageProvider)
        {
            _logger = logger;
            _storageProvider = storageProvider;
            _repository = new GenericRepository<Price, Guid>(logger, storageProvider, CollectionName);
        }

        /// <inheritdoc/>
        public async Task<Price> CreateAsync(Price price)
        {
            _logger.LogInformation("Creating price: {Id}, Symbol: {Symbol}, BaseCurrency: {BaseCurrency}, Value: {Value}",
                price.Id, price.Symbol, price.BaseCurrency, price.Value);

            if (price.Id == Guid.Empty)
            {
                price.Id = Guid.NewGuid();
            }

            price.CreatedAt = DateTime.UtcNow;

            return await _repository.CreateAsync(price);
        }

        /// <inheritdoc/>
        public async Task<Price> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting price by ID: {Id}", id);

            try
            {
                return await _repository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting price by ID: {Id}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Price> GetLatestPriceAsync(string symbol, string baseCurrency)
        {
            _logger.LogInformation("Getting latest price for Symbol: {Symbol}, BaseCurrency: {BaseCurrency}", symbol, baseCurrency);

            try
            {
                var prices = await _repository.FindAsync(p =>
                    p.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase) &&
                    p.BaseCurrency.Equals(baseCurrency, StringComparison.OrdinalIgnoreCase));

                var symbolPrices = prices.ToList();

                if (symbolPrices.Count == 0)
                {
                    return null;
                }

                return symbolPrices.OrderByDescending(p => p.Timestamp).First();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest price for Symbol: {Symbol}, BaseCurrency: {BaseCurrency}", symbol, baseCurrency);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Price>> GetPricesInRangeAsync(string symbol, string baseCurrency, DateTime startTime, DateTime endTime)
        {
            _logger.LogInformation("Getting prices in range for Symbol: {Symbol}, BaseCurrency: {BaseCurrency}, StartTime: {StartTime}, EndTime: {EndTime}",
                symbol, baseCurrency, startTime, endTime);

            try
            {
                var prices = await _repository.FindAsync(p =>
                    p.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase) &&
                    p.BaseCurrency.Equals(baseCurrency, StringComparison.OrdinalIgnoreCase) &&
                    p.Timestamp >= startTime && p.Timestamp <= endTime);

                return prices.OrderBy(p => p.Timestamp).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prices in range for Symbol: {Symbol}, BaseCurrency: {BaseCurrency}", symbol, baseCurrency);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, Price>> GetAllLatestPricesAsync(string baseCurrency)
        {
            _logger.LogInformation("Getting all latest prices for BaseCurrency: {BaseCurrency}", baseCurrency);

            try
            {
                var prices = await _repository.FindAsync(p =>
                    p.BaseCurrency.Equals(baseCurrency, StringComparison.OrdinalIgnoreCase));

                var result = new Dictionary<string, Price>();

                // Group by symbol
                var pricesBySymbol = prices.GroupBy(p => p.Symbol);

                foreach (var group in pricesBySymbol)
                {
                    var symbol = group.Key;
                    var latestPrice = group.OrderByDescending(p => p.Timestamp).First();
                    result[symbol] = latestPrice;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all latest prices for BaseCurrency: {BaseCurrency}", baseCurrency);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetSupportedSymbolsAsync()
        {
            _logger.LogInformation("Getting supported symbols");

            try
            {
                var prices = await _repository.GetAllAsync();
                var symbols = prices.Select(p => p.Symbol).Distinct().ToList();
                return symbols;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supported symbols");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetSupportedBaseCurrenciesAsync()
        {
            _logger.LogInformation("Getting supported base currencies");

            try
            {
                var prices = await _repository.GetAllAsync();
                var baseCurrencies = prices.Select(p => p.BaseCurrency).Distinct().ToList();
                return baseCurrencies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supported base currencies");
                throw;
            }
        }
    }
}
