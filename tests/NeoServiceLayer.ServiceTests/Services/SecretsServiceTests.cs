using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NeoServiceLayer.Core;
using NeoServiceLayer.Core.Exceptions;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Services.Secrets;
using NeoServiceLayer.Services.Secrets.Repositories;
using NeoServiceLayer.ServiceTests.Mocks;
using Xunit;

namespace NeoServiceLayer.ServiceTests.Services
{
    public class SecretsServiceTests
    {
        private readonly Mock<ILogger<SecretsService>> _loggerMock;
        private readonly Mock<ISecretsRepository> _secretsRepositoryMock;
        private readonly MockEnclaveService _enclaveService;
        private readonly SecretsService _secretsService;

        public SecretsServiceTests()
        {
            _loggerMock = new Mock<ILogger<SecretsService>>();
            _secretsRepositoryMock = new Mock<ISecretsRepository>();
            _enclaveService = new MockEnclaveService();

            _secretsService = new SecretsService(
                _loggerMock.Object,
                _secretsRepositoryMock.Object,
                _enclaveService);

            // Setup enclave service handlers
            _enclaveService.RegisterHandler<object, Secret>(
                Constants.EnclaveServiceTypes.Secrets,
                Constants.SecretsOperations.CreateSecret,
                request => new Secret
                {
                    Id = Guid.NewGuid(),
                    Name = ((dynamic)request).Name,
                    Description = ((dynamic)request).Description,
                    EncryptedValue = "encrypted:" + ((dynamic)request).Value,
                    Version = 1,
                    AccountId = ((dynamic)request).AccountId,
                    AllowedFunctionIds = ((dynamic)request).AllowedFunctionIds,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    ExpiresAt = ((dynamic)request).ExpiresAt
                });

            _enclaveService.RegisterHandler<object, Secret>(
                Constants.EnclaveServiceTypes.Secrets,
                Constants.SecretsOperations.UpdateValue,
                request => new Secret
                {
                    Id = ((dynamic)request).Id,
                    EncryptedValue = "encrypted:" + ((dynamic)request).Value,
                    Version = 2
                });

            _enclaveService.RegisterHandler<object, Secret>(
                Constants.EnclaveServiceTypes.Secrets,
                Constants.SecretsOperations.RotateSecret,
                request => new Secret
                {
                    Id = ((dynamic)request).Id,
                    EncryptedValue = "encrypted:" + ((dynamic)request).NewValue,
                    Version = 3
                });

            _enclaveService.RegisterHandler<object, object>(
                Constants.EnclaveServiceTypes.Secrets,
                Constants.SecretsOperations.GetSecretValue,
                request => new { Value = "decrypted-secret-value" });

            _enclaveService.RegisterHandler<object, object>(
                Constants.EnclaveServiceTypes.Secrets,
                Constants.SecretsOperations.HasAccess,
                request => new { HasAccess = true });
        }

        [Fact]
        public async Task CreateSecretAsync_ValidInput_ReturnsCreatedSecret()
        {
            // Arrange
            var name = "test-secret";
            var value = "secret-value";
            var description = "Test secret";
            var accountId = Guid.NewGuid();
            var allowedFunctionIds = new List<Guid> { Guid.NewGuid() };
            var expiresAt = DateTime.UtcNow.AddDays(30);

            var createdSecret = new Secret
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                EncryptedValue = "encrypted:secret-value",
                Version = 1,
                AccountId = accountId,
                AllowedFunctionIds = allowedFunctionIds,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt
            };

            _secretsRepositoryMock
                .Setup(x => x.GetByNameAsync(name, accountId))
                .ReturnsAsync((Secret)null);

