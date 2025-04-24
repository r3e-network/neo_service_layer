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
    public class SecretsErrorHandlingTests : IClassFixture<MockServiceTestFixture>
    {
        private readonly MockServiceTestFixture _fixture;
        private readonly ISecretsService _secretsService;

        public SecretsErrorHandlingTests(MockServiceTestFixture fixture)
        {
            _fixture = fixture;
            _secretsService = _fixture.ServiceProvider.GetRequiredService<ISecretsService>();
        }

        [Fact]
        public async Task CreateSecret_EmptyName_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var emptyName = string.Empty;
            var secretValue = "secret-value";
            var description = "Test secret";

            _fixture.SecretsServiceMock
                .Setup(x => x.CreateSecretAsync(
                    It.Is<string>(s => string.IsNullOrEmpty(s)),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<List<Guid>>(),
                    It.IsAny<DateTime?>()))
                .ThrowsAsync(new ArgumentException("Secret name cannot be empty"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _secretsService.CreateSecretAsync(emptyName, secretValue, description, accountId, new List<Guid>()));
            
            Assert.Contains("cannot be empty", exception.Message);
        }

        [Fact]
        public async Task CreateSecret_EmptyValue_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var secretName = "api-key";
            var emptyValue = string.Empty;
            var description = "Test secret";

            _fixture.SecretsServiceMock
                .Setup(x => x.CreateSecretAsync(
                    It.IsAny<string>(),
                    It.Is<string>(s => string.IsNullOrEmpty(s)),
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<List<Guid>>(),
                    It.IsAny<DateTime?>()))
                .ThrowsAsync(new ArgumentException("Secret value cannot be empty"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _secretsService.CreateSecretAsync(secretName, emptyValue, description, accountId, new List<Guid>()));
            
            Assert.Contains("cannot be empty", exception.Message);
        }

        [Fact]
        public async Task CreateSecret_DuplicateName_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var existingSecretName = "existing-api-key";
            var secretValue = "secret-value";
            var description = "Test secret";

            _fixture.SecretsServiceMock
                .Setup(x => x.CreateSecretAsync(
                    existingSecretName,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    accountId,
                    It.IsAny<List<Guid>>(),
                    It.IsAny<DateTime?>()))
                .ThrowsAsync(new SecretsException("A secret with this name already exists for this account"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<SecretsException>(() => 
                _secretsService.CreateSecretAsync(existingSecretName, secretValue, description, accountId, new List<Guid>()));
            
            Assert.Contains("already exists", exception.Message);
        }

        [Fact]
        public async Task GetSecretValue_SecretNotFound_ThrowsException()
        {
            // Arrange
            var nonExistentSecretId = Guid.NewGuid();
            var functionId = Guid.NewGuid();

            _fixture.SecretsServiceMock
                .Setup(x => x.GetSecretValueAsync(nonExistentSecretId, It.IsAny<Guid>()))
                .ThrowsAsync(new KeyNotFoundException("Secret not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _secretsService.GetSecretValueAsync(nonExistentSecretId, functionId));
            
            Assert.Contains("not found", exception.Message);
        }

        [Fact]
        public async Task GetSecretValue_NoAccess_ThrowsException()
        {
            // Arrange
            var secretId = Guid.NewGuid();
            var unauthorizedFunctionId = Guid.NewGuid();

            _fixture.SecretsServiceMock
                .Setup(x => x.HasAccessAsync(secretId, unauthorizedFunctionId))
                .ReturnsAsync(false);

            _fixture.SecretsServiceMock
                .Setup(x => x.GetSecretValueAsync(secretId, unauthorizedFunctionId))
                .ThrowsAsync(new SecretsException("Function does not have access to this secret"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<SecretsException>(() => 
                _secretsService.GetSecretValueAsync(secretId, unauthorizedFunctionId));
            
            Assert.Contains("does not have access", exception.Message);
        }

        [Fact]
        public async Task UpdateAllowedFunctions_SecretNotFound_ThrowsException()
        {
            // Arrange
            var nonExistentSecretId = Guid.NewGuid();
            var functionIds = new List<Guid> { Guid.NewGuid() };

            _fixture.SecretsServiceMock
                .Setup(x => x.UpdateAllowedFunctionsAsync(nonExistentSecretId, It.IsAny<List<Guid>>()))
                .ThrowsAsync(new KeyNotFoundException("Secret not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _secretsService.UpdateAllowedFunctionsAsync(nonExistentSecretId, functionIds));
            
            Assert.Contains("not found", exception.Message);
        }

        [Fact]
        public async Task RotateSecret_SecretNotFound_ThrowsException()
        {
            // Arrange
            var nonExistentSecretId = Guid.NewGuid();
            var newValue = "new-secret-value";

            _fixture.SecretsServiceMock
                .Setup(x => x.RotateSecretAsync(nonExistentSecretId, It.IsAny<string>()))
                .ThrowsAsync(new KeyNotFoundException("Secret not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _secretsService.RotateSecretAsync(nonExistentSecretId, newValue));
            
            Assert.Contains("not found", exception.Message);
        }

        [Fact]
        public async Task RotateSecret_EmptyValue_ThrowsException()
        {
            // Arrange
            var secretId = Guid.NewGuid();
            var emptyValue = string.Empty;

            _fixture.SecretsServiceMock
                .Setup(x => x.RotateSecretAsync(secretId, It.Is<string>(s => string.IsNullOrEmpty(s))))
                .ThrowsAsync(new ArgumentException("New secret value cannot be empty"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _secretsService.RotateSecretAsync(secretId, emptyValue));
            
            Assert.Contains("cannot be empty", exception.Message);
        }
    }
}
