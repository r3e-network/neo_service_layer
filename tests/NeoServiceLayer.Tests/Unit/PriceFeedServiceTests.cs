using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NeoServiceLayer.Core;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using PriceSourceTypeEnum = NeoServiceLayer.Core.Enums.PriceSourceType;
using NeoServiceLayer.Services.PriceFeed;
using NeoServiceLayer.Services.PriceFeed.Repositories;
using Xunit;

namespace NeoServiceLayer.Tests.Unit
{
    public class PriceFeedServiceTests
    {
        private readonly Mock<ILogger<PriceFeedService>> _loggerMock;
        private readonly Mock<IPriceRepository> _priceRepositoryMock;
        private readonly Mock<IPriceSourceRepository> _sourceRepositoryMock;
        private readonly Mock<IPriceHistoryRepository> _historyRepositoryMock;
        private readonly Mock<IEnclaveService> _enclaveServiceMock;
        private readonly Mock<IWalletService> _walletServiceMock;
        private readonly PriceFeedService _priceFeedService;

        public PriceFeedServiceTests()
        {
            _loggerMock = new Mock<ILogger<PriceFeedService>>();
            _priceRepositoryMock = new Mock<IPriceRepository>();
            _sourceRepositoryMock = new Mock<IPriceSourceRepository>();
            _historyRepositoryMock = new Mock<IPriceHistoryRepository>();
            _enclaveServiceMock = new Mock<IEnclaveService>();
            _walletServiceMock = new Mock<IWalletService>();

            _priceFeedService = new PriceFeedService(
                _loggerMock.Object,
                _priceRepositoryMock.Object,
                _sourceRepositoryMock.Object,
                _historyRepositoryMock.Object,
                _enclaveServiceMock.Object,
                _walletServiceMock.Object);
        }