            _secretsRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<Secret>()))
                .ReturnsAsync(createdSecret);

            // Act
            var result = await _secretsService.CreateSecretAsync(name, value, description, accountId, allowedFunctionIds, expiresAt);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(name, result.Name);
            Assert.Equal(description, result.Description);
            Assert.Equal("encrypted:secret-value", result.EncryptedValue);
            Assert.Equal(1, result.Version);
            Assert.Equal(accountId, result.AccountId);
            Assert.Equal(allowedFunctionIds, result.AllowedFunctionIds);
            Assert.Equal(expiresAt, result.ExpiresAt);
        }

        [Fact]
        public async Task CreateSecretAsync_ExistingName_ThrowsSecretsException()
        {
            // Arrange
            var name = "existing-secret";
            var value = "secret-value";
            var description = "Test secret";
            var accountId = Guid.NewGuid();
            var allowedFunctionIds = new List<Guid> { Guid.NewGuid() };

            var existingSecret = new Secret
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = "Existing secret",
                EncryptedValue = "encrypted:existing-value",
                Version = 1,
                AccountId = accountId,
                AllowedFunctionIds = new List<Guid>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _secretsRepositoryMock
                .Setup(x => x.GetByNameAsync(name, accountId))
                .ReturnsAsync(existingSecret);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<SecretsException>(() => 
                _secretsService.CreateSecretAsync(name, value, description, accountId, allowedFunctionIds));
            
            Assert.Contains("Secret with this name already exists", exception.Message);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsSecret()
        {
            // Arrange
            var secretId = Guid.NewGuid();
            var secret = new Secret
            {
                Id = secretId,
                Name = "test-secret",
                Description = "Test secret",
                EncryptedValue = "encrypted:secret-value",
                Version = 1,
                AccountId = Guid.NewGuid(),
                AllowedFunctionIds = new List<Guid> { Guid.NewGuid() },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _secretsRepositoryMock
                .Setup(x => x.GetByIdAsync(secretId))
                .ReturnsAsync(secret);

            // Act
            var result = await _secretsService.GetByIdAsync(secretId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(secretId, result.Id);
            Assert.Equal("test-secret", result.Name);
            Assert.Equal("Test secret", result.Description);
        }

        [Fact]
        public async Task GetByNameAsync_ExistingName_ReturnsSecret()
        {
            // Arrange
            var name = "test-secret";
            var accountId = Guid.NewGuid();
            var secret = new Secret
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = "Test secret",
                EncryptedValue = "encrypted:secret-value",
                Version = 1,
                AccountId = accountId,
                AllowedFunctionIds = new List<Guid> { Guid.NewGuid() },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _secretsRepositoryMock
                .Setup(x => x.GetByNameAsync(name, accountId))
                .ReturnsAsync(secret);

            // Act
            var result = await _secretsService.GetByNameAsync(name, accountId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(name, result.Name);
            Assert.Equal(accountId, result.AccountId);
        }

        [Fact]
        public async Task UpdateValueAsync_ExistingSecret_ReturnsUpdatedSecret()
        {
            // Arrange
            var secretId = Guid.NewGuid();
            var newValue = "new-secret-value";
            var secret = new Secret
            {
                Id = secretId,
                Name = "test-secret",
                Description = "Test secret",
                EncryptedValue = "encrypted:old-value",
                Version = 1,
                AccountId = Guid.NewGuid(),
                AllowedFunctionIds = new List<Guid> { Guid.NewGuid() },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _secretsRepositoryMock
                .Setup(x => x.GetByIdAsync(secretId))
                .ReturnsAsync(secret);

            _secretsRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<Secret>()))
                .ReturnsAsync((Secret s) => s);

            // Act
            var result = await _secretsService.UpdateValueAsync(secretId, newValue);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(secretId, result.Id);
            Assert.Equal("encrypted:new-secret-value", result.EncryptedValue);
            Assert.Equal(2, result.Version);
        }

        [Fact]
        public async Task UpdateValueAsync_NonExistingSecret_ThrowsSecretsException()
        {
            // Arrange
            var secretId = Guid.NewGuid();
            var newValue = "new-secret-value";

            _secretsRepositoryMock
                .Setup(x => x.GetByIdAsync(secretId))
                .ReturnsAsync((Secret)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<SecretsException>(() => 
                _secretsService.UpdateValueAsync(secretId, newValue));
            
            Assert.Contains("Secret not found", exception.Message);
        }

        [Fact]
        public async Task UpdateAllowedFunctionsAsync_ExistingSecret_ReturnsUpdatedSecret()
        {
            // Arrange
            var secretId = Guid.NewGuid();
            var newAllowedFunctionIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var secret = new Secret
            {
                Id = secretId,
                Name = "test-secret",
                Description = "Test secret",
                EncryptedValue = "encrypted:secret-value",
                Version = 1,
                AccountId = Guid.NewGuid(),
                AllowedFunctionIds = new List<Guid> { Guid.NewGuid() },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _secretsRepositoryMock
                .Setup(x => x.GetByIdAsync(secretId))
                .ReturnsAsync(secret);

            _secretsRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<Secret>()))
                .ReturnsAsync((Secret s) => s);

            // Act
            var result = await _secretsService.UpdateAllowedFunctionsAsync(secretId, newAllowedFunctionIds);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(secretId, result.Id);
            Assert.Equal(newAllowedFunctionIds, result.AllowedFunctionIds);
        }

        [Fact]
        public async Task GetSecretValueAsync_ValidAccess_ReturnsDecryptedValue()
        {
            // Arrange
            var secretId = Guid.NewGuid();
            var functionId = Guid.NewGuid();
            var secret = new Secret
            {
                Id = secretId,
                Name = "test-secret",
                Description = "Test secret",
                EncryptedValue = "encrypted:secret-value",
                Version = 1,
                AccountId = Guid.NewGuid(),
                AllowedFunctionIds = new List<Guid> { functionId },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _secretsRepositoryMock
                .Setup(x => x.GetByIdAsync(secretId))
                .ReturnsAsync(secret);

            // Act
            var result = await _secretsService.GetSecretValueAsync(secretId, functionId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("decrypted-secret-value", result);
        }

        [Fact]
        public async Task HasAccessAsync_ValidAccess_ReturnsTrue()
        {
            // Arrange
            var secretId = Guid.NewGuid();
            var functionId = Guid.NewGuid();
            var secret = new Secret
            {
                Id = secretId,
                Name = "test-secret",
                Description = "Test secret",
                EncryptedValue = "encrypted:secret-value",
                Version = 1,
                AccountId = Guid.NewGuid(),
                AllowedFunctionIds = new List<Guid> { functionId },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _secretsRepositoryMock
                .Setup(x => x.GetByIdAsync(secretId))
                .ReturnsAsync(secret);

            // Act
            var result = await _secretsService.HasAccessAsync(secretId, functionId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task HasAccessAsync_ExpiredSecret_ReturnsFalse()
        {
            // Arrange
            var secretId = Guid.NewGuid();
            var functionId = Guid.NewGuid();
            var secret = new Secret
            {
                Id = secretId,
                Name = "test-secret",
                Description = "Test secret",
                EncryptedValue = "encrypted:secret-value",
                Version = 1,
                AccountId = Guid.NewGuid(),
                AllowedFunctionIds = new List<Guid> { functionId },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(-1) // Expired
            };

            _secretsRepositoryMock
                .Setup(x => x.GetByIdAsync(secretId))
                .ReturnsAsync(secret);

            // Act
            var result = await _secretsService.HasAccessAsync(secretId, functionId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task HasAccessAsync_UnauthorizedFunction_ReturnsFalse()
        {
            // Arrange
            var secretId = Guid.NewGuid();
            var functionId = Guid.NewGuid();
            var unauthorizedFunctionId = Guid.NewGuid();
            var secret = new Secret
            {
                Id = secretId,
                Name = "test-secret",
                Description = "Test secret",
                EncryptedValue = "encrypted:secret-value",
                Version = 1,
                AccountId = Guid.NewGuid(),
                AllowedFunctionIds = new List<Guid> { functionId }, // Different function ID
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _secretsRepositoryMock
                .Setup(x => x.GetByIdAsync(secretId))
                .ReturnsAsync(secret);

            // Act
            var result = await _secretsService.HasAccessAsync(secretId, unauthorizedFunctionId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RotateSecretAsync_ExistingSecret_ReturnsRotatedSecret()
        {
            // Arrange
            var secretId = Guid.NewGuid();
            var newValue = "rotated-secret-value";
            var secret = new Secret
            {
                Id = secretId,
                Name = "test-secret",
                Description = "Test secret",
                EncryptedValue = "encrypted:old-value",
                Version = 2,
                AccountId = Guid.NewGuid(),
                AllowedFunctionIds = new List<Guid> { Guid.NewGuid() },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _secretsRepositoryMock
                .Setup(x => x.GetByIdAsync(secretId))
                .ReturnsAsync(secret);

            _secretsRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<Secret>()))
                .ReturnsAsync((Secret s) => s);

            // Act
            var result = await _secretsService.RotateSecretAsync(secretId, newValue);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(secretId, result.Id);
            Assert.Equal("encrypted:rotated-secret-value", result.EncryptedValue);
            Assert.Equal(3, result.Version);
        }

        [Fact]
        public async Task DeleteAsync_ExistingSecret_ReturnsTrue()
        {
            // Arrange
            var secretId = Guid.NewGuid();

            _secretsRepositoryMock
                .Setup(x => x.DeleteAsync(secretId))
                .ReturnsAsync(true);

            // Act
            var result = await _secretsService.DeleteAsync(secretId);

            // Assert
            Assert.True(result);
        }
    }
}
