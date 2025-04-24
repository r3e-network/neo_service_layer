using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NeoServiceLayer.Enclave.Enclave.Execution;
using Xunit;
using CoreEvent = NeoServiceLayer.Core.Models.Event;
using EnclaveEvent = NeoServiceLayer.Enclave.Enclave.Models.Event;

namespace NeoServiceLayer.Tests.Unit
{
    public class JavaScriptFunctionExecutionTests
    {
        private readonly Mock<ILogger<NodeJsRuntime>> _loggerMock;
        private readonly NodeJsRuntime _nodeJsRuntime;

        public JavaScriptFunctionExecutionTests()
        {
            _loggerMock = new Mock<ILogger<NodeJsRuntime>>();

            // Create mocks for the required services
            // We need to use MockBehavior.Loose to avoid issues with non-parameterless constructors
            var priceFeedLoggerMock = new Mock<ILogger<Enclave.Enclave.Services.EnclavePriceFeedService>>();
            var secretsLoggerMock = new Mock<ILogger<Enclave.Enclave.Services.EnclaveSecretsService>>();
            var walletLoggerMock = new Mock<ILogger<Enclave.Enclave.Services.EnclaveWalletService>>();
            var functionLoggerMock = new Mock<ILogger<Enclave.Enclave.Services.EnclaveFunctionService>>();
            // We can't directly mock FunctionExecutor because it doesn't have a parameterless constructor
            // Instead, we'll create a mock for ILogger<FunctionExecutor> which is needed for the constructor
            var functionExecutorLoggerMock = new Mock<ILogger<Enclave.Enclave.Execution.FunctionExecutor>>();

            // Create loggers for the runtime dependencies that FunctionExecutor needs
            var dotNetRuntimeLoggerMock = new Mock<ILogger<Enclave.Enclave.Execution.DotNetRuntime>>();
            var pythonRuntimeLoggerMock = new Mock<ILogger<Enclave.Enclave.Execution.PythonRuntime>>();

            // Create actual instances of the runtime classes
            var dotNetRuntime = new Enclave.Enclave.Execution.DotNetRuntime(dotNetRuntimeLoggerMock.Object);
            var pythonRuntime = new Enclave.Enclave.Execution.PythonRuntime(pythonRuntimeLoggerMock.Object);

            // Create the service instances with their required dependencies
            var walletService = new Enclave.Enclave.Services.EnclaveWalletService(walletLoggerMock.Object);
            var priceFeedService = new Enclave.Enclave.Services.EnclavePriceFeedService(priceFeedLoggerMock.Object, walletService);
            var secretsService = new Enclave.Enclave.Services.EnclaveSecretsService(secretsLoggerMock.Object);

            // Create a mock FunctionExecutor first
            var mockFunctionExecutor = new Mock<Enclave.Enclave.Execution.FunctionExecutor>(
                functionExecutorLoggerMock.Object,
                null,
                dotNetRuntime,
                pythonRuntime);

            // Create a mock function service
            var functionService = new Enclave.Enclave.Services.EnclaveFunctionService(
                functionLoggerMock.Object,
                mockFunctionExecutor.Object);

            // Now create the NodeJsRuntime with all dependencies
            _nodeJsRuntime = new NodeJsRuntime(
                _loggerMock.Object,
                priceFeedService,
                secretsService,
                walletService,
                functionService);

            // No need to create another function service

            // We're using a mock FunctionExecutor, so we don't need to update its _runtimes dictionary
        }

        [Fact]
        public async Task CompileAsync_ValidJavaScript_ReturnsCompiledFunction()
        {
            // Arrange
            var sourceCode = @"
                function add(params) {
                    return params.a + params.b;
                }
            ";
            var entryPoint = "add";

            // Act
            var result = await _nodeJsRuntime.CompileAsync(sourceCode, entryPoint);

            // Assert
            Assert.NotNull(result);
            var compiledFunction = Assert.IsType<NeoServiceLayer.Enclave.Enclave.Execution.CompiledJsFunction>(result);
            Assert.Equal(sourceCode, compiledFunction.SourceCode);
            Assert.Equal(entryPoint, compiledFunction.EntryPoint);
        }

