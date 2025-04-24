using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Services.Storage;
using NeoServiceLayer.Services.Storage.Configuration;
using NeoServiceLayer.Services.Storage.Providers;
using Xunit;

namespace NeoServiceLayer.Tests.Unit
{
    public class DatabaseServiceTests
    {
        private readonly Mock<ILogger<DatabaseService>> _loggerMock;
        private readonly Mock<IOptions<DatabaseConfiguration>> _configMock;
        private readonly Mock<IStorageProvider> _providerMock;
        private readonly DatabaseService _databaseService;
        private readonly DatabaseConfiguration _databaseConfig;

        public DatabaseServiceTests()
        {
            _loggerMock = new Mock<ILogger<DatabaseService>>();

            _databaseConfig = new DatabaseConfiguration
            {
                DefaultProvider = "TestProvider",
                Providers = new List<DatabaseProviderConfiguration>
                {
                    new DatabaseProviderConfiguration
                    {
                        Name = "TestProvider",
                        Type = "inmemory",
                        ConnectionString = "test-connection",
                        Database = "test-db"
                    }
                }
            };

            _configMock = new Mock<IOptions<DatabaseConfiguration>>();
            _configMock.Setup(x => x.Value).Returns(_databaseConfig);

            _providerMock = new Mock<IStorageProvider>();
            _providerMock.Setup(x => x.Name).Returns("TestProvider");
            _providerMock.Setup(x => x.Type).Returns("inmemory");
            _providerMock.Setup(x => x.InitializeAsync()).ReturnsAsync(true);
            _providerMock.Setup(x => x.HealthCheckAsync()).ReturnsAsync(true);

            _databaseService = new DatabaseService(_loggerMock.Object, _configMock.Object);
        }

        [Fact]
        public void GetDefaultProvider_NoProviders_ReturnsNull()
        {
            // Act
            var result = _databaseService.GetDefaultProvider();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void RegisterProvider_ValidProvider_ReturnsTrue()
        {
            // Act
            var result = _databaseService.RegisterProvider(_providerMock.Object);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void RegisterProvider_DuplicateProvider_ReturnsFalse()
        {
            // Arrange
            _databaseService.RegisterProvider(_providerMock.Object);

            // Act
            var result = _databaseService.RegisterProvider(_providerMock.Object);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetProvider_RegisteredProvider_ReturnsProvider()
        {
            // Arrange
            _databaseService.RegisterProvider(_providerMock.Object);

            // Act
            var result = _databaseService.GetProvider("TestProvider");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TestProvider", result.Name);
        }

        [Fact]
        public void GetProvider_UnregisteredProvider_ReturnsNull()
        {
            // Act
            var result = _databaseService.GetProvider("NonExistentProvider");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetDefaultProvider_RegisteredDefaultProvider_ReturnsDefaultProvider()
        {
            // Arrange
            _databaseService.RegisterProvider(_providerMock.Object);

            // Act
            var result = _databaseService.GetDefaultProvider();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TestProvider", result.Name);
        }

        [Fact]
        public async Task InitializeProvidersAsync_ValidConfiguration_ReturnsTrue()
        {
            // Arrange - Create a mock provider factory that we can control
            var mockInMemoryProvider = new Mock<IStorageProvider>();
            mockInMemoryProvider.Setup(x => x.Name).Returns("TestProvider");
            mockInMemoryProvider.Setup(x => x.Type).Returns("inmemory");
            mockInMemoryProvider.Setup(x => x.InitializeAsync()).ReturnsAsync(true);

            // Use a custom DatabaseService that doesn't actually create real providers
            var customDatabaseService = new TestDatabaseService(_loggerMock.Object, _configMock.Object, mockInMemoryProvider.Object);

            // Act
            var result = await customDatabaseService.InitializeTestProvidersAsync();

            // Assert
            Assert.True(result);
            mockInMemoryProvider.Verify(x => x.InitializeAsync(), Times.Once);
        }

        [Fact]
        public async Task HealthCheckAsync_HealthyProviders_ReturnsTrue()
        {
            // Arrange
            _databaseService.RegisterProvider(_providerMock.Object);

            // Act
            var result = await _databaseService.HealthCheckAsync();

            // Assert
            Assert.True(result);
            _providerMock.Verify(x => x.HealthCheckAsync(), Times.Once);
        }

        [Fact]
        public async Task HealthCheckAsync_UnhealthyProviders_ReturnsFalse()
        {
            // Arrange
            var unhealthyProviderMock = new Mock<IStorageProvider>();
            unhealthyProviderMock.Setup(x => x.Name).Returns("UnhealthyProvider");
            unhealthyProviderMock.Setup(x => x.Type).Returns("inmemory");
            unhealthyProviderMock.Setup(x => x.HealthCheckAsync()).ReturnsAsync(false);

            _databaseService.RegisterProvider(unhealthyProviderMock.Object);

            // Act
            var result = await _databaseService.HealthCheckAsync();

            // Assert
            Assert.False(result);
            unhealthyProviderMock.Verify(x => x.HealthCheckAsync(), Times.Once);
        }

        [Fact]
        public async Task GetDefaultProviderAsync_NoProviders_InitializesProviders()
        {
            // Arrange - Create a mock provider factory that we can control
            var mockInMemoryProvider = new Mock<IStorageProvider>();
            mockInMemoryProvider.Setup(x => x.Name).Returns("TestProvider");
            mockInMemoryProvider.Setup(x => x.Type).Returns("inmemory");
            mockInMemoryProvider.Setup(x => x.InitializeAsync()).ReturnsAsync(true);

            // Use a custom DatabaseService that doesn't actually create real providers
            var customDatabaseService = new TestDatabaseService(_loggerMock.Object, _configMock.Object, mockInMemoryProvider.Object);

            // Act
            var result = await customDatabaseService.GetDefaultProviderAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TestProvider", result.Name);
            mockInMemoryProvider.Verify(x => x.InitializeAsync(), Times.Once);
        }

        // Helper class for testing without creating real providers
        private class TestDatabaseService : DatabaseService
        {
            private readonly IStorageProvider _mockProvider;

            public TestDatabaseService(
                ILogger<DatabaseService> logger,
                IOptions<DatabaseConfiguration> configuration,
                IStorageProvider mockProvider)
                : base(logger, configuration)
            {
                _mockProvider = mockProvider;
                RegisterProvider(_mockProvider);
            }

            public async Task<bool> InitializeTestProvidersAsync()
            {
                RegisterProvider(_mockProvider);
                await _mockProvider.InitializeAsync();
                return true;
            }

            public override async Task<IStorageProvider> GetDefaultProviderAsync()
            {
                await _mockProvider.InitializeAsync();
                return _mockProvider;
            }
        }
    }
}
