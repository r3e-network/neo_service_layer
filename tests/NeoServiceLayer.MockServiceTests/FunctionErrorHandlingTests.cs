using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NeoServiceLayer.Core.Enums;
using NeoServiceLayer.Core.Exceptions;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.MockServiceTests.TestFixtures;
using Xunit;

namespace NeoServiceLayer.MockServiceTests
{
    public class FunctionErrorHandlingTests : IClassFixture<MockServiceTestFixture>
    {
        private readonly MockServiceTestFixture _fixture;
        private readonly IFunctionService _functionService;

        public FunctionErrorHandlingTests(MockServiceTestFixture fixture)
        {
            _fixture = fixture;
            _functionService = _fixture.ServiceProvider.GetRequiredService<IFunctionService>();
        }

        [Fact]
        public async Task CreateFunction_EmptyName_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var emptyName = string.Empty;
            var description = "Test function";
            var runtime = FunctionRuntime.CSharp;
            var sourceCode = "public class Test { public static string Run() { return \"Hello\"; } }";
            var entryPoint = "Test.Run";

            _fixture.FunctionServiceMock
                .Setup(x => x.CreateFunctionAsync(
                    It.Is<string>(s => string.IsNullOrEmpty(s)),
                    It.IsAny<string>(),
                    It.IsAny<FunctionRuntime>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<List<Guid>>(),
                    It.IsAny<Dictionary<string, string>>()))
                .ThrowsAsync(new ArgumentException("Function name cannot be empty"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _functionService.CreateFunctionAsync(
                    emptyName, 
                    description, 
                    runtime, 
                    sourceCode, 
                    entryPoint, 
                    accountId));
            
            Assert.Contains("cannot be empty", exception.Message);
        }

        [Fact]
        public async Task CreateFunction_EmptySourceCode_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var name = "test-function";
            var description = "Test function";
            var runtime = FunctionRuntime.CSharp;
            var emptySourceCode = string.Empty;
            var entryPoint = "Test.Run";

            _fixture.FunctionServiceMock
                .Setup(x => x.CreateFunctionAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<FunctionRuntime>(),
                    It.Is<string>(s => string.IsNullOrEmpty(s)),
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<List<Guid>>(),
                    It.IsAny<Dictionary<string, string>>()))
                .ThrowsAsync(new ArgumentException("Source code cannot be empty"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _functionService.CreateFunctionAsync(
                    name, 
                    description, 
                    runtime, 
                    emptySourceCode, 
                    entryPoint, 
                    accountId));
            
            Assert.Contains("cannot be empty", exception.Message);
        }

        [Fact]
        public async Task CreateFunction_EmptyEntryPoint_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var name = "test-function";
            var description = "Test function";
            var runtime = FunctionRuntime.CSharp;
            var sourceCode = "public class Test { public static string Run() { return \"Hello\"; } }";
            var emptyEntryPoint = string.Empty;

            _fixture.FunctionServiceMock
                .Setup(x => x.CreateFunctionAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<FunctionRuntime>(),
                    It.IsAny<string>(),
                    It.Is<string>(s => string.IsNullOrEmpty(s)),
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<List<Guid>>(),
                    It.IsAny<Dictionary<string, string>>()))
                .ThrowsAsync(new ArgumentException("Entry point cannot be empty"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _functionService.CreateFunctionAsync(
                    name, 
                    description, 
                    runtime, 
                    sourceCode, 
                    emptyEntryPoint, 
                    accountId));
            
            Assert.Contains("cannot be empty", exception.Message);
        }

        [Fact]
        public async Task CreateFunction_DuplicateName_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var existingName = "existing-function";
            var description = "Test function";
            var runtime = FunctionRuntime.CSharp;
            var sourceCode = "public class Test { public static string Run() { return \"Hello\"; } }";
            var entryPoint = "Test.Run";

            _fixture.FunctionServiceMock
                .Setup(x => x.CreateFunctionAsync(
                    existingName,
                    It.IsAny<string>(),
                    It.IsAny<FunctionRuntime>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    accountId,
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<List<Guid>>(),
                    It.IsAny<Dictionary<string, string>>()))
                .ThrowsAsync(new InvalidOperationException("A function with this name already exists for this account"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _functionService.CreateFunctionAsync(
                    existingName, 
                    description, 
                    runtime, 
                    sourceCode, 
                    entryPoint, 
                    accountId));
            
            Assert.Contains("already exists", exception.Message);
        }

        [Fact]
        public async Task ExecuteFunction_FunctionNotFound_ThrowsException()
        {
            // Arrange
            var nonExistentFunctionId = Guid.NewGuid();
            var parameters = new Dictionary<string, object>();

            _fixture.FunctionServiceMock
                .Setup(x => x.ExecuteAsync(nonExistentFunctionId, It.IsAny<Dictionary<string, object>>()))
                .ThrowsAsync(new KeyNotFoundException("Function not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _functionService.ExecuteAsync(nonExistentFunctionId, parameters));
            
            Assert.Contains("not found", exception.Message);
        }

        [Fact]
        public async Task ExecuteFunction_FunctionInactive_ThrowsException()
        {
            // Arrange
            var inactiveFunctionId = Guid.NewGuid();
            var parameters = new Dictionary<string, object>();

            _fixture.FunctionServiceMock
                .Setup(x => x.ExecuteAsync(inactiveFunctionId, It.IsAny<Dictionary<string, object>>()))
                .ThrowsAsync(new InvalidOperationException("Function is not active"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _functionService.ExecuteAsync(inactiveFunctionId, parameters));
            
            Assert.Contains("not active", exception.Message);
        }

        [Fact]
        public async Task ExecuteFunction_RuntimeError_ThrowsException()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var parameters = new Dictionary<string, object>();

            _fixture.FunctionServiceMock
                .Setup(x => x.ExecuteAsync(functionId, It.IsAny<Dictionary<string, object>>()))
                .ThrowsAsync(new Exception("Runtime error: Division by zero"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _functionService.ExecuteAsync(functionId, parameters));
            
            Assert.Contains("Runtime error", exception.Message);
        }

        [Fact]
        public async Task UpdateSourceCode_FunctionNotFound_ThrowsException()
        {
            // Arrange
            var nonExistentFunctionId = Guid.NewGuid();
            var newSourceCode = "public class Test { public static string Run() { return \"Updated\"; } }";

            _fixture.FunctionServiceMock
                .Setup(x => x.UpdateSourceCodeAsync(nonExistentFunctionId, It.IsAny<string>()))
                .ThrowsAsync(new KeyNotFoundException("Function not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _functionService.UpdateSourceCodeAsync(nonExistentFunctionId, newSourceCode));
            
            Assert.Contains("not found", exception.Message);
        }

        [Fact]
        public async Task UpdateSourceCode_EmptySourceCode_ThrowsException()
        {
            // Arrange
            var functionId = Guid.NewGuid();
            var emptySourceCode = string.Empty;

            _fixture.FunctionServiceMock
                .Setup(x => x.UpdateSourceCodeAsync(functionId, It.Is<string>(s => string.IsNullOrEmpty(s))))
                .ThrowsAsync(new ArgumentException("Source code cannot be empty"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _functionService.UpdateSourceCodeAsync(functionId, emptySourceCode));
            
            Assert.Contains("cannot be empty", exception.Message);
        }
    }
}
