using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Api.Models;
using NeoServiceLayer.Core.Exceptions;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Api.Controllers
{
    /// <summary>
    /// Controller for price feed operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PriceFeedController : ControllerBase
    {
        private readonly ILogger<PriceFeedController> _logger;
        private readonly IPriceFeedService _priceFeedService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceFeedController"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="priceFeedService">Price feed service</param>
        public PriceFeedController(ILogger<PriceFeedController> logger, IPriceFeedService priceFeedService)
        {
            _logger = logger;
            _priceFeedService = priceFeedService;
        }

        /// <summary>
        /// Gets the latest price for a symbol
        /// </summary>
        /// <param name="symbol">Symbol to get price for</param>
        /// <param name="baseCurrency">Base currency for the price</param>
        /// <returns>Latest price for the symbol</returns>
        [HttpGet("latest/{symbol}")]
        public async Task<IActionResult> GetLatestPrice(string symbol, [FromQuery] string baseCurrency = "USD")
        {
            _logger.LogInformation("Getting latest price for symbol: {Symbol}, base currency: {BaseCurrency}", symbol, baseCurrency);

            try
            {
                var price = await _priceFeedService.GetLatestPriceAsync(symbol, baseCurrency);
                if (price == null)
                {
                    return NotFound(new { Message = $"No price found for {symbol}" });
                }

                return Ok(price);
            }
            catch (PriceFeedException ex)
            {
                _logger.LogError(ex, "Error getting latest price for symbol: {Symbol}", symbol);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting latest price for symbol: {Symbol}", symbol);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Gets the latest prices for all symbols
        /// </summary>
        /// <param name="baseCurrency">Base currency for the prices</param>
        /// <returns>Dictionary of symbol to latest price</returns>
        [HttpGet("latest")]
        public async Task<IActionResult> GetAllLatestPrices([FromQuery] string baseCurrency = "USD")
        {
            _logger.LogInformation("Getting all latest prices for base currency: {BaseCurrency}", baseCurrency);

            try
            {
                var prices = await _priceFeedService.GetAllLatestPricesAsync(baseCurrency);
                return Ok(prices);
            }
            catch (PriceFeedException ex)
            {
                _logger.LogError(ex, "Error getting all latest prices");
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting all latest prices");
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Gets historical prices for a symbol
        /// </summary>
        /// <param name="symbol">Symbol to get prices for</param>
        /// <param name="baseCurrency">Base currency for the prices</param>
        /// <param name="startTime">Start time for the historical data</param>
        /// <param name="endTime">End time for the historical data</param>
        /// <returns>List of historical prices for the symbol</returns>
        [HttpGet("history/{symbol}")]
        public async Task<IActionResult> GetHistoricalPrices(
            string symbol, 
            [FromQuery] string baseCurrency = "USD",
            [FromQuery] DateTime? startTime = null,
            [FromQuery] DateTime? endTime = null)
        {
            var start = startTime ?? DateTime.UtcNow.AddDays(-7);
            var end = endTime ?? DateTime.UtcNow;

            _logger.LogInformation("Getting historical prices for symbol: {Symbol}, base currency: {BaseCurrency}, start time: {StartTime}, end time: {EndTime}", 
                symbol, baseCurrency, start, end);

            try
            {
                var prices = await _priceFeedService.GetHistoricalPricesAsync(symbol, baseCurrency, start, end);
                return Ok(prices);
            }
            catch (PriceFeedException ex)
            {
                _logger.LogError(ex, "Error getting historical prices for symbol: {Symbol}", symbol);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting historical prices for symbol: {Symbol}", symbol);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Gets price history for a symbol with OHLCV data
        /// </summary>
        /// <param name="symbol">Symbol to get price history for</param>
        /// <param name="interval">Interval for the price data (e.g., "1m", "1h", "1d")</param>
        /// <param name="baseCurrency">Base currency for the prices</param>
        /// <param name="startTime">Start time for the historical data</param>
        /// <param name="endTime">End time for the historical data</param>
        /// <returns>Price history with OHLCV data</returns>
        [HttpGet("ohlcv/{symbol}")]
        public async Task<IActionResult> GetPriceHistory(
            string symbol,
            [FromQuery] string interval = "1h",
            [FromQuery] string baseCurrency = "USD",
            [FromQuery] DateTime? startTime = null,
            [FromQuery] DateTime? endTime = null)
        {
            var start = startTime ?? DateTime.UtcNow.AddDays(-7);
            var end = endTime ?? DateTime.UtcNow;

            _logger.LogInformation("Getting price history for symbol: {Symbol}, interval: {Interval}, base currency: {BaseCurrency}, start time: {StartTime}, end time: {EndTime}", 
                symbol, interval, baseCurrency, start, end);

            try
            {
                var history = await _priceFeedService.GetPriceHistoryAsync(symbol, baseCurrency, interval, start, end);
                if (history == null || history.DataPoints.Count == 0)
                {
                    return NotFound(new { Message = $"No price history found for {symbol}" });
                }

                return Ok(history);
            }
            catch (PriceFeedException ex)
            {
                _logger.LogError(ex, "Error getting price history for symbol: {Symbol}, interval: {Interval}", symbol, interval);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting price history for symbol: {Symbol}, interval: {Interval}", symbol, interval);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Gets all supported symbols
        /// </summary>
        /// <returns>List of supported symbols</returns>
        [HttpGet("symbols")]
        public async Task<IActionResult> GetSupportedSymbols()
        {
            _logger.LogInformation("Getting supported symbols");

            try
            {
                var symbols = await _priceFeedService.GetSupportedSymbolsAsync();
                return Ok(symbols);
            }
            catch (PriceFeedException ex)
            {
                _logger.LogError(ex, "Error getting supported symbols");
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting supported symbols");
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Gets all supported base currencies
        /// </summary>
        /// <returns>List of supported base currencies</returns>
        [HttpGet("currencies")]
        public async Task<IActionResult> GetSupportedBaseCurrencies()
        {
            _logger.LogInformation("Getting supported base currencies");

            try
            {
                var currencies = await _priceFeedService.GetSupportedBaseCurrenciesAsync();
                return Ok(currencies);
            }
            catch (PriceFeedException ex)
            {
                _logger.LogError(ex, "Error getting supported base currencies");
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting supported base currencies");
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Adds a new price source
        /// </summary>
        /// <param name="request">Price source to add</param>
        /// <returns>The added price source</returns>
        [HttpPost("sources")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddSource([FromBody] PriceSourceRequest request)
        {
            _logger.LogInformation("Adding price source: {Name}, Type: {Type}", request.Name, request.Type);

            try
            {
                var source = new PriceSource
                {
                    Name = request.Name,
                    Type = request.Type,
                    Url = request.Url,
                    ApiKey = request.ApiKey,
                    ApiSecret = request.ApiSecret,
                    Weight = request.Weight,
                    UpdateIntervalSeconds = request.UpdateIntervalSeconds,
                    TimeoutSeconds = request.TimeoutSeconds,
                    SupportedAssets = request.SupportedAssets,
                    Status = request.Status,
                    Config = request.Config
                };

                var addedSource = await _priceFeedService.AddSourceAsync(source);
                return Ok(addedSource);
            }
            catch (PriceFeedException ex)
            {
                _logger.LogError(ex, "Error adding price source: {Name}", request.Name);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error adding price source: {Name}", request.Name);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Updates a price source
        /// </summary>
        /// <param name="id">Price source ID</param>
        /// <param name="request">Price source to update</param>
        /// <returns>The updated price source</returns>
        [HttpPut("sources/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSource(Guid id, [FromBody] PriceSourceRequest request)
        {
            _logger.LogInformation("Updating price source: {Id}, {Name}", id, request.Name);

            try
            {
                var existingSource = await _priceFeedService.GetSourceByIdAsync(id);
                if (existingSource == null)
                {
                    return NotFound(new { Message = $"Price source not found: {id}" });
                }

                existingSource.Name = request.Name;
                existingSource.Type = request.Type;
                existingSource.Url = request.Url;
                existingSource.ApiKey = request.ApiKey;
                existingSource.ApiSecret = request.ApiSecret;
                existingSource.Weight = request.Weight;
                existingSource.UpdateIntervalSeconds = request.UpdateIntervalSeconds;
                existingSource.TimeoutSeconds = request.TimeoutSeconds;
                existingSource.SupportedAssets = request.SupportedAssets;
                existingSource.Status = request.Status;
                existingSource.Config = request.Config;

                var updatedSource = await _priceFeedService.UpdateSourceAsync(existingSource);
                return Ok(updatedSource);
            }
            catch (PriceFeedException ex)
            {
                _logger.LogError(ex, "Error updating price source: {Id}, {Name}", id, request.Name);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating price source: {Id}, {Name}", id, request.Name);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Gets all price sources
        /// </summary>
        /// <returns>List of all price sources</returns>
        [HttpGet("sources")]
        [Authorize]
        public async Task<IActionResult> GetAllSources()
        {
            _logger.LogInformation("Getting all price sources");

            try
            {
                var sources = await _priceFeedService.GetAllSourcesAsync();
                return Ok(sources);
            }
            catch (PriceFeedException ex)
            {
                _logger.LogError(ex, "Error getting all price sources");
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting all price sources");
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Gets a price source by ID
        /// </summary>
        /// <param name="id">Price source ID</param>
        /// <returns>The price source if found</returns>
        [HttpGet("sources/{id}")]
        [Authorize]
        public async Task<IActionResult> GetSourceById(Guid id)
        {
            _logger.LogInformation("Getting price source by ID: {Id}", id);

            try
            {
                var source = await _priceFeedService.GetSourceByIdAsync(id);
                if (source == null)
                {
                    return NotFound(new { Message = $"Price source not found: {id}" });
                }

                return Ok(source);
            }
            catch (PriceFeedException ex)
            {
                _logger.LogError(ex, "Error getting price source by ID: {Id}", id);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting price source by ID: {Id}", id);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Removes a price source
        /// </summary>
        /// <param name="id">Price source ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("sources/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveSource(Guid id)
        {
            _logger.LogInformation("Removing price source: {Id}", id);

            try
            {
                var success = await _priceFeedService.RemoveSourceAsync(id);
                if (!success)
                {
                    return BadRequest(new { Message = $"Failed to remove price source: {id}" });
                }

                return Ok(new { Message = "Price source removed successfully" });
            }
            catch (PriceFeedException ex)
            {
                _logger.LogError(ex, "Error removing price source: {Id}", id);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error removing price source: {Id}", id);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Fetches prices from all sources
        /// </summary>
        /// <param name="baseCurrency">Base currency for the prices</param>
        /// <returns>List of fetched prices</returns>
        [HttpPost("fetch")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> FetchPrices([FromQuery] string baseCurrency = "USD")
        {
            _logger.LogInformation("Fetching prices for base currency: {BaseCurrency}", baseCurrency);

            try
            {
                var prices = await _priceFeedService.FetchPricesAsync(baseCurrency);
                return Ok(prices);
            }
            catch (PriceFeedException ex)
            {
                _logger.LogError(ex, "Error fetching prices");
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching prices");
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Fetches price for a specific symbol
        /// </summary>
        /// <param name="symbol">Symbol to fetch price for</param>
        /// <param name="baseCurrency">Base currency for the price</param>
        /// <returns>List of fetched prices for the symbol</returns>
        [HttpPost("fetch/{symbol}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> FetchPriceForSymbol(string symbol, [FromQuery] string baseCurrency = "USD")
        {
            _logger.LogInformation("Fetching price for symbol: {Symbol}, base currency: {BaseCurrency}", symbol, baseCurrency);

            try
            {
                var prices = await _priceFeedService.FetchPriceForSymbolAsync(symbol, baseCurrency);
                return Ok(prices);
            }
            catch (PriceFeedException ex)
            {
                _logger.LogError(ex, "Error fetching price for symbol: {Symbol}", symbol);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching price for symbol: {Symbol}", symbol);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Submits a price to the Neo N3 oracle contract
        /// </summary>
        /// <param name="request">Price submission request</param>
        /// <returns>Transaction hash</returns>
        [HttpPost("submit")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SubmitToOracle([FromBody] SubmitPriceRequest request)
        {
            _logger.LogInformation("Submitting price to oracle: {Symbol}, {BaseCurrency}, {Value}", 
                request.Symbol, request.BaseCurrency, request.Value);

            try
            {
                var price = new Price
                {
                    Symbol = request.Symbol,
                    BaseCurrency = request.BaseCurrency,
                    Value = request.Value,
                    Timestamp = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                var transactionHash = await _priceFeedService.SubmitToOracleAsync(price);
                return Ok(new { TransactionHash = transactionHash });
            }
            catch (PriceFeedException ex)
            {
                _logger.LogError(ex, "Error submitting price to oracle: {Symbol}", request.Symbol);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error submitting price to oracle: {Symbol}", request.Symbol);
                return StatusCode(500, new { Message = "An unexpected error occurred" });
            }
        }
    }
}