        [Fact]
        public async Task CompileAsync_InvalidJavaScript_ThrowsException()
        {
            // Arrange
            var sourceCode = @"
                function add(params {  // Missing closing parenthesis
                    return params.a + params.b;
                }
            ";
            var entryPoint = "add";

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _nodeJsRuntime.CompileAsync(sourceCode, entryPoint));
        }

        [Fact]
        public async Task CompileAsync_MissingEntryPoint_ThrowsException()
        {
            // Arrange
            var sourceCode = @"
                function add(params) {
                    return params.a + params.b;
                }
            ";
            var entryPoint = "nonExistentFunction";

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _nodeJsRuntime.CompileAsync(sourceCode, entryPoint));
        }

        [Fact]
        public async Task ExecuteAsync_SimpleAddFunction_ReturnsCorrectResult()
        {
            // Arrange
            var sourceCode = @"
                function add(params) {
                    return params.a + params.b;
                }
            ";
            var entryPoint = "add";
            var parameters = new Dictionary<string, object>
            {
                { "a", 5 },
                { "b", 7 }
            };

            var compiledFunction = await _nodeJsRuntime.CompileAsync(sourceCode, entryPoint);
            var context = new FunctionExecutionContext
            {
                FunctionId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                MaxExecutionTime = 5000,
                MaxMemory = 128
            };

            // Act
            var result = await _nodeJsRuntime.ExecuteAsync(compiledFunction, entryPoint, parameters, context);

            // Assert
            Assert.NotNull(result);
            var resultObj = result.GetType().GetProperty("Result")?.GetValue(result);
            Assert.Equal(12.0, resultObj);
        }

        [Fact]
        public async Task ExecuteAsync_WithConsoleLog_CapturesLogs()
        {
            // Arrange
            var sourceCode = @"
                function logMessage(params) {
                    console.log('Processing request with id: ' + params.id);
                    console.log('Parameters: ' + JSON.stringify(params));
                    return 'Logged message with id: ' + params.id;
                }
            ";
            var entryPoint = "logMessage";
            var parameters = new Dictionary<string, object>
            {
                { "id", "test-123" },
                { "value", "test-value" }
            };

            var compiledFunction = await _nodeJsRuntime.CompileAsync(sourceCode, entryPoint);
            var context = new FunctionExecutionContext
            {
                FunctionId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                MaxExecutionTime = 5000,
                MaxMemory = 128
            };

            // Act
            var result = await _nodeJsRuntime.ExecuteAsync(compiledFunction, entryPoint, parameters, context);

            // Assert
            Assert.NotNull(result);
            var resultObj = result.GetType().GetProperty("Result")?.GetValue(result);
            Assert.Equal("Logged message with id: test-123", resultObj);

            var logs = result.GetType().GetProperty("Logs")?.GetValue(result) as List<string>;
            Assert.NotNull(logs);
            Assert.Equal(2, logs.Count);
            Assert.Contains("Processing request with id: test-123", logs[0]);
        }

        [Fact]
        public async Task ExecuteAsync_WithEnvironmentVariables_CanAccessEnvVars()
        {
            // Arrange
            var sourceCode = @"
                function getEnvVar(params) {
                    return {
                        requestedVar: params.varName,
                        value: env[params.varName]
                    };
                }
            ";
            var entryPoint = "getEnvVar";
            var parameters = new Dictionary<string, object>
            {
                { "varName", "API_KEY" }
            };

            var compiledFunction = await _nodeJsRuntime.CompileAsync(sourceCode, entryPoint);
            var context = new FunctionExecutionContext
            {
                FunctionId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                MaxExecutionTime = 5000,
                MaxMemory = 128,
                EnvironmentVariables = new Dictionary<string, string>
                {
                    { "API_KEY", "secret-api-key-123" },
                    { "API_URL", "https://api.example.com" }
                }
            };

            // Act
            var result = await _nodeJsRuntime.ExecuteAsync(compiledFunction, entryPoint, parameters, context);

            // Assert
            Assert.NotNull(result);
            var resultObj = result.GetType().GetProperty("Result")?.GetValue(result) as Dictionary<string, object>;
            Assert.NotNull(resultObj);
            Assert.Equal("API_KEY", resultObj["requestedVar"]);
            Assert.Equal("secret-api-key-123", resultObj["value"]);
        }

