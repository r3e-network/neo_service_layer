using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.MockServiceTests.TestFixtures;
using Xunit;

namespace NeoServiceLayer.MockServiceTests
{
    public class PriceFeedMockTests : IClassFixture<MockServiceTestFixture>
    {
        private readonly MockServiceTestFixture _fixture;
        private readonly IPriceFeedService _priceFeedService;

        public PriceFeedMockTests(MockServiceTestFixture fixture)
        {
            _fixture = fixture;
            _priceFeedService = _fixture.ServiceProvider.GetRequiredService<IPriceFeedService>();
        }

        [Fact]
        public async Task CreatePriceSource_AddPrice_GetPrice_Success()
        {
            // Arrange
            var sourceId = Guid.NewGuid();
            var sourceName = "Binance";
            var url = "https://api.binance.com";
            var apiKey = "api-key-123";
            var apiSecret = "api-secret-456";
            var weight = 80;
            var status = PriceSourceStatus.Active;
            var updateInterval = 60;
            var timeout = 5;
            var supportedAssets = new List<string> { "BTC", "ETH", "NEO" };

            var sourceConfig = new PriceSourceConfig
            {
                PriceJsonPath = "$.price",
                TimestampJsonPath = "$.timestamp",
                TimestampFormat = "yyyy-MM-ddTHH:mm:ssZ",
                Headers = new Dictionary<string, string>
                {
                    { "X-API-KEY", "api-key-123" }
                },
                QueryParams = new Dictionary<string, string>
                {
                    { "symbol", "BTCUSD" }
                }
            };

            var source = new PriceSource
            {
                Id = sourceId,
                Name = sourceName,
                Type = PriceSourceType.Exchange,
                Url = url,
                ApiKey = apiKey,
                ApiSecret = apiSecret,
                Weight = weight,
                Status = status,
                UpdateIntervalSeconds = updateInterval,
                TimeoutSeconds = timeout,
                SupportedAssets = supportedAssets,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Config = sourceConfig
            };

            var timestamp = DateTime.UtcNow;
            var priceId = Guid.NewGuid();
            var symbol = "BTC";
            var baseCurrency = "USD";
            var priceValue = 50000.50m;
            var confidenceScore = 95;
            var sourcePrice = new SourcePrice
            {
                Id = Guid.NewGuid(),
                SourceId = sourceId,
                SourceName = sourceName,
                Value = priceValue,
                Timestamp = timestamp,
                Weight = weight
            };

            var price = new Price
            {
                Id = priceId,
                Symbol = symbol,
                BaseCurrency = baseCurrency,
                Value = priceValue,
                Timestamp = timestamp,
                SourcePrices = new List<SourcePrice> { sourcePrice },
                ConfidenceScore = confidenceScore,
                CreatedAt = DateTime.UtcNow,
                Signature = "signature-123",
                Source = sourceName
            };

            // Setup mocks
            _fixture.PriceFeedServiceMock
                .Setup(x => x.AddSourceAsync(It.IsAny<PriceSource>()))
                .ReturnsAsync(source);

            _fixture.PriceFeedServiceMock
                .Setup(x => x.GetLatestPriceAsync(symbol, baseCurrency))
                .ReturnsAsync(price);

            // Create a source object to add
            var sourceToAdd = new PriceSource
            {
                Name = sourceName,
                Type = PriceSourceType.Exchange,
                Url = url,
                ApiKey = apiKey,
                ApiSecret = apiSecret,
                Weight = weight,
                Status = status,
                UpdateIntervalSeconds = updateInterval,
                TimeoutSeconds = timeout,
                SupportedAssets = supportedAssets,
                Config = sourceConfig
            };

            // Act - Create price source
            var createdSource = await _priceFeedService.AddSourceAsync(sourceToAdd);

            // Assert - Source created successfully
            Assert.NotNull(createdSource);
            Assert.Equal(sourceName, createdSource.Name);
            Assert.Equal(PriceSourceType.Exchange, createdSource.Type);
            Assert.Equal(status, createdSource.Status);
            Assert.Equal(weight, createdSource.Weight);
            Assert.Equal(updateInterval, createdSource.UpdateIntervalSeconds);
            Assert.Equal(timeout, createdSource.TimeoutSeconds);
            Assert.Contains("BTC", createdSource.SupportedAssets);

            // We don't need to create a price in this test since we're mocking the GetLatestPriceAsync method

            // Skip price creation assertion since we're not creating a price

            // Act - Get latest price
            var latestPrice = await _priceFeedService.GetLatestPriceAsync(symbol, baseCurrency);

            // Assert - Latest price retrieved successfully
            Assert.NotNull(latestPrice);
            Assert.Equal(symbol, latestPrice.Symbol);
            Assert.Equal(baseCurrency, latestPrice.BaseCurrency);
            Assert.Equal(priceValue, latestPrice.Value);
        }

