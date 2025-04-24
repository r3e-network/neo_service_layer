using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NeoServiceLayer.Core.Exceptions;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.MockServiceTests.TestFixtures;
using Xunit;

namespace NeoServiceLayer.MockServiceTests
{
    public class PriceFeedErrorHandlingTests : IClassFixture<MockServiceTestFixture>
    {
        private readonly MockServiceTestFixture _fixture;
        private readonly IPriceFeedService _priceFeedService;

        public PriceFeedErrorHandlingTests(MockServiceTestFixture fixture)
        {
            _fixture = fixture;
            _priceFeedService = _fixture.ServiceProvider.GetRequiredService<IPriceFeedService>();
        }

        [Fact]
        public async Task GetLatestPrice_SymbolNotFound_ReturnsNull()
        {
            // Arrange
            var symbol = "UNKNOWN";
            var baseCurrency = "USD";

            _fixture.PriceFeedServiceMock
                .Setup(x => x.GetLatestPriceAsync(symbol, baseCurrency))
                .ReturnsAsync((Price)null);

            // Act
            var result = await _priceFeedService.GetLatestPriceAsync(symbol, baseCurrency);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddSource_DuplicateName_ThrowsException()
        {
            // Arrange
            var existingSource = new PriceSource
            {
                Name = "Binance",
                Type = PriceSourceType.Exchange
            };

            _fixture.PriceFeedServiceMock
                .Setup(x => x.AddSourceAsync(It.Is<PriceSource>(s => s.Name == "Binance")))
                .ThrowsAsync(new InvalidOperationException("A price source with this name already exists"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _priceFeedService.AddSourceAsync(existingSource));
            
            Assert.Contains("already exists", exception.Message);
        }

        [Fact]
        public async Task UpdateSource_SourceNotFound_ThrowsException()
        {
            // Arrange
            var nonExistentSourceId = Guid.NewGuid();
            var sourceToUpdate = new PriceSource
            {
                Id = nonExistentSourceId,
                Name = "NonExistentSource",
                Type = PriceSourceType.Exchange
            };

            _fixture.PriceFeedServiceMock
                .Setup(x => x.UpdateSourceAsync(It.Is<PriceSource>(s => s.Id == nonExistentSourceId)))
                .ThrowsAsync(new KeyNotFoundException("Price source not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _priceFeedService.UpdateSourceAsync(sourceToUpdate));
            
            Assert.Contains("not found", exception.Message);
        }

        [Fact]
        public async Task GetPriceHistory_InvalidInterval_ThrowsException()
        {
            // Arrange
            var symbol = "BTC";
            var baseCurrency = "USD";
            var invalidInterval = "invalid";
            var startTime = DateTime.UtcNow.AddDays(-1);
            var endTime = DateTime.UtcNow;

            _fixture.PriceFeedServiceMock
                .Setup(x => x.GetPriceHistoryAsync(symbol, baseCurrency, invalidInterval, startTime, endTime))
                .ThrowsAsync(new ArgumentException("Invalid interval format"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _priceFeedService.GetPriceHistoryAsync(symbol, baseCurrency, invalidInterval, startTime, endTime));
            
            Assert.Contains("Invalid interval", exception.Message);
        }

        [Fact]
        public async Task GetPriceHistory_EndTimeBeforeStartTime_ThrowsException()
        {
            // Arrange
            var symbol = "BTC";
            var baseCurrency = "USD";
            var interval = "1h";
            var startTime = DateTime.UtcNow;
            var endTime = DateTime.UtcNow.AddDays(-1); // End time before start time

            _fixture.PriceFeedServiceMock
                .Setup(x => x.GetPriceHistoryAsync(symbol, baseCurrency, interval, startTime, endTime))
                .ThrowsAsync(new ArgumentException("End time must be after start time"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _priceFeedService.GetPriceHistoryAsync(symbol, baseCurrency, interval, startTime, endTime));
            
            Assert.Contains("End time must be after start time", exception.Message);
        }

        [Fact]
        public async Task SubmitToOracle_InvalidPrice_ThrowsException()
        {
            // Arrange
            var invalidPrice = new Price
            {
                Symbol = "BTC",
                BaseCurrency = "USD",
                Value = -1 // Invalid negative price
            };

            _fixture.PriceFeedServiceMock
                .Setup(x => x.SubmitToOracleAsync(It.Is<Price>(p => p.Value < 0)))
                .ThrowsAsync(new ArgumentException("Price value must be positive"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _priceFeedService.SubmitToOracleAsync(invalidPrice));
            
            Assert.Contains("must be positive", exception.Message);
        }

        [Fact]
        public async Task SubmitBatchToOracle_EmptyBatch_ThrowsException()
        {
            // Arrange
            var emptyBatch = new List<Price>();

            _fixture.PriceFeedServiceMock
                .Setup(x => x.SubmitBatchToOracleAsync(It.Is<IEnumerable<Price>>(p => !p.Any())))
                .ThrowsAsync(new ArgumentException("Price batch cannot be empty"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _priceFeedService.SubmitBatchToOracleAsync(emptyBatch));
            
            Assert.Contains("cannot be empty", exception.Message);
        }
    }
}
