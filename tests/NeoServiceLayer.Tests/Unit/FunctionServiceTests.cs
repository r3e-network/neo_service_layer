using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NeoServiceLayer.Core;
using NeoServiceLayer.Core.Enums;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Services.Function;
// Use Core interfaces instead of Service repositories
// using NeoServiceLayer.Services.Function.Repositories;
using Xunit;

namespace NeoServiceLayer.Tests.Unit
{
    public class FunctionServiceTests
    {
        private readonly Mock<ILogger<FunctionService>> _loggerMock;
        private readonly Mock<Core.Interfaces.IFunctionRepository> _functionRepositoryMock;
        private readonly Mock<Core.Interfaces.IFunctionExecutionRepository> _executionRepositoryMock;
        private readonly Mock<Core.Interfaces.IFunctionLogRepository> _logRepositoryMock;
        private readonly Mock<IEnclaveService> _enclaveServiceMock;
        private readonly Mock<ISecretsService> _secretsServiceMock;
        private readonly FunctionService _functionService;

        public FunctionServiceTests()
        {
            _loggerMock = new Mock<ILogger<FunctionService>>();
            _functionRepositoryMock = new Mock<Core.Interfaces.IFunctionRepository>();
            _executionRepositoryMock = new Mock<Core.Interfaces.IFunctionExecutionRepository>();
            _logRepositoryMock = new Mock<Core.Interfaces.IFunctionLogRepository>();
            _enclaveServiceMock = new Mock<IEnclaveService>();
            _secretsServiceMock = new Mock<ISecretsService>();

            _functionService = new FunctionService(
                _loggerMock.Object,
                _functionRepositoryMock.Object,
                _executionRepositoryMock.Object,
                _enclaveServiceMock.Object,
                _secretsServiceMock.Object);
        }

