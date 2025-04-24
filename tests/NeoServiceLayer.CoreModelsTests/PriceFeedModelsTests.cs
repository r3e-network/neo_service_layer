using System;
using System.Collections.Generic;
using NeoServiceLayer.Core.Models;
using Xunit;

namespace NeoServiceLayer.CoreModelsTests
{
    public class PriceFeedModelsTests
    {
        [Fact]
        public void PriceSource_Properties_Work()
        {
            // Arrange
            var config = new PriceSourceConfig
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
                Id = Guid.NewGuid(),
                Name = "Binance",
                Type = PriceSourceType.Exchange,
                Url = "https://api.binance.com",
                ApiKey = "api-key-123",
                ApiSecret = "api-secret-456",
                Weight = 80,
                Status = PriceSourceStatus.Active,
                UpdateIntervalSeconds = 60,
                TimeoutSeconds = 5,
                SupportedAssets = new List<string> { "BTC", "ETH", "NEO" },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Config = config
            };

            // Act & Assert
            Assert.Equal("Binance", source.Name);
            Assert.Equal(PriceSourceType.Exchange, source.Type);
            Assert.Equal("https://api.binance.com", source.Url);
            Assert.Equal("api-key-123", source.ApiKey);
            Assert.Equal("api-secret-456", source.ApiSecret);
            Assert.Equal(80, source.Weight);
            Assert.Equal(PriceSourceStatus.Active, source.Status);
            Assert.Equal(60, source.UpdateIntervalSeconds);
            Assert.Equal(5, source.TimeoutSeconds);
            Assert.Contains("BTC", source.SupportedAssets);
            Assert.Equal("$.price", source.Config.PriceJsonPath);
            Assert.Equal("X-API-KEY", source.Config.Headers.Keys.First());
            Assert.Equal("api-key-123", source.Config.Headers["X-API-KEY"]);
        }

        [Fact]
        public void Price_Properties_Work()
        {
            // Arrange
            var timestamp = DateTime.UtcNow;
            var sourcePrice = new SourcePrice
            {
                Id = Guid.NewGuid(),
                SourceId = Guid.NewGuid(),
                SourceName = "Binance",
                Value = 50000.50m,
                Timestamp = timestamp,
                Weight = 80
            };

            var price = new Price
            {
                Id = Guid.NewGuid(),
                Symbol = "BTC",
                BaseCurrency = "USD",
                Value = 50000.50m,
                Timestamp = timestamp,
                SourcePrices = new List<SourcePrice> { sourcePrice },
                ConfidenceScore = 95,
                CreatedAt = DateTime.UtcNow,
                Signature = "signature-123",
                Source = "Binance"
            };

            // Act & Assert
            Assert.Equal("BTC", price.Symbol);
            Assert.Equal("USD", price.BaseCurrency);
            Assert.Equal(50000.50m, price.Value);
            Assert.Equal(timestamp, price.Timestamp);
            Assert.Single(price.SourcePrices);
            Assert.Equal(95, price.ConfidenceScore);
            Assert.Equal("signature-123", price.Signature);
            Assert.Equal("Binance", price.Source);

            // Check source price
            var sp = price.SourcePrices[0];
            Assert.Equal("Binance", sp.SourceName);
            Assert.Equal(50000.50m, sp.Value);
            Assert.Equal(80, sp.Weight);
        }

        [Fact]
        public void PriceHistory_Properties_Work()
        {
            // Arrange
            var startTime = DateTime.UtcNow.AddDays(-1);
            var endTime = DateTime.UtcNow;
            var history = new PriceHistory
            {
                Id = Guid.NewGuid(),
                Symbol = "ETH",
                BaseCurrency = "USD",
                Interval = "1h",
                StartTime = startTime,
                EndTime = endTime,
                DataPoints = new List<PriceDataPoint>
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
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Act & Assert
            Assert.Equal("ETH", history.Symbol);
            Assert.Equal("USD", history.BaseCurrency);
            Assert.Equal("1h", history.Interval);
            Assert.Equal(startTime, history.StartTime);
            Assert.Equal(endTime, history.EndTime);
            Assert.Equal(2, history.DataPoints.Count);
            Assert.Equal(3025.0m, history.DataPoints[0].Close);
            Assert.Equal(3060.0m, history.DataPoints[1].Close);
            Assert.Equal(50000.0m, history.DataPoints[0].Volume);
            Assert.Equal(55000.0m, history.DataPoints[1].Volume);
        }

        [Fact]
        public void PriceDataPoint_Properties_Work()
        {
            // Arrange
            var data = new PriceDataPoint
            {
                Timestamp = DateTime.UtcNow,
                Open = 3000.0m,
                High = 3050.0m,
                Low = 2950.0m,
                Close = 3025.0m,
                Volume = 50000.0m
            };

            // Act & Assert
            Assert.Equal(3000.0m, data.Open);
            Assert.Equal(3050.0m, data.High);
            Assert.Equal(2950.0m, data.Low);
            Assert.Equal(3025.0m, data.Close);
            Assert.Equal(50000.0m, data.Volume);
        }
    }
}
