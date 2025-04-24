using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.IntegrationTests.TestFixtures;
using Xunit;

namespace NeoServiceLayer.IntegrationTests
{
    public class PriceFeedIntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly IntegrationTestFixture _fixture;
        private readonly IPriceFeedService _priceFeedService;

        public PriceFeedIntegrationTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
            _priceFeedService = _fixture.ServiceProvider.GetRequiredService<IPriceFeedService>();
        }

        [Fact]
        public async Task CreatePriceSource_AddPrice_GetPrice_Success()
        {
            // Arrange - Create price source
            var sourceName = $"Binance_{Guid.NewGuid()}";
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
            
            var source = await _priceFeedService.AddPriceSourceAsync(
                sourceName,
                PriceSourceType.Exchange,
                "https://api.binance.com",
                "api-key-123",
                "api-secret-456",
                80,
                PriceSourceStatus.Active,
                60,
                5,
                new List<string> { "BTC", "ETH", "NEO" },
                sourceConfig);
            
            // Assert - Source created successfully
            Assert.NotNull(source);
            Assert.Equal(sourceName, source.Name);
            Assert.Equal(PriceSourceType.Exchange, source.Type);
            Assert.Equal(PriceSourceStatus.Active, source.Status);
            Assert.Equal(80, source.Weight);
            Assert.Equal(60, source.UpdateIntervalSeconds);
            Assert.Equal(5, source.TimeoutSeconds);
            Assert.Contains("BTC", source.SupportedAssets);

            // Act - Create price
            var timestamp = DateTime.UtcNow;
            var sourcePrice = new SourcePrice
            {
                SourceId = source.Id,
                SourceName = source.Name,
                Value = 50000.50m,
                Timestamp = timestamp,
                Weight = source.Weight
            };
            
            var price = await _priceFeedService.AddPriceAsync(
                "BTC",
                "USD",
                50000.50m,
                timestamp,
                new List<SourcePrice> { sourcePrice },
                95,
                "Binance");
            
            // Assert - Price created successfully
            Assert.NotNull(price);
            Assert.Equal("BTC", price.Symbol);
            Assert.Equal("USD", price.BaseCurrency);
            Assert.Equal(50000.50m, price.Value);
            Assert.Equal(timestamp, price.Timestamp);
            Assert.Single(price.SourcePrices);
            Assert.Equal(95, price.ConfidenceScore);
            Assert.Equal("Binance", price.Source);

            // Act - Get latest price
            var latestPrice = await _priceFeedService.GetLatestPriceAsync("BTC", "USD");
            
            // Assert - Latest price retrieved successfully
            Assert.NotNull(latestPrice);
            Assert.Equal("BTC", latestPrice.Symbol);
            Assert.Equal("USD", latestPrice.BaseCurrency);
            Assert.Equal(50000.50m, latestPrice.Value);
        }

        [Fact]
        public async Task CreatePriceHistory_GetPriceHistory_Success()
        {
            // Arrange - Create price history
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
            
            var history = await _priceFeedService.AddPriceHistoryAsync(
                symbol,
                baseCurrency,
                interval,
                startTime,
                endTime,
                dataPoints);
            
            // Assert - History created successfully
            Assert.NotNull(history);
            Assert.Equal(symbol, history.Symbol);
            Assert.Equal(baseCurrency, history.BaseCurrency);
            Assert.Equal(interval, history.Interval);
            Assert.Equal(startTime, history.StartTime);
            Assert.Equal(endTime, history.EndTime);
            Assert.Equal(2, history.DataPoints.Count);

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
            // Arrange - Create price source
            var sourceName = $"CoinGecko_{Guid.NewGuid()}";
            var sourceConfig = new PriceSourceConfig
            {
                PriceJsonPath = "$.price",
                TimestampJsonPath = "$.timestamp",
                TimestampFormat = "yyyy-MM-ddTHH:mm:ssZ"
            };
            
            var source = await _priceFeedService.AddPriceSourceAsync(
                sourceName,
                PriceSourceType.API,
                "https://api.coingecko.com",
                "api-key-123",
                "api-secret-456",
                70,
                PriceSourceStatus.Active,
                120,
                10,
                new List<string> { "BTC", "ETH" },
                sourceConfig);
            
            // Act - Update price source
            var updatedSource = await _priceFeedService.UpdatePriceSourceAsync(
                source.Id,
                sourceName,
                PriceSourceType.API,
                "https://api.coingecko.com/v2", // Updated URL
                "new-api-key-456", // Updated API key
                "new-api-secret-789", // Updated API secret
                80, // Updated weight
                PriceSourceStatus.Active,
                60, // Updated interval
                5, // Updated timeout
                new List<string> { "BTC", "ETH", "NEO", "GAS" }, // Updated supported assets
                sourceConfig);
            
            // Assert - Source updated successfully
            Assert.NotNull(updatedSource);
            Assert.Equal(source.Id, updatedSource.Id);
            Assert.Equal("https://api.coingecko.com/v2", updatedSource.Url);
            Assert.Equal("new-api-key-456", updatedSource.ApiKey);
            Assert.Equal("new-api-secret-789", updatedSource.ApiSecret);
            Assert.Equal(80, updatedSource.Weight);
            Assert.Equal(60, updatedSource.UpdateIntervalSeconds);
            Assert.Equal(5, updatedSource.TimeoutSeconds);
            Assert.Equal(4, updatedSource.SupportedAssets.Count);
            Assert.Contains("NEO", updatedSource.SupportedAssets);
            Assert.Contains("GAS", updatedSource.SupportedAssets);
        }

        [Fact]
        public async Task GetMultiplePrices_Success()
        {
            // Arrange - Create price source
            var sourceName = $"Kraken_{Guid.NewGuid()}";
            var source = await _priceFeedService.AddPriceSourceAsync(
                sourceName,
                PriceSourceType.Exchange,
                "https://api.kraken.com",
                "api-key-123",
                "api-secret-456",
                75,
                PriceSourceStatus.Active,
                60,
                5,
                new List<string> { "BTC", "ETH", "NEO" },
                new PriceSourceConfig());
            
            // Create prices for multiple symbols
            var timestamp = DateTime.UtcNow;
            var sourcePrice1 = new SourcePrice
            {
                SourceId = source.Id,
                SourceName = source.Name,
                Value = 50000.50m,
                Timestamp = timestamp,
                Weight = source.Weight
            };
            
            var sourcePrice2 = new SourcePrice
            {
                SourceId = source.Id,
                SourceName = source.Name,
                Value = 3000.25m,
                Timestamp = timestamp,
                Weight = source.Weight
            };
            
            await _priceFeedService.AddPriceAsync(
                "BTC",
                "USD",
                50000.50m,
                timestamp,
                new List<SourcePrice> { sourcePrice1 },
                95,
                "Kraken");
            
            await _priceFeedService.AddPriceAsync(
                "ETH",
                "USD",
                3000.25m,
                timestamp,
                new List<SourcePrice> { sourcePrice2 },
                95,
                "Kraken");
            
            // Act - Get multiple prices
            var prices = await _priceFeedService.GetLatestPricesAsync(
                new List<string> { "BTC", "ETH" },
                "USD");
            
            // Assert - Prices retrieved successfully
            Assert.NotNull(prices);
            Assert.Equal(2, prices.Count);
            Assert.Contains(prices, p => p.Symbol == "BTC" && p.Value == 50000.50m);
            Assert.Contains(prices, p => p.Symbol == "ETH" && p.Value == 3000.25m);
        }
    }
}
