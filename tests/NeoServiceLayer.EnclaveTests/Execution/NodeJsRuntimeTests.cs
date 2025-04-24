using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NeoServiceLayer.Enclave.Enclave.Execution;
using NeoServiceLayer.Enclave.Enclave.Services;
using Xunit;

namespace NeoServiceLayer.EnclaveTests.Execution
{
    public class NodeJsRuntimeTests
    {
        private readonly Mock<ILogger<NodeJsRuntime>> _loggerMock;
        private readonly Mock<EnclavePriceFeedService> _priceFeedServiceMock;
        private readonly Mock<EnclaveSecretsService> _secretsServiceMock;
        private readonly Mock<EnclaveWalletService> _walletServiceMock;
        private readonly Mock<EnclaveFunctionService> _functionServiceMock;
        private readonly NodeJsRuntime _nodeJsRuntime;

        public NodeJsRuntimeTests()
        {
            _loggerMock = new Mock<ILogger<NodeJsRuntime>>();
            _priceFeedServiceMock = new Mock<EnclavePriceFeedService>();
            _secretsServiceMock = new Mock<EnclaveSecretsService>();
            _walletServiceMock = new Mock<EnclaveWalletService>();
            _functionServiceMock = new Mock<EnclaveFunctionService>();

            _nodeJsRuntime = new NodeJsRuntime(
                _loggerMock.Object,
                _priceFeedServiceMock.Object,
                _secretsServiceMock.Object,
                _walletServiceMock.Object,
                _functionServiceMock.Object);
        }

        [Fact]
        public async Task CompileAsync_ValidJavaScript_ReturnsCompiledFunction()
        {
            // Arrange
            var sourceCode = @"
                function testFunction(params) {
                    return { message: 'Hello, ' + params.name };
                }
            ";
            var entryPoint = "testFunction";

            // Act
            var result = await _nodeJsRuntime.CompileAsync(sourceCode, entryPoint);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CompiledJsFunction>(result);
            var compiledFunction = (CompiledJsFunction)result;
            Assert.Equal(sourceCode, compiledFunction.SourceCode);
            Assert.Equal(entryPoint, compiledFunction.EntryPoint);
        }

        [Fact]
        public async Task CompileAsync_InvalidJavaScript_ThrowsException()
        {
            // Arrange
            var sourceCode = @"
                function testFunction(params {
                    return { message: 'Hello, ' + params.name };
                }
            ";
            var entryPoint = "testFunction";

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _nodeJsRuntime.CompileAsync(sourceCode, entryPoint));
        }

        [Fact]
        public async Task CompileAsync_EntryPointNotFound_ThrowsException()
        {
            // Arrange
            var sourceCode = @"
                function testFunction(params) {
                    return { message: 'Hello, ' + params.name };
                }
            ";
            var entryPoint = "nonExistentFunction";

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _nodeJsRuntime.CompileAsync(sourceCode, entryPoint));
        }

        [Fact]
        public async Task ExecuteAsync_ValidFunction_ReturnsResult()
        {
            // Arrange
            var sourceCode = @"
                function testFunction(params) {
                    return { message: 'Hello, ' + params.name };
                }
            ";
            var entryPoint = "testFunction";
            var parameters = new Dictionary<string, object>
            {
                { "name", "World" }
            };
            var context = new FunctionExecutionContext
            {
                FunctionId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                MaxExecutionTime = 5000,
                MaxMemory = 256
            };
            var compiledFunction = new CompiledJsFunction
            {
                SourceCode = sourceCode,
                EntryPoint = entryPoint
            };

            // Act
            var result = await _nodeJsRuntime.ExecuteAsync(compiledFunction, entryPoint, parameters, context);

            // Assert
            Assert.NotNull(result);
            var resultDict = Assert.IsType<Dictionary<string, object>>(result);
            Assert.True(resultDict.ContainsKey("Result"));
            var resultObj = Assert.IsType<Dictionary<string, object>>(resultDict["Result"]);
            Assert.Equal("Hello, World", resultObj["message"]);
        }

        [Fact]
        public async Task ExecuteAsync_WithConsoleLog_LogsMessages()
        {
            // Arrange
            var sourceCode = @"
                function testFunction(params) {
                    console.log('This is a log message');
                    console.error('This is an error message');
                    return { success: true };
                }
            ";
            var entryPoint = "testFunction";
            var parameters = new Dictionary<string, object>();
            var context = new FunctionExecutionContext
            {
                FunctionId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                MaxExecutionTime = 5000,
                MaxMemory = 256
            };
            var compiledFunction = new CompiledJsFunction
            {
                SourceCode = sourceCode,
                EntryPoint = entryPoint
            };

            // Act
            var result = await _nodeJsRuntime.ExecuteAsync(compiledFunction, entryPoint, parameters, context);

            // Assert
            Assert.NotNull(result);
            var resultDict = Assert.IsType<Dictionary<string, object>>(result);
            Assert.True(resultDict.ContainsKey("Logs"));
            var logs = Assert.IsType<List<string>>(resultDict["Logs"]);
            Assert.Equal(2, logs.Count);
            Assert.Contains("This is a log message", logs[0]);
            Assert.Contains("ERROR: This is an error message", logs[1]);
        }

        [Fact]
        public async Task ExecuteAsync_WithEnvironmentVariables_AccessesVariables()
        {
            // Arrange
            var sourceCode = @"
                function testFunction(params) {
                    return { 
                        apiKey: env.API_KEY,
                        environment: env.ENVIRONMENT
                    };
                }
            ";
            var entryPoint = "testFunction";
            var parameters = new Dictionary<string, object>();
            var context = new FunctionExecutionContext
            {
                FunctionId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                MaxExecutionTime = 5000,
                MaxMemory = 256,
                EnvironmentVariables = new Dictionary<string, string>
                {
                    { "API_KEY", "test-api-key" },
                    { "ENVIRONMENT", "test" }
                }
            };
            var compiledFunction = new CompiledJsFunction
            {
                SourceCode = sourceCode,
                EntryPoint = entryPoint
            };

            // Act
            var result = await _nodeJsRuntime.ExecuteAsync(compiledFunction, entryPoint, parameters, context);

            // Assert
            Assert.NotNull(result);
            var resultDict = Assert.IsType<Dictionary<string, object>>(result);
            Assert.True(resultDict.ContainsKey("Result"));
            var resultObj = Assert.IsType<Dictionary<string, object>>(resultDict["Result"]);
            Assert.Equal("test-api-key", resultObj["apiKey"]);
            Assert.Equal("test", resultObj["environment"]);
        }
    }
}