        [Fact]
        public async Task CreateFunctionAsync_ValidInput_ReturnsCreatedFunction()
        {
            // Arrange
            var name = "TestFunction";
            var description = "Test function description";
            var runtime = FunctionRuntime.JavaScript;
            var sourceCode = "function main() { return 'Hello, World!'; }";
            var entryPoint = "main";
            var accountId = Guid.NewGuid();
            var maxExecutionTime = 30000;
            var maxMemory = 128;

            _enclaveServiceMock
                .Setup(x => x.SendRequestAsync<object, object>(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .ReturnsAsync(new object());

            _functionRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<Function>()))
                .ReturnsAsync((Function f) => f);

            // Act
            var result = await _functionService.CreateFunctionAsync(
                name,
                description,
                runtime,
                sourceCode,
                entryPoint,
                accountId,
                maxExecutionTime,
                maxMemory);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(name, result.Name);
            Assert.Equal(description, result.Description);
            Assert.Equal(runtime.ToString(), result.Runtime);
            Assert.Equal(sourceCode, result.SourceCode);
            Assert.Equal(entryPoint, result.EntryPoint);
            Assert.Equal(accountId, result.AccountId);
            Assert.Equal(maxExecutionTime, result.MaxExecutionTime);
            Assert.Equal(maxMemory, result.MaxMemory);
            Assert.Equal("Active", result.Status);

            _functionRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Function>()), Times.Once);
            _enclaveServiceMock.Verify(
                x => x.SendRequestAsync<object, object>(
                    It.Is<string>(s => s == Core.Constants.EnclaveServiceTypes.Function),
                    It.Is<string>(s => s == Core.Constants.FunctionOperations.CreateFunction),
                    It.IsAny<object>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingFunction_ReturnsFunction()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var function = new Function
            {
                Id = functionId,
                Name = "TestFunction",
                Description = "Test function description",
                Runtime = FunctionRuntime.JavaScript.ToString(),
                SourceCode = "function main() { return 'Hello, World!'; }",
                EntryPoint = "main",
                AccountId = Guid.NewGuid(),
                MaxExecutionTime = 30000,
                MaxMemory = 128,
                Status = "Active"
            };

            _functionRepositoryMock
                .Setup(x => x.GetByIdAsync(functionId))
                .ReturnsAsync(function);

            // Act
            var result = await _functionService.GetByIdAsync(functionId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(functionId, result.Id);
            Assert.Equal(function.Name, result.Name);
            Assert.Equal(function.Description, result.Description);
            Assert.Equal(function.Runtime, result.Runtime);
            Assert.Equal(function.SourceCode, result.SourceCode);
            Assert.Equal(function.EntryPoint, result.EntryPoint);
            Assert.Equal(function.AccountId, result.AccountId);
            Assert.Equal(function.MaxExecutionTime, result.MaxExecutionTime);
            Assert.Equal(function.MaxMemory, result.MaxMemory);
            Assert.Equal(function.Status, result.Status);

            _functionRepositoryMock.Verify(x => x.GetByIdAsync(functionId), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingFunction_ReturnsNull()
        {
            // Arrange
            var functionId = Guid.NewGuid();

            _functionRepositoryMock
                .Setup(x => x.GetByIdAsync(functionId))
                .ReturnsAsync((Function)null);

            // Act
            var result = await _functionService.GetByIdAsync(functionId);

            // Assert
            Assert.Null(result);
            _functionRepositoryMock.Verify(x => x.GetByIdAsync(functionId), Times.Once);
        }

        [Fact]
        public async Task GetFunctionAsync_ExistingFunction_ReturnsFunction()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var function = new Function
            {
                Id = functionId,
                Name = "TestFunction",
                Description = "Test function description",
                Runtime = FunctionRuntime.JavaScript.ToString(),
                SourceCode = "function main() { return 'Hello, World!'; }",
                EntryPoint = "main",
                AccountId = Guid.NewGuid(),
                MaxExecutionTime = 30000,
                MaxMemory = 128,
                Status = "Active"
            };

            _functionRepositoryMock
                .Setup(x => x.GetByIdAsync(functionId))
                .ReturnsAsync(function);

            // Act
            var result = await _functionService.GetFunctionAsync(functionId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(functionId, result.Id);
            Assert.Equal(function.Name, result.Name);

            _functionRepositoryMock.Verify(x => x.GetByIdAsync(functionId), Times.Once);
        }

        [Fact]
        public async Task GetFunctionAsync_StringId_ReturnsFunction()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var functionIdString = functionId.ToString();
            var function = new Function
            {
                Id = functionId,
                Name = "TestFunction",
                Description = "Test function description",
                Runtime = FunctionRuntime.JavaScript.ToString(),
                SourceCode = "function main() { return 'Hello, World!'; }",
                EntryPoint = "main",
                AccountId = Guid.NewGuid(),
                MaxExecutionTime = 30000,
                MaxMemory = 128,
                Status = "Active"
            };

            _functionRepositoryMock
                .Setup(x => x.GetByIdAsync(functionId))
                .ReturnsAsync(function);

            // Act
            var result = await _functionService.GetFunctionAsync(functionIdString);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(functionId, result.Id);
            Assert.Equal(function.Name, result.Name);

            _functionRepositoryMock.Verify(x => x.GetByIdAsync(functionId), Times.Once);
        }

        [Fact]
        public async Task UpdateSourceCodeAsync_ExistingFunction_ReturnsUpdatedFunction()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var originalSourceCode = "function main() { return 'Hello, World!'; }";
            var newSourceCode = "function main() { return 'Hello, Updated World!'; }";

            var function = new Function
            {
                Id = functionId,
                Name = "TestFunction",
                Description = "Test function description",
                Runtime = FunctionRuntime.JavaScript.ToString(),
                SourceCode = originalSourceCode,
                EntryPoint = "main",
                AccountId = Guid.NewGuid(),
                MaxExecutionTime = 30000,
                MaxMemory = 128,
                Status = "Active"
            };

            _functionRepositoryMock
                .Setup(x => x.GetByIdAsync(functionId))
                .ReturnsAsync(function);

            _functionRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Function>()))
                .ReturnsAsync((Guid id, Function f) => f);

            _enclaveServiceMock
                .Setup(x => x.SendRequestAsync<object, object>(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .ReturnsAsync(new object());

            // Act
            var result = await _functionService.UpdateSourceCodeAsync(functionId, newSourceCode);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(functionId, result.Id);
            Assert.Equal(newSourceCode, result.SourceCode);

            _functionRepositoryMock.Verify(x => x.GetByIdAsync(functionId), Times.Once);
            _functionRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Function>()), Times.Once);
            _enclaveServiceMock.Verify(
                x => x.SendRequestAsync<object, object>(
                    It.Is<string>(s => s == Core.Constants.EnclaveServiceTypes.Function),
                    It.Is<string>(s => s == Core.Constants.FunctionOperations.UpdateSourceCode),
                    It.IsAny<object>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ValidFunction_ReturnsExecutionResult()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var parameters = new Dictionary<string, object>
            {
                { "param1", "value1" },
                { "param2", 42 }
            };

            var function = new Function
            {
                Id = functionId,
                Name = "TestFunction",
                Description = "Test function description",
                Runtime = FunctionRuntime.JavaScript.ToString(),
                SourceCode = "function main(params) { return params; }",
                EntryPoint = "main",
                AccountId = Guid.NewGuid(),
                MaxExecutionTime = 30000,
                MaxMemory = 128,
                Status = "Active"
            };

            var expectedResult = new { result = "Success" };

            _functionRepositoryMock
                .Setup(x => x.GetByIdAsync(functionId))
                .ReturnsAsync(function);

            _executionRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<FunctionExecutionResult>()))
                .ReturnsAsync((FunctionExecutionResult e) => e);

            _executionRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<FunctionExecutionResult>()))
                .ReturnsAsync((Guid id, FunctionExecutionResult e) => e);

            _functionRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Function>()))
                .ReturnsAsync((Guid id, Function f) => f);

            _enclaveServiceMock
                .Setup(x => x.SendRequestAsync<object, object>(
                    It.Is<string>(s => s == Core.Constants.EnclaveServiceTypes.Function),
                    It.Is<string>(s => s == Core.Constants.FunctionOperations.ExecuteFunction),
                    It.IsAny<object>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _functionService.ExecuteAsync(functionId, parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult, result);

            _functionRepositoryMock.Verify(x => x.GetByIdAsync(functionId), Times.Once);
            _executionRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<FunctionExecutionResult>()), Times.Once);
            _executionRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<FunctionExecutionResult>()), Times.Once);
            _functionRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Function>()), Times.Once);
            _enclaveServiceMock.Verify(
                x => x.SendRequestAsync<object, object>(
                    It.Is<string>(s => s == Core.Constants.EnclaveServiceTypes.Function),
                    It.Is<string>(s => s == Core.Constants.FunctionOperations.ExecuteFunction),
                    It.IsAny<object>()),
                Times.Once);
        }
    }
}
