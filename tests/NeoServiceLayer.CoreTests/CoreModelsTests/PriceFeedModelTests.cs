using System;
using System.Collections.Generic;
using NeoServiceLayer.Core.Models;
using Xunit;

namespace NeoServiceLayer.CoreTests.CoreModelsTests
{
    public class PriceFeedModelTests
    {
        [Fact]
        public void PriceSource_Properties_Work()
        {
            // Arrange
            var priceSource = new PriceSource
            {
                Id = Guid.NewGuid(),
                Name = "TestSource",
                Type = PriceSourceType.Exchange,
                Url = "https://api.example.com/prices",
                ApiKey = "api-key-123",
                ApiSecret = "api-secret-123",
                Weight = 50,
                Status = PriceSourceStatus.Active,
                UpdateIntervalSeconds = 60,
                TimeoutSeconds = 30,
                SupportedAssets = new List<string> { "NEO", "GAS" },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastSuccessfulFetchAt = DateTime.UtcNow,
                Config = new PriceSourceConfig
                {
                    PriceJsonPath = "$.price",
                    TimestampJsonPath = "$.timestamp",
                    TimestampFormat = "yyyy-MM-ddTHH:mm:ssZ"
                }
            };

            // Act & Assert
            Assert.Equal("TestSource", priceSource.Name);
            Assert.Equal(PriceSourceType.Exchange, priceSource.Type);
            Assert.Equal("https://api.example.com/prices", priceSource.Url);
            Assert.Equal("api-key-123", priceSource.ApiKey);
            Assert.Equal("api-secret-123", priceSource.ApiSecret);
            Assert.Equal(50, priceSource.Weight);
            Assert.Equal(PriceSourceStatus.Active, priceSource.Status);
            Assert.Equal(60, priceSource.UpdateIntervalSeconds);
            Assert.Equal(30, priceSource.TimeoutSeconds);
            Assert.Equal(2, priceSource.SupportedAssets.Count);
            Assert.NotNull(priceSource.Config);
            Assert.Equal("$.price", priceSource.Config.PriceJsonPath);
        }

        [Fact]
        public void PriceData_Properties_Work()
        {
            // Arrange
            var priceData = new PriceData
            {
                Id = Guid.NewGuid(),
                Symbol = "NEO/USD",
                Price = 50.25m,
                Timestamp = DateTime.UtcNow,
                Source = "TestSource",
                ConfidenceScore = 95,
                TransactionHash = "0x1234567890abcdef",
                IsSubmitted = true,
                SubmittedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, string>
                {
                    { "exchange", "Binance" }
                }
            };

            // Act & Assert
            Assert.Equal("NEO/USD", priceData.Symbol);
            Assert.Equal(50.25m, priceData.Price);
            Assert.Equal("TestSource", priceData.Source);
            Assert.Equal(95, priceData.ConfidenceScore);
            Assert.Equal("0x1234567890abcdef", priceData.TransactionHash);
            Assert.True(priceData.IsSubmitted);
            Assert.NotNull(priceData.SubmittedAt);
            Assert.Single(priceData.Metadata);
            Assert.Equal("Binance", priceData.Metadata["exchange"]);
        }

        [Fact]
        public void PriceHistory_Properties_Work()
        {
            // Arrange
            var startTime = DateTime.UtcNow.AddDays(-7);
            var endTime = DateTime.UtcNow;
            var priceHistory = new PriceHistory
            {
                Id = Guid.NewGuid(),
                Symbol = "NEO",
                BaseCurrency = "USD",
                Interval = "1d",
                StartTime = startTime,
                EndTime = endTime,
                DataPoints = new List<PriceDataPoint>
                {
                    new PriceDataPoint
                    {
                        Timestamp = startTime,
                        Open = 50.0m,
                        High = 52.5m,
                        Low = 49.0m,
                        Close = 51.0m,
                        Volume = 10000.0m
                    },
                    new PriceDataPoint
                    {
                        Timestamp = endTime,
                        Open = 51.0m,
                        High = 53.0m,
                        Low = 50.5m,
                        Close = 52.0m,
                        Volume = 12000.0m
                    }
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Act & Assert
            Assert.Equal("NEO", priceHistory.Symbol);
            Assert.Equal("USD", priceHistory.BaseCurrency);
            Assert.Equal("1d", priceHistory.Interval);
            Assert.Equal(startTime, priceHistory.StartTime);
            Assert.Equal(endTime, priceHistory.EndTime);
            Assert.Equal(2, priceHistory.DataPoints.Count);
            Assert.Equal(50.0m, priceHistory.DataPoints[0].Open);
            Assert.Equal(52.0m, priceHistory.DataPoints[1].Close);
        }
    }
}
