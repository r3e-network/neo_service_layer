using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NeoServiceLayer.Core;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Enclave.Enclave;
using NeoServiceLayer.Enclave.Enclave.Services;
using System.Reflection;
using NeoServiceLayer.Tests.Mocks;
using Xunit;

namespace NeoServiceLayer.Tests.Unit
{

    public class VsockServerTests
    {
        private readonly Mock<ILogger<VsockServer>> _loggerMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<EnclaveAccountService> _accountServiceMock;
        private readonly Mock<EnclaveWalletService> _walletServiceMock;
        private readonly Mock<EnclaveSecretsService> _secretsServiceMock;
        private readonly Mock<EnclaveFunctionService> _functionServiceMock;
        private readonly MockFunctionExecutor _mockFunctionExecutor;
        private readonly Mock<EnclavePriceFeedService> _priceFeedServiceMock;

        public VsockServerTests()
        {
            _loggerMock = new Mock<ILogger<VsockServer>>();
            _serviceProviderMock = new Mock<IServiceProvider>();

            _accountServiceMock = new Mock<EnclaveAccountService>(MockBehavior.Loose, new object[] { Mock.Of<ILogger<EnclaveAccountService>>() });
            _walletServiceMock = new Mock<EnclaveWalletService>(MockBehavior.Loose, new object[] { Mock.Of<ILogger<EnclaveWalletService>>() });
            _secretsServiceMock = new Mock<EnclaveSecretsService>(MockBehavior.Loose, new object[] { Mock.Of<ILogger<EnclaveSecretsService>>() });

            // Create a mock function executor
            _mockFunctionExecutor = new MockFunctionExecutor(Mock.Of<ILogger<NeoServiceLayer.Enclave.Enclave.Execution.FunctionExecutor>>());

            // Create the function service with the mock executor
            _functionServiceMock = new Mock<EnclaveFunctionService>(MockBehavior.Loose, new object[] {
                Mock.Of<ILogger<EnclaveFunctionService>>(),
                _mockFunctionExecutor
            });
            _priceFeedServiceMock = new Mock<EnclavePriceFeedService>(MockBehavior.Loose, new object[] {
                Mock.Of<ILogger<EnclavePriceFeedService>>(),
                _walletServiceMock.Object
            });

            // Setup service provider to return our mocked services
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(EnclaveAccountService))).Returns(_accountServiceMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(EnclaveWalletService))).Returns(_walletServiceMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(EnclaveSecretsService))).Returns(_secretsServiceMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(EnclaveFunctionService))).Returns(_functionServiceMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(EnclavePriceFeedService))).Returns(_priceFeedServiceMock.Object);
        }

        [Fact]
        public void GetCpuUsage_ReturnsValidValue()
        {
            // Arrange
            var vsockServer = new VsockServer(_loggerMock.Object, _serviceProviderMock.Object);

            // Use reflection to access the private method
            var method = typeof(VsockServer).GetMethod("GetCpuUsage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var result = (double)method.Invoke(vsockServer, null);

            // Assert
            Assert.True(result >= 0 && result <= 100);
        }

        [Fact]
        public void GetMemoryUsage_ReturnsValidValue()
        {
            // Arrange
            var vsockServer = new VsockServer(_loggerMock.Object, _serviceProviderMock.Object);

            // Use reflection to access the private method
            var method = typeof(VsockServer).GetMethod("GetMemoryUsage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var result = (long)method.Invoke(vsockServer, null);

            // Assert
            Assert.True(result >= 0);
        }

        [Fact]
        public void GetUptime_ReturnsValidValue()
        {
            // Arrange
            var vsockServer = new VsockServer(_loggerMock.Object, _serviceProviderMock.Object);

            // Use reflection to access the private method
            var method = typeof(VsockServer).GetMethod("GetUptime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var result = (TimeSpan)method.Invoke(vsockServer, null);

            // Assert
            Assert.True(result.TotalMilliseconds >= 0);
        }

        [Fact]
        public void GetRequestCount_ReturnsValidValue()
        {
            // Arrange
            var vsockServer = new VsockServer(_loggerMock.Object, _serviceProviderMock.Object);

            // Use reflection to access the private method
            var method = typeof(VsockServer).GetMethod("GetRequestCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var result = (long)method.Invoke(vsockServer, null);

            // Assert
            Assert.True(result >= 0);
        }

        [Fact]
        public void IncrementRequestCount_IncrementsCounter()
        {
            // Arrange
            var vsockServer = new VsockServer(_loggerMock.Object, _serviceProviderMock.Object);

            // Use reflection to access the private methods
            var getMethod = typeof(VsockServer).GetMethod("GetRequestCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var incrementMethod = typeof(VsockServer).GetMethod("IncrementRequestCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var initialCount = (long)getMethod.Invoke(vsockServer, null);
            incrementMethod.Invoke(vsockServer, null);
            var newCount = (long)getMethod.Invoke(vsockServer, null);

            // Assert
            Assert.Equal(initialCount + 1, newCount);
        }

        [Fact]
        public async Task ProcessMessageAsync_PingRequest_ReturnsSuccessResponse()
        {
            // Arrange
            var vsockServer = new MockVsockServer(_loggerMock.Object, _serviceProviderMock.Object);

            var request = new EnclaveRequest
            {
                RequestId = "6ea65a0c-4e6d-4cbe-a776-4ec42efc86e1",
                ServiceType = "ping",
                Operation = "ping",
                Payload = null
            };

            var requestBytes = JsonSerializer.SerializeToUtf8Bytes(request);

            // Act
            var responseBytes = await vsockServer.ProcessMessageAsync(requestBytes);
            var response = JsonSerializer.Deserialize<EnclaveResponse>(responseBytes);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(request.RequestId, response.RequestId);
            Assert.True(response.Success);
            Assert.Null(response.ErrorMessage);

            // Parse the payload
            var payloadObj = JsonSerializer.Deserialize<JsonElement>(response.Payload);
            Assert.Equal("OK", payloadObj.GetProperty("Status").GetString());
        }

        [Fact]
        public async Task ProcessMessageAsync_MetricsRequest_ReturnsSuccessResponse()
        {
            // Arrange
            var vsockServer = new MockVsockServer(_loggerMock.Object, _serviceProviderMock.Object);

            var request = new EnclaveRequest
            {
                RequestId = "15ac7ca9-9b1b-4ad1-8c5c-791b35ae7798",
                ServiceType = "metrics",
                Operation = "get",
                Payload = null
            };

            var requestBytes = JsonSerializer.SerializeToUtf8Bytes(request);

            // Act
            var responseBytes = await vsockServer.ProcessMessageAsync(requestBytes);
            var response = JsonSerializer.Deserialize<EnclaveResponse>(responseBytes);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(request.RequestId, response.RequestId);
            Assert.True(response.Success);
            Assert.Null(response.ErrorMessage);

            // Parse the payload
            var payloadObj = JsonSerializer.Deserialize<JsonElement>(response.Payload);
            Assert.True(payloadObj.TryGetProperty("CpuUsage", out _));
            Assert.True(payloadObj.TryGetProperty("MemoryUsage", out _));
            Assert.True(payloadObj.TryGetProperty("Uptime", out _));
            Assert.True(payloadObj.TryGetProperty("RequestCount", out _));
            Assert.True(payloadObj.TryGetProperty("Timestamp", out _));
        }

        [Fact]
        public async Task ProcessMessageAsync_InvalidRequest_ReturnsErrorResponse()
        {
            // Arrange
            var vsockServer = new MockVsockServer(_loggerMock.Object, _serviceProviderMock.Object);

            var invalidJson = Encoding.UTF8.GetBytes("invalid json");

            // Act
            var responseBytes = await vsockServer.ProcessMessageAsync(invalidJson);
            var response = JsonSerializer.Deserialize<EnclaveResponse>(responseBytes);

            // Assert
            Assert.NotNull(response);
            Assert.Equal("unknown", response.RequestId);
            Assert.False(response.Success);
            Assert.NotNull(response.ErrorMessage);
            Assert.Contains("Error processing message", response.ErrorMessage);
        }
    }
}