        [Fact]
        public async Task FetchPricesAsync_WithSources_ReturnsPrices()
        {
            // Arrange
            var baseCurrency = "USD";
            var sources = new List<PriceSource>
            {
                new PriceSource
                {
                    Id = Guid.NewGuid(),
                    Name = "TestSource1",
                    Type = (PriceSourceType)(int)PriceSourceTypeEnum.API,
                    Url = "https://api.test.com/prices",
                    ApiKey = "test-api-key",
                    ApiSecret = "test-api-secret",
                    SupportedAssets = new List<string> { "BTC", "ETH" },
                    Config = new PriceSourceConfig
                    {
                        PriceJsonPath = "$.price",
                        TimestampJsonPath = "$.timestamp",
                        TimestampFormat = "yyyy-MM-ddTHH:mm:ssZ"
                    }
                },
                new PriceSource
                {
                    Id = Guid.NewGuid(),
                    Name = "TestSource2",
                    Type = (PriceSourceType)(int)PriceSourceTypeEnum.API,
                    Url = "https://api.test2.com/prices",
                    ApiKey = "test-api-key-2",
                    ApiSecret = "test-api-secret-2",
                    SupportedAssets = new List<string> { "BTC", "ETH", "NEO" },
                    Config = new PriceSourceConfig
                    {
                        PriceJsonPath = "$.price",
                        TimestampJsonPath = "$.timestamp",
                        TimestampFormat = "yyyy-MM-ddTHH:mm:ssZ"
                    }
                }
            };

            var prices = new List<Price>
            {
                new Price
                {
                    Id = Guid.NewGuid(),
                    Symbol = "BTC",
                    BaseCurrency = baseCurrency,
                    Value = 50000.0m,
                    Timestamp = DateTime.UtcNow,
                    Source = sources[0].Name,
                    SourcePrices = new List<SourcePrice> { new SourcePrice { SourceId = sources[0].Id, SourceName = sources[0].Name } },
                    ConfidenceScore = 100,
                    CreatedAt = DateTime.UtcNow
                },
                new Price
                {
                    Id = Guid.NewGuid(),
                    Symbol = "ETH",
                    BaseCurrency = baseCurrency,
                    Value = 3000.0m,
                    Timestamp = DateTime.UtcNow,
                    Source = sources[0].Name,
                    SourcePrices = new List<SourcePrice> { new SourcePrice { SourceId = sources[0].Id, SourceName = sources[0].Name } },
                    ConfidenceScore = 100,
                    CreatedAt = DateTime.UtcNow
                },
                new Price
                {
                    Id = Guid.NewGuid(),
                    Symbol = "NEO",
                    BaseCurrency = baseCurrency,
                    Value = 50.0m,
                    Timestamp = DateTime.UtcNow,
                    Source = sources[1].Name,
                    SourcePrices = new List<SourcePrice> { new SourcePrice { SourceId = sources[1].Id, SourceName = sources[1].Name } },
                    ConfidenceScore = 100,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _sourceRepositoryMock
                .Setup(x => x.GetActiveSourcesAsync())
                .ReturnsAsync(sources);

            _enclaveServiceMock
                .Setup(x => x.SendRequestAsync<object, List<Price>>(
                    It.Is<string>(s => s == Core.Constants.EnclaveServiceTypes.PriceFeed),
                    It.Is<string>(s => s == Core.Constants.PriceFeedOperations.FetchPrices),
                    It.IsAny<object>()))
                .ReturnsAsync(prices);

            _priceRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<Price>()))
                .ReturnsAsync((Price p) => p);

            // Act
            var result = await _priceFeedService.FetchPricesAsync(baseCurrency);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
            Assert.Contains(result, p => p.Symbol == "BTC");
            Assert.Contains(result, p => p.Symbol == "ETH");
            Assert.Contains(result, p => p.Symbol == "NEO");
            Assert.All(result, p => Assert.Equal(baseCurrency, p.BaseCurrency));

            _sourceRepositoryMock.Verify(x => x.GetActiveSourcesAsync(), Times.Once);
            _enclaveServiceMock.Verify(
                x => x.SendRequestAsync<object, List<Price>>(
                    It.Is<string>(s => s == Core.Constants.EnclaveServiceTypes.PriceFeed),
                    It.Is<string>(s => s == Core.Constants.PriceFeedOperations.FetchPrices),
                    It.IsAny<object>()),
                Times.Once);
            _priceRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Price>()), Times.Exactly(3));
        }

        [Fact]
        public async Task GetLatestPriceAsync_ExistingPrice_ReturnsPrice()
        {
            // Arrange
            var symbol = "BTC";
            var baseCurrency = "USD";
            var price = new Price
            {
                Id = Guid.NewGuid(),
                Symbol = symbol,
                BaseCurrency = baseCurrency,
                Value = 50000.0m,
                Timestamp = DateTime.UtcNow,
                Source = "TestSource",
                SourcePrices = new List<SourcePrice> { new SourcePrice { SourceId = Guid.NewGuid(), SourceName = "TestSource" } },
                ConfidenceScore = 100,
                CreatedAt = DateTime.UtcNow
            };

            _priceRepositoryMock
                .Setup(x => x.GetLatestPriceAsync(symbol, baseCurrency))
                .ReturnsAsync(price);

            // Act
            var result = await _priceFeedService.GetLatestPriceAsync(symbol, baseCurrency);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(symbol, result.Symbol);
            Assert.Equal(baseCurrency, result.BaseCurrency);
            Assert.Equal(price.Value, result.Value);

            _priceRepositoryMock.Verify(x => x.GetLatestPriceAsync(symbol, baseCurrency), Times.Once);
        }

        [Fact]
        public async Task GetLatestPriceAsync_NonExistingPrice_FetchesAndReturnsPrice()
        {
            // Arrange
            var symbol = "BTC";
            var baseCurrency = "USD";
            var price = new Price
            {
                Id = Guid.NewGuid(),
                Symbol = symbol,
                BaseCurrency = baseCurrency,
                Value = 50000.0m,
                Timestamp = DateTime.UtcNow,
                Source = "TestSource",
                SourcePrices = new List<SourcePrice> { new SourcePrice { SourceId = Guid.NewGuid(), SourceName = "TestSource" } },
                ConfidenceScore = 100,
                CreatedAt = DateTime.UtcNow
            };

            _priceRepositoryMock
                .Setup(x => x.GetLatestPriceAsync(symbol, baseCurrency))
                .ReturnsAsync((Price?)null);

            _enclaveServiceMock
                .Setup(x => x.SendRequestAsync<object, List<Price>>(
                    It.Is<string>(s => s == Core.Constants.EnclaveServiceTypes.PriceFeed),
                    It.Is<string>(s => s == Core.Constants.PriceFeedOperations.FetchPriceForSymbol),
                    It.IsAny<object>()))
                .ReturnsAsync(new List<Price> { price });

            // Mock the source repository to return sources
            _sourceRepositoryMock
                .Setup(x => x.GetByAssetAsync(symbol))
                .ReturnsAsync(new List<PriceSource> { new PriceSource { Status = PriceSourceStatus.Active } });

            _priceRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<Price>()))
                .ReturnsAsync((Price p) => p);

            // Act
            var result = await _priceFeedService.GetLatestPriceAsync(symbol, baseCurrency);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(symbol, result.Symbol);
            Assert.Equal(baseCurrency, result.BaseCurrency);
            Assert.Equal(price.Value, result.Value);

            _priceRepositoryMock.Verify(x => x.GetLatestPriceAsync(symbol, baseCurrency), Times.Once);
            _enclaveServiceMock.Verify(
                x => x.SendRequestAsync<object, List<Price>>(
                    It.Is<string>(s => s == Core.Constants.EnclaveServiceTypes.PriceFeed),
                    It.Is<string>(s => s == Core.Constants.PriceFeedOperations.FetchPriceForSymbol),
                    It.IsAny<object>()),
                Times.Once);
            _priceRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Price>()), Times.Once);
        }

        [Fact]
        public async Task GetAllLatestPricesAsync_WithPrices_ReturnsPrices()
        {
            // Arrange
            var baseCurrency = "USD";
            var prices = new Dictionary<string, Price>
            {
                {
                    "BTC", new Price
                    {
                        Id = Guid.NewGuid(),
                        Symbol = "BTC",
                        BaseCurrency = baseCurrency,
                        Value = 50000.0m,
                        Timestamp = DateTime.UtcNow,
                        Source = "TestSource",
                        SourcePrices = new List<SourcePrice> { new SourcePrice { SourceId = Guid.NewGuid(), SourceName = "TestSource" } },
                        ConfidenceScore = 100,
                        CreatedAt = DateTime.UtcNow
                    }
                },
                {
                    "ETH", new Price
                    {
                        Id = Guid.NewGuid(),
                        Symbol = "ETH",
                        BaseCurrency = baseCurrency,
                        Value = 3000.0m,
                        Timestamp = DateTime.UtcNow,
                        Source = "TestSource",
                        SourcePrices = new List<SourcePrice> { new SourcePrice { SourceId = Guid.NewGuid(), SourceName = "TestSource" } },
                        ConfidenceScore = 100,
                        CreatedAt = DateTime.UtcNow
                    }
                }
            };

            _priceRepositoryMock
                .Setup(x => x.GetAllLatestPricesAsync(baseCurrency))
                .ReturnsAsync(prices);

            // Act
            var result = await _priceFeedService.GetAllLatestPricesAsync(baseCurrency);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.True(result.ContainsKey("BTC"));
            Assert.True(result.ContainsKey("ETH"));
            Assert.Equal(50000.0m, result["BTC"].Value);
            Assert.Equal(3000.0m, result["ETH"].Value);

            _priceRepositoryMock.Verify(x => x.GetAllLatestPricesAsync(baseCurrency), Times.Once);
        }

        [Fact]
        public async Task AddSourceAsync_ValidSource_ReturnsCreatedSource()
        {
            // Arrange
            var source = new PriceSource
            {
                Name = "TestSource",
                Type = (PriceSourceType)(int)PriceSourceTypeEnum.API,
                Url = "https://api.test.com/prices",
                ApiKey = "test-api-key",
                ApiSecret = "test-api-secret",
                SupportedAssets = new List<string> { "BTC", "ETH" },
                Config = new PriceSourceConfig
                {
                    PriceJsonPath = "$.price",
                    TimestampJsonPath = "$.timestamp",
                    TimestampFormat = "yyyy-MM-ddTHH:mm:ssZ"
                }
            };

            _sourceRepositoryMock
                .Setup(x => x.GetByNameAsync(source.Name))
                .ReturnsAsync((PriceSource?)null);

            _sourceRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<PriceSource>()))
                .ReturnsAsync((PriceSource s) => s);

            _enclaveServiceMock
                .Setup(x => x.SendRequestAsync<object, object>(
                    It.Is<string>(s => s == Core.Constants.EnclaveServiceTypes.PriceFeed),
                    It.Is<string>(s => s == Core.Constants.PriceFeedOperations.ValidateSource),
                    It.IsAny<object>()))
                .ReturnsAsync(new { Valid = true });

            // Act
            // Mock the repository to return the source when CreateAsync is called
            _sourceRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<PriceSource>()))
                .ReturnsAsync((PriceSource s) =>
                {
                    s.Id = Guid.NewGuid();
                    return s;
                });

            // Act
            var result = await _priceFeedService.AddSourceAsync(source);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(source.Name, result.Name);
            Assert.Equal(source.Type, result.Type);
            Assert.Equal(source.Url, result.Url);
            Assert.Equal(source.ApiKey, result.ApiKey);
            Assert.Equal(source.ApiSecret, result.ApiSecret);
            Assert.Equal(source.SupportedAssets, result.SupportedAssets);
            Assert.Equal(source.Config, result.Config);

            _sourceRepositoryMock.Verify(x => x.GetByNameAsync(source.Name), Times.Once);
            _sourceRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<PriceSource>()), Times.Once);
            _enclaveServiceMock.Verify(
                x => x.SendRequestAsync<object, object>(
                    It.Is<string>(s => s == Core.Constants.EnclaveServiceTypes.PriceFeed),
                    It.Is<string>(s => s == Core.Constants.PriceFeedOperations.ValidateSource),
                    It.IsAny<object>()),
                Times.Once);
        }
    }
}