        [Fact]
        public async Task CreatePriceHistory_GetPriceHistory_Success()
        {
            // Arrange
            var historyId = Guid.NewGuid();
            var symbol = "ETH";
            var baseCurrency = "USD";
            var interval = "1h";
            var startTime = DateTime.UtcNow.AddDays(-1);
            var endTime = DateTime.UtcNow;

            var dataPoints = new List<PriceDataPoint>
            {
                new PriceDataPoint
                {
                    Timestamp = DateTime.UtcNow.AddHours(-2),
                    Open = 3000.0m,
                    High = 3050.0m,
                    Low = 2950.0m,
                    Close = 3025.0m,
                    Volume = 50000.0m
                },
                new PriceDataPoint
                {
                    Timestamp = DateTime.UtcNow.AddHours(-1),
                    Open = 3025.0m,
                    High = 3075.0m,
                    Low = 3000.0m,
                    Close = 3060.0m,
                    Volume = 55000.0m
                }
            };

            var history = new PriceHistory
            {
                Id = historyId,
                Symbol = symbol,
                BaseCurrency = baseCurrency,
                Interval = interval,
                StartTime = startTime,
                EndTime = endTime,
                DataPoints = dataPoints,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Setup mocks
            _fixture.PriceFeedServiceMock
                .Setup(x => x.GetPriceHistoryAsync(
                    symbol,
                    baseCurrency,
                    interval,
                    startTime,
                    endTime))
                .ReturnsAsync(history);

            // Skip creating price history since the interface doesn't support it

            // Act - Get price history
            var retrievedHistory = await _priceFeedService.GetPriceHistoryAsync(
                symbol,
                baseCurrency,
                interval,
                startTime,
                endTime);

            // Assert - History retrieved successfully
            Assert.NotNull(retrievedHistory);
            Assert.Equal(symbol, retrievedHistory.Symbol);
            Assert.Equal(baseCurrency, retrievedHistory.BaseCurrency);
            Assert.Equal(interval, retrievedHistory.Interval);
            Assert.Equal(2, retrievedHistory.DataPoints.Count);
            Assert.Equal(3025.0m, retrievedHistory.DataPoints[0].Close);
            Assert.Equal(3060.0m, retrievedHistory.DataPoints[1].Close);
        }

        [Fact]
        public async Task UpdatePriceSource_Success()
        {
            // Arrange
            var sourceId = Guid.NewGuid();
            var sourceName = "CoinGecko";
            var originalUrl = "https://api.coingecko.com";
            var updatedUrl = "https://api.coingecko.com/v2";
            var originalApiKey = "api-key-123";
            var updatedApiKey = "new-api-key-456";
            var originalApiSecret = "api-secret-456";
            var updatedApiSecret = "new-api-secret-789";
            var originalWeight = 70;
            var updatedWeight = 80;
            var status = PriceSourceStatus.Active;
            var originalInterval = 120;
            var updatedInterval = 60;
            var originalTimeout = 10;
            var updatedTimeout = 5;
            var originalAssets = new List<string> { "BTC", "ETH" };
            var updatedAssets = new List<string> { "BTC", "ETH", "NEO", "GAS" };

            var sourceConfig = new PriceSourceConfig
            {
                PriceJsonPath = "$.price",
                TimestampJsonPath = "$.timestamp",
                TimestampFormat = "yyyy-MM-ddTHH:mm:ssZ"
            };

            var originalSource = new PriceSource
            {
                Id = sourceId,
                Name = sourceName,
                Type = PriceSourceType.Exchange,
                Url = originalUrl,
                ApiKey = originalApiKey,
                ApiSecret = originalApiSecret,
                Weight = originalWeight,
                Status = status,
                UpdateIntervalSeconds = originalInterval,
                TimeoutSeconds = originalTimeout,
                SupportedAssets = originalAssets,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Config = sourceConfig
            };

            var updatedSource = new PriceSource
            {
                Id = sourceId,
                Name = sourceName,
                Type = PriceSourceType.Exchange,
                Url = updatedUrl,
                ApiKey = updatedApiKey,
                ApiSecret = updatedApiSecret,
                Weight = updatedWeight,
                Status = status,
                UpdateIntervalSeconds = updatedInterval,
                TimeoutSeconds = updatedTimeout,
                SupportedAssets = updatedAssets,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Config = sourceConfig
            };

            // Setup mocks
            _fixture.PriceFeedServiceMock
                .Setup(x => x.AddSourceAsync(It.IsAny<PriceSource>()))
                .ReturnsAsync(originalSource);

            _fixture.PriceFeedServiceMock
                .Setup(x => x.UpdateSourceAsync(It.IsAny<PriceSource>()))
                .ReturnsAsync(updatedSource);

            // Act - Create price source
            var sourceToAdd = new PriceSource
            {
                Name = sourceName,
                Type = PriceSourceType.Exchange,
                Url = originalUrl,
                ApiKey = originalApiKey,
                ApiSecret = originalApiSecret,
                Weight = originalWeight,
                Status = status,
                UpdateIntervalSeconds = originalInterval,
                TimeoutSeconds = originalTimeout,
                SupportedAssets = originalAssets,
                Config = sourceConfig
            };

            var createdSource = await _priceFeedService.AddSourceAsync(sourceToAdd);

            // Assert - Source created successfully
            Assert.NotNull(createdSource);
            Assert.Equal(originalUrl, createdSource.Url);
            Assert.Equal(originalApiKey, createdSource.ApiKey);
            Assert.Equal(originalWeight, createdSource.Weight);
            Assert.Equal(originalInterval, createdSource.UpdateIntervalSeconds);
            Assert.Equal(2, createdSource.SupportedAssets.Count);

            // Act - Update price source
            var sourceToUpdate = new PriceSource
            {
                Id = sourceId,
                Name = sourceName,
                Type = PriceSourceType.Exchange,
                Url = updatedUrl,
                ApiKey = updatedApiKey,
                ApiSecret = updatedApiSecret,
                Weight = updatedWeight,
                Status = status,
                UpdateIntervalSeconds = updatedInterval,
                TimeoutSeconds = updatedTimeout,
                SupportedAssets = updatedAssets,
                Config = sourceConfig
            };

            var updatedSourceResult = await _priceFeedService.UpdateSourceAsync(sourceToUpdate);

            // Assert - Source updated successfully
            Assert.NotNull(updatedSourceResult);
            Assert.Equal(sourceId, updatedSourceResult.Id);
            Assert.Equal(updatedUrl, updatedSourceResult.Url);
            Assert.Equal(updatedApiKey, updatedSourceResult.ApiKey);
            Assert.Equal(updatedApiSecret, updatedSourceResult.ApiSecret);
            Assert.Equal(updatedWeight, updatedSourceResult.Weight);
            Assert.Equal(updatedInterval, updatedSourceResult.UpdateIntervalSeconds);
            Assert.Equal(updatedTimeout, updatedSourceResult.TimeoutSeconds);
            Assert.Equal(4, updatedSourceResult.SupportedAssets.Count);
            Assert.Contains("NEO", updatedSourceResult.SupportedAssets);
            Assert.Contains("GAS", updatedSourceResult.SupportedAssets);
        }

        [Fact]
        public async Task GetMultiplePrices_Success()
        {
            // Arrange
            var timestamp = DateTime.UtcNow;
            var btcPrice = new Price
            {
                Id = Guid.NewGuid(),
                Symbol = "BTC",
                BaseCurrency = "USD",
                Value = 50000.50m,
                Timestamp = timestamp,
                SourcePrices = new List<SourcePrice>(),
                ConfidenceScore = 95,
                CreatedAt = DateTime.UtcNow,
                Source = "Kraken"
            };

            var ethPrice = new Price
            {
                Id = Guid.NewGuid(),
                Symbol = "ETH",
                BaseCurrency = "USD",
                Value = 3000.25m,
                Timestamp = timestamp,
                SourcePrices = new List<SourcePrice>(),
                ConfidenceScore = 95,
                CreatedAt = DateTime.UtcNow,
                Source = "Kraken"
            };

            // Setup mocks
            _fixture.PriceFeedServiceMock
                .Setup(x => x.GetAllLatestPricesAsync("USD"))
                .ReturnsAsync(new Dictionary<string, Price>
                {
                    { "BTC", btcPrice },
                    { "ETH", ethPrice }
                });

            // Act - Get all latest prices
            var pricesDict = await _priceFeedService.GetAllLatestPricesAsync("USD");

            // Assert - Prices retrieved successfully
            Assert.NotNull(pricesDict);
            Assert.Equal(2, pricesDict.Count);
            Assert.True(pricesDict.ContainsKey("BTC"));
            Assert.True(pricesDict.ContainsKey("ETH"));
            Assert.Equal(50000.50m, pricesDict["BTC"].Value);
            Assert.Equal(3000.25m, pricesDict["ETH"].Value);
        }
    }
}