        [Fact]
        public async Task ExecuteAsync_ComplexObject_HandlesNestedObjects()
        {
            // Arrange
            var sourceCode = @"
                function processObject(params) {
                    let result = {
                        id: params.user.id,
                        fullName: params.user.firstName + ' ' + params.user.lastName,
                        items: params.user.items.map(item => item.name)
                    };
                    return result;
                }
            ";
            var entryPoint = "processObject";
            var parameters = new Dictionary<string, object>
            {
                {
                    "user", new Dictionary<string, object>
                    {
                        { "id", 123 },
                        { "firstName", "John" },
                        { "lastName", "Doe" },
                        {
                            "items", new List<Dictionary<string, object>>
                            {
                                new Dictionary<string, object> { { "id", 1 }, { "name", "Item 1" } },
                                new Dictionary<string, object> { { "id", 2 }, { "name", "Item 2" } }
                            }
                        }
                    }
                }
            };

            var compiledFunction = await _nodeJsRuntime.CompileAsync(sourceCode, entryPoint);
            var context = new FunctionExecutionContext
            {
                FunctionId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                MaxExecutionTime = 5000,
                MaxMemory = 128
            };

            // Act
            var result = await _nodeJsRuntime.ExecuteAsync(compiledFunction, entryPoint, parameters, context);

            // Assert
            Assert.NotNull(result);
            var resultObj = result.GetType().GetProperty("Result")?.GetValue(result) as Dictionary<string, object>;
            Assert.NotNull(resultObj);
            Assert.Equal(123.0, resultObj["id"]);
            Assert.Equal("John Doe", resultObj["fullName"]);

            var items = resultObj["items"] as object[];
            Assert.NotNull(items);
            Assert.Equal(2, items.Length);
            Assert.Equal("Item 1", items[0]);
            Assert.Equal("Item 2", items[1]);
        }

        [Fact]
        public async Task ExecuteForEventAsync_ProcessesEventData()
        {
            // Arrange
            var sourceCode = @"
                function processEvent(event) {
                    return {
                        eventType: event.type,
                        eventName: event.name,
                        processedAt: new Date().toISOString(),
                        data: event.data
                    };
                }
            ";
            var entryPoint = "processEvent";

            var enclaveEventData = new EnclaveEvent
            {
                Id = Guid.NewGuid(),
                Type = "blockchain.transaction",
                Name = "TransactionConfirmed",
                Source = "neo-node",
                Data = new Dictionary<string, object>
                {
                    { "txId", "0x1234567890abcdef" },
                    { "blockHeight", 12345 },
                    { "confirmations", 6 }
                },
                Timestamp = DateTime.UtcNow
            };

            var coreEventData = new CoreEvent
            {
                Id = enclaveEventData.Id,
                Type = enclaveEventData.Type,
                Name = enclaveEventData.Name,
                Source = enclaveEventData.Source,
                Data = enclaveEventData.Data,
                Timestamp = enclaveEventData.Timestamp
            };

            var compiledFunction = await _nodeJsRuntime.CompileAsync(sourceCode, entryPoint);
            var context = new FunctionExecutionContext
            {
                FunctionId = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                MaxExecutionTime = 5000,
                MaxMemory = 128,
                Event = enclaveEventData
            };

            // Act
            var result = await _nodeJsRuntime.ExecuteForEventAsync(compiledFunction, entryPoint, coreEventData, context);

            // Assert
            Assert.NotNull(result);
            var resultObj = result.GetType().GetProperty("Result")?.GetValue(result) as Dictionary<string, object>;
            Assert.NotNull(resultObj);
            Assert.Equal("blockchain.transaction", resultObj["eventType"]);
            Assert.Equal("TransactionConfirmed", resultObj["eventName"]);

            var data = resultObj["data"] as Dictionary<string, object>;
            Assert.NotNull(data);
            Assert.Equal("0x1234567890abcdef", data["txId"]);
            Assert.Equal(12345.0, data["blockHeight"]);
            Assert.Equal(6.0, data["confirmations"]);
        }
    }

    // This class is no longer needed as we're using the actual CompiledJsFunction class from NodeJsRuntime
}
