using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core;
using NeoServiceLayer.Core.Exceptions;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Core.Extensions;
using NeoServiceLayer.Services.PriceFeed.Repositories;
using NeoServiceLayer.Services.Common.Utilities;
using NeoServiceLayer.Services.Common.Extensions;

namespace NeoServiceLayer.Services.PriceFeed
{
    /// <summary>
    /// Implementation of the price feed service
    /// </summary>
    public class PriceFeedService : IPriceFeedService
    {
        private readonly ILogger<PriceFeedService> _logger;
        private readonly IPriceRepository _priceRepository;
        private readonly IPriceSourceRepository _sourceRepository;
        private readonly IPriceHistoryRepository _historyRepository;
        private readonly IEnclaveService _enclaveService;
        private readonly IWalletService _walletService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceFeedService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="priceRepository">Price repository</param>
        /// <param name="sourceRepository">Price source repository</param>
        /// <param name="historyRepository">Price history repository</param>
        /// <param name="enclaveService">Enclave service</param>
        /// <param name="walletService">Wallet service</param>
        public PriceFeedService(
            ILogger<PriceFeedService> logger,
            IPriceRepository priceRepository,
            IPriceSourceRepository sourceRepository,
            IPriceHistoryRepository historyRepository,
            IEnclaveService enclaveService,
            IWalletService walletService)
        {
            _logger = logger;
            _priceRepository = priceRepository;
            _sourceRepository = sourceRepository;
            _historyRepository = historyRepository;
            _enclaveService = enclaveService;
            _walletService = walletService;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Price>> FetchPricesAsync(string baseCurrency = "USD")
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["BaseCurrency"] = baseCurrency
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "FetchPrices", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(baseCurrency, "Base currency");

                // Get active sources
                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<PriceFeedService, IEnumerable<Price>>(
                    _logger,
                    async () =>
                    {
                        // Get active sources
                        var sources = await _sourceRepository.GetActiveSourcesAsync();
                        if (!sources.Any())
                        {
                            Common.Utilities.LoggingUtility.LogWarning(_logger, "No active price sources found", requestId, additionalData);
                            return new List<Price>();
                        }

                        additionalData["SourceCount"] = sources.Count();

                        // Send fetch request to enclave
                        var fetchRequest = new
                        {
                            BaseCurrency = baseCurrency,
                            Sources = sources.Select(s => new
                            {
                                Id = s.Id,
                                Name = s.Name,
                                Type = s.Type.ToString(),
                                Url = s.Url,
                                ApiKey = s.ApiKey,
                                ApiSecret = s.ApiSecret,
                                SupportedAssets = s.SupportedAssets,
                                Config = s.Config
                            }).ToList()
                        };

                        var prices = await _enclaveService.SendRequestAsync<object, List<Price>>(
                            Constants.EnclaveServiceTypes.PriceFeed,
                            Constants.PriceFeedOperations.FetchPrices,
                            fetchRequest);

                        additionalData["PriceCount"] = prices.Count;

                        // Save prices to repository
                        foreach (var price in prices)
                        {
                            await _priceRepository.CreateAsync(price);
                        }

                        return prices;
                    },
                    "FetchPrices",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new PriceFeedException("Failed to fetch prices");
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "FetchPrices", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "FetchPrices", requestId, ex, 0, additionalData);
                throw new PriceFeedException("Error fetching prices", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Price>> FetchPriceForSymbolAsync(string symbol, string baseCurrency = "USD")
        {
            _logger.LogInformation("Fetching price for symbol: {Symbol}, base currency: {BaseCurrency}", symbol, baseCurrency);

            try
            {
                // Get active sources that support the symbol
                var sources = await _sourceRepository.GetByAssetAsync(symbol);
                sources = sources.Where(s => s.Status == PriceSourceStatus.Active).ToList();

                if (!sources.Any())
                {
                    _logger.LogWarning("No active price sources found for symbol: {Symbol}", symbol);
                    return new List<Price>();
                }

                // Send fetch request to enclave
                var fetchRequest = new
                {
                    Symbol = symbol,
                    BaseCurrency = baseCurrency,
                    Sources = sources.Select(s => new
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Type = s.Type.ToString(),
                        Url = s.Url,
                        ApiKey = s.ApiKey,
                        ApiSecret = s.ApiSecret,
                        Config = s.Config
                    }).ToList()
                };

                var prices = await _enclaveService.SendRequestAsync<object, List<Price>>(
                    Constants.EnclaveServiceTypes.PriceFeed,
                    Constants.PriceFeedOperations.FetchPriceForSymbol,
                    fetchRequest);

                // Save prices to repository
                foreach (var price in prices)
                {
                    await _priceRepository.CreateAsync(price);
                }

                _logger.LogInformation("Fetched {Count} prices for symbol: {Symbol}, base currency: {BaseCurrency}", prices.Count, symbol, baseCurrency);
                return prices;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching price for symbol: {Symbol}, base currency: {BaseCurrency}", symbol, baseCurrency);
                throw new PriceFeedException($"Error fetching price for {symbol}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Price>> FetchPriceFromSourceAsync(Guid sourceId, string baseCurrency = "USD")
        {
            _logger.LogInformation("Fetching price from source: {SourceId}, base currency: {BaseCurrency}", sourceId, baseCurrency);

            try
            {
                // Get the source
                var source = await _sourceRepository.GetByIdAsync(sourceId);
                if (source == null)
                {
                    throw new PriceFeedException($"Price source not found: {sourceId}");
                }

                if (source.Status != PriceSourceStatus.Active)
                {
                    throw new PriceFeedException($"Price source is not active: {source.Name}");
                }

                // Send fetch request to enclave
                var fetchRequest = new
                {
                    BaseCurrency = baseCurrency,
                    Source = new
                    {
                        Id = source.Id,
                        Name = source.Name,
                        Type = source.Type.ToString(),
                        Url = source.Url,
                        ApiKey = source.ApiKey,
                        ApiSecret = source.ApiSecret,
                        SupportedAssets = source.SupportedAssets,
                        Config = source.Config
                    }
                };

                var prices = await _enclaveService.SendRequestAsync<object, List<Price>>(
                    Constants.EnclaveServiceTypes.PriceFeed,
                    Constants.PriceFeedOperations.FetchPriceFromSource,
                    fetchRequest);

                // Save prices to repository
                foreach (var price in prices)
                {
                    await _priceRepository.CreateAsync(price);
                }

                // Update source last successful fetch time
                source.LastSuccessfulFetchAt = DateTime.UtcNow;
                await _sourceRepository.UpdateAsync(source);

                _logger.LogInformation("Fetched {Count} prices from source: {SourceName}, base currency: {BaseCurrency}", prices.Count, source.Name, baseCurrency);
                return prices;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching price from source: {SourceId}, base currency: {BaseCurrency}", sourceId, baseCurrency);
                throw new PriceFeedException($"Error fetching price from source {sourceId}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Price> GetLatestPriceAsync(string symbol, string baseCurrency = "USD")
        {
            _logger.LogInformation("Getting latest price for symbol: {Symbol}, base currency: {BaseCurrency}", symbol, baseCurrency);

            try
            {
                var price = await _priceRepository.GetLatestPriceAsync(symbol, baseCurrency);
                if (price == null)
                {
                    _logger.LogWarning("No price found for symbol: {Symbol}, base currency: {BaseCurrency}", symbol, baseCurrency);

                    // Try to fetch the price
                    var prices = await FetchPriceForSymbolAsync(symbol, baseCurrency);
                    price = prices.OrderByDescending(p => p.Timestamp).FirstOrDefault();
                }

                return price;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest price for symbol: {Symbol}, base currency: {BaseCurrency}", symbol, baseCurrency);
                throw new PriceFeedException($"Error getting latest price for {symbol}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, Price>> GetAllLatestPricesAsync(string baseCurrency = "USD")
        {
            _logger.LogInformation("Getting all latest prices for base currency: {BaseCurrency}", baseCurrency);

            try
            {
                return await _priceRepository.GetAllLatestPricesAsync(baseCurrency);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all latest prices for base currency: {BaseCurrency}", baseCurrency);
                throw new PriceFeedException("Error getting all latest prices", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Price>> GetHistoricalPricesAsync(string symbol, string baseCurrency, DateTime startTime, DateTime endTime)
        {
            _logger.LogInformation("Getting historical prices for symbol: {Symbol}, base currency: {BaseCurrency}, start time: {StartTime}, end time: {EndTime}",
                symbol, baseCurrency, startTime, endTime);

            try
            {
                return await _priceRepository.GetPricesInRangeAsync(symbol, baseCurrency, startTime, endTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting historical prices for symbol: {Symbol}, base currency: {BaseCurrency}", symbol, baseCurrency);
                throw new PriceFeedException($"Error getting historical prices for {symbol}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<PriceHistory> GetPriceHistoryAsync(string symbol, string baseCurrency, string interval, DateTime startTime, DateTime endTime)
        {
            _logger.LogInformation("Getting price history for symbol: {Symbol}, base currency: {BaseCurrency}, interval: {Interval}, start time: {StartTime}, end time: {EndTime}",
                symbol, baseCurrency, interval, startTime, endTime);

            try
            {
                var history = await _historyRepository.GetHistoryAsync(symbol, baseCurrency, interval, startTime, endTime);
                if (history == null || !history.DataPoints.Any())
                {
                    _logger.LogWarning("No price history found for symbol: {Symbol}, base currency: {BaseCurrency}, interval: {Interval}",
                        symbol, baseCurrency, interval);

                    // Try to generate price history from available prices
                    var prices = await _priceRepository.GetPricesInRangeAsync(symbol, baseCurrency, startTime, endTime);
                    if (prices.Any())
                    {
                        // Send request to enclave to generate price history
                        var generateRequest = new
                        {
                            Symbol = symbol,
                            BaseCurrency = baseCurrency,
                            Interval = interval,
                            StartTime = startTime,
                            EndTime = endTime,
                            Prices = prices.ToList()
                        };

                        history = await _enclaveService.SendRequestAsync<object, PriceHistory>(
                            Constants.EnclaveServiceTypes.PriceFeed,
                            Constants.PriceFeedOperations.GeneratePriceHistory,
                            generateRequest);

                        // Save price history to repository
                        if (history != null && history.DataPoints.Any())
                        {
                            await _historyRepository.CreateAsync(history);
                        }
                    }
                }

                return history;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting price history for symbol: {Symbol}, base currency: {BaseCurrency}, interval: {Interval}",
                    symbol, baseCurrency, interval);
                throw new PriceFeedException($"Error getting price history for {symbol}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<string> SubmitToOracleAsync(Price price)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Symbol"] = price.Symbol,
                ["BaseCurrency"] = price.BaseCurrency,
                ["Value"] = price.Value
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "SubmitPriceToOracle", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateNotNull(price, nameof(price));
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(price.Symbol, "Symbol");
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(price.BaseCurrency, "Base currency");
                Common.Utilities.ValidationUtility.ValidateGreaterThanZero(price.Value, "Price value");

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<PriceFeedService, string>(
                    _logger,
                    async () =>
                    {
                        // Send submit request to enclave
                        var submitRequest = new
                        {
                            Price = price
                        };

                        var oracleResult = await _enclaveService.SendRequestAsync<object, object>(
                            Constants.EnclaveServiceTypes.PriceFeed,
                            Constants.PriceFeedOperations.SubmitToOracle,
                            submitRequest);

                        // Extract transaction hash from result
                        var transactionHash = oracleResult.GetType().GetProperty("TransactionHash")?.GetValue(oracleResult)?.ToString();

                        if (string.IsNullOrEmpty(transactionHash))
                        {
                            throw new PriceFeedException("Failed to get transaction hash from oracle result");
                        }

                        additionalData["TransactionHash"] = transactionHash;

                        return transactionHash;
                    },
                    "SubmitPriceToOracle",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new PriceFeedException("Failed to submit price to oracle");
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "SubmitPriceToOracle", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "SubmitPriceToOracle", requestId, ex, 0, additionalData);
                throw new PriceFeedException("Error submitting price to oracle", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> SubmitBatchToOracleAsync(IEnumerable<Price> prices)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["PriceCount"] = prices.Count()
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "SubmitBatchToOracle", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateNotNull(prices, nameof(prices));
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(prices, "Prices");

                // Validate each price
                foreach (var price in prices)
                {
                    Common.Utilities.ValidationUtility.ValidateNotNull(price, "Price");
                    Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(price.Symbol, "Symbol");
                    Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(price.BaseCurrency, "Base currency");
                    Common.Utilities.ValidationUtility.ValidateGreaterThanZero(price.Value, "Price value");
                }

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<PriceFeedService, IEnumerable<string>>(
                    _logger,
                    async () =>
                    {
                        // Send submit batch request to enclave
                        var submitRequest = new
                        {
                            Prices = prices.ToList()
                        };

                        var batchResult = await _enclaveService.SendRequestAsync<object, List<string>>(
                            Constants.EnclaveServiceTypes.PriceFeed,
                            Constants.PriceFeedOperations.SubmitBatchToOracle,
                            submitRequest);

                        additionalData["TransactionCount"] = batchResult.Count;

                        return batchResult;
                    },
                    "SubmitBatchToOracle",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new PriceFeedException("Failed to submit batch of prices to oracle");
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "SubmitBatchToOracle", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "SubmitBatchToOracle", requestId, ex, 0, additionalData);
                throw new PriceFeedException("Error submitting batch of prices to oracle", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Price> GetPriceByIdAsync(Guid id)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Id"] = id
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "GetPriceById", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateGuid(id, "Price ID");

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<PriceFeedService, Price>(
                    _logger,
                    async () => await _priceRepository.GetByIdAsync(id),
                    "GetPriceById",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new PriceFeedException($"Failed to get price by ID {id}");
                }

                if (result.result != null)
                {
                    additionalData["Symbol"] = result.result.Symbol;
                    additionalData["BaseCurrency"] = result.result.BaseCurrency;
                    additionalData["Value"] = result.result.Value;
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "GetPriceById", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "GetPriceById", requestId, ex, 0, additionalData);
                throw new PriceFeedException($"Error getting price by ID {id}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<PriceSource> AddSourceAsync(PriceSource source)
        {
            var requestId = Guid.NewGuid().ToString();
            var additionalData = new Dictionary<string, object>
            {
                ["Name"] = source.Name,
                ["Type"] = source.Type.ToString()
            };

            Common.Utilities.LoggingUtility.LogOperationStart(_logger, "AddPriceSource", requestId, additionalData);

            try
            {
                // Validate input
                Common.Utilities.ValidationUtility.ValidateNotNull(source, nameof(source));
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(source.Name, "Source name");
                Common.Utilities.ValidationUtility.ValidateNotNullOrEmpty(source.Url, "Source URL");

                if (!source.Url.IsValidUrl())
                {
                    throw new PriceFeedException("Invalid URL format");
                }

                if (source.SupportedAssets == null || !source.SupportedAssets.Any())
                {
                    throw new PriceFeedException("Price source must support at least one asset");
                }

                var result = await Common.Utilities.ExceptionUtility.ExecuteWithExceptionHandlingAsync<PriceFeedService, PriceSource>(
                    _logger,
                    async () =>
                    {
                        // Check if source with same name already exists
                        var existingSource = await _sourceRepository.GetByNameAsync(source.Name);
                        if (existingSource != null)
                        {
                            throw new PriceFeedException($"Price source with name '{source.Name}' already exists");
                        }

                        // Set default values
                        if (source.Id == Guid.Empty)
                        {
                            source.Id = Guid.NewGuid();
                        }

                        if (source.UpdateIntervalSeconds <= 0)
                        {
                            source.UpdateIntervalSeconds = 60; // Default to 1 minute
                        }

                        if (source.TimeoutSeconds <= 0)
                        {
                            source.TimeoutSeconds = 10; // Default to 10 seconds
                        }

                        if (source.Weight <= 0)
                        {
                            source.Weight = 100; // Default to 100% weight
                        }

                        if (source.Status == 0)
                        {
                            source.Status = PriceSourceStatus.Testing; // Default to testing status
                        }

                        // Initialize config if null
                        if (source.Config == null)
                        {
                            source.Config = new PriceSourceConfig
                            {
                                HttpMethod = "GET",
                                ContentType = "application/json"
                            };
                        }

                        // Send validation request to enclave
                        var validationRequest = new
                        {
                            Source = new
                            {
                                Id = source.Id,
                                Name = source.Name,
                                Type = source.Type.ToString(),
                                Url = source.Url,
                                ApiKey = source.ApiKey,
                                ApiSecret = source.ApiSecret,
                                SupportedAssets = source.SupportedAssets,
                                Config = source.Config
                            }
                        };

                        var validationResult = await _enclaveService.SendRequestAsync<object, object>(
                            Constants.EnclaveServiceTypes.PriceFeed,
                            Constants.PriceFeedOperations.ValidateSource,
                            validationRequest);

                        // Save source to repository
                        return await _sourceRepository.CreateAsync(source);
                    },
                    "AddPriceSource",
                    requestId,
                    additionalData);

                if (!result.success)
                {
                    throw new PriceFeedException("Failed to add price source");
                }

                Common.Utilities.LoggingUtility.LogOperationSuccess(_logger, "AddPriceSource", requestId, 0, additionalData);

                return result.result;
            }
            catch (Exception ex)
            {
                Common.Utilities.LoggingUtility.LogOperationFailure(_logger, "AddPriceSource", requestId, ex, 0, additionalData);
                throw new PriceFeedException("Error adding price source", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<PriceSource> UpdateSourceAsync(PriceSource source)
        {
            _logger.LogInformation("Updating price source: {Id}, {Name}", source.Id, source.Name);

            try
            {
                // Check if source exists
                var existingSource = await _sourceRepository.GetByIdAsync(source.Id);
                if (existingSource == null)
                {
                    throw new PriceFeedException($"Price source not found: {source.Id}");
                }

                // Check if name is being changed and if new name already exists
                if (existingSource.Name != source.Name)
                {
                    var sourceWithSameName = await _sourceRepository.GetByNameAsync(source.Name);
                    if (sourceWithSameName != null && sourceWithSameName.Id != source.Id)
                    {
                        throw new PriceFeedException($"Price source with name '{source.Name}' already exists");
                    }
                }

                // Validate source configuration
                if (string.IsNullOrEmpty(source.Url))
                {
                    throw new PriceFeedException("Price source URL is required");
                }

                if (source.SupportedAssets == null || !source.SupportedAssets.Any())
                {
                    throw new PriceFeedException("Price source must support at least one asset");
                }

                // Preserve creation timestamp
                source.CreatedAt = existingSource.CreatedAt;
                source.UpdatedAt = DateTime.UtcNow;

                // Send validation request to enclave
                var validationRequest = new
                {
                    Source = new
                    {
                        Id = source.Id,
                        Name = source.Name,
                        Type = source.Type.ToString(),
                        Url = source.Url,
                        ApiKey = source.ApiKey,
                        ApiSecret = source.ApiSecret,
                        SupportedAssets = source.SupportedAssets,
                        Config = source.Config
                    }
                };

                var validationResult = await _enclaveService.SendRequestAsync<object, object>(
                    Constants.EnclaveServiceTypes.PriceFeed,
                    Constants.PriceFeedOperations.ValidateSource,
                    validationRequest);

                // Update source in repository
                return await _sourceRepository.UpdateAsync(source);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating price source: {Id}, {Name}", source.Id, source.Name);
                throw new PriceFeedException("Error updating price source", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<PriceSource> GetSourceByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting price source by ID: {Id}", id);

            try
            {
                return await _sourceRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting price source by ID: {Id}", id);
                throw new PriceFeedException($"Error getting price source by ID {id}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<PriceSource> GetSourceByNameAsync(string name)
        {
            _logger.LogInformation("Getting price source by name: {Name}", name);

            try
            {
                return await _sourceRepository.GetByNameAsync(name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting price source by name: {Name}", name);
                throw new PriceFeedException($"Error getting price source by name {name}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PriceSource>> GetAllSourcesAsync()
        {
            _logger.LogInformation("Getting all price sources");

            try
            {
                return await _sourceRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all price sources");
                throw new PriceFeedException("Error getting all price sources", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PriceSource>> GetActiveSourcesAsync()
        {
            _logger.LogInformation("Getting active price sources");

            try
            {
                return await _sourceRepository.GetActiveSourcesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active price sources");
                throw new PriceFeedException("Error getting active price sources", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveSourceAsync(Guid id)
        {
            _logger.LogInformation("Removing price source: {Id}", id);

            try
            {
                // Check if source exists
                var source = await _sourceRepository.GetByIdAsync(id);
                if (source == null)
                {
                    throw new PriceFeedException($"Price source not found: {id}");
                }

                return await _sourceRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing price source: {Id}", id);
                throw new PriceFeedException($"Error removing price source {id}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetSupportedSymbolsAsync()
        {
            _logger.LogInformation("Getting supported symbols");

            try
            {
                return await _priceRepository.GetSupportedSymbolsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supported symbols");
                throw new PriceFeedException("Error getting supported symbols", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetSupportedBaseCurrenciesAsync()
        {
            _logger.LogInformation("Getting supported base currencies");

            try
            {
                return await _priceRepository.GetSupportedBaseCurrenciesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supported base currencies");
                throw new PriceFeedException("Error getting supported base currencies", ex);
            }
        }
    }
}
