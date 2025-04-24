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
using Xunit;

namespace NeoServiceLayer.Tests.Unit
{
    public class SecretsServiceTests
    {
        private readonly Mock<ILogger<SecretsService>> _loggerMock;
        private readonly Mock<ISecretsRepository> _secretsRepositoryMock;
        private readonly Mock<IEnclaveService> _enclaveServiceMock;
        private readonly SecretsService _secretsService;

        public SecretsServiceTests()
        {
            _loggerMock = new Mock<ILogger<SecretsService>>();
            _secretsRepositoryMock = new Mock<ISecretsRepository>();
            _enclaveServiceMock = new Mock<IEnclaveService>();

            _secretsService = new SecretsService(
                _loggerMock.Object,
                _secretsRepositoryMock.Object,
                _enclaveServiceMock.Object);
        }

        [Fact]
        public async Task CreateSecretAsync_ValidInput_ReturnsCreatedSecret()
        {
            // Arrange
            var name = "TestSecret";
            var value = "SecretValue123!";
            var description = "Test secret description";
            var accountId = Guid.NewGuid();
            var allowedFunctionIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var expiresAt = DateTime.UtcNow.AddDays(30);

            var createdSecret = new Secret
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                AccountId = accountId,
                AllowedFunctionIds = allowedFunctionIds,
                EncryptedValue = "EncryptedValue",
                Version = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt
            };

            _secretsRepositoryMock
                .Setup(x => x.GetByNameAsync(name, accountId))
                .ReturnsAsync((Secret)null);

            _enclaveServiceMock
                .Setup(x => x.SendRequestAsync<object, Secret>(
                    It.Is<string>(s => s == Core.Constants.EnclaveServiceTypes.Secrets),
                    It.Is<string>(s => s == Core.Constants.SecretsOperations.CreateSecret),
                    It.IsAny<object>()))
                .ReturnsAsync(createdSecret);

            _secretsRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<Secret>()))
                .ReturnsAsync(createdSecret);

            // Act
            var result = await _secretsService.CreateSecretAsync(
                name,
                value,
                description,
                accountId,
                allowedFunctionIds,
                expiresAt);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createdSecret.Id, result.Id);
            Assert.Equal(name, result.Name);
            Assert.Equal(description, result.Description);
            Assert.Equal(accountId, result.AccountId);
            Assert.Equal(allowedFunctionIds, result.AllowedFunctionIds);
            Assert.Equal(expiresAt, result.ExpiresAt);
            Assert.Equal("EncryptedValue", result.EncryptedValue);
            Assert.Equal(1, result.Version);

            _secretsRepositoryMock.Verify(x => x.GetByNameAsync(name, accountId), Times.Once);
            _enclaveServiceMock.Verify(
                x => x.SendRequestAsync<object, Secret>(
                    It.Is<string>(s => s == Core.Constants.EnclaveServiceTypes.Secrets),
                    It.Is<string>(s => s == Core.Constants.SecretsOperations.CreateSecret),
                    It.IsAny<object>()),
                Times.Once);
            _secretsRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Secret>()), Times.Once);
        }

        [Fact]
        public async Task CreateSecretAsync_DuplicateName_ThrowsException()
        {
            // Arrange
            var name = "TestSecret";
            var value = "SecretValue123!";
            var description = "Test secret description";
            var accountId = Guid.NewGuid();
            var allowedFunctionIds = new List<Guid> { Guid.NewGuid() };

            var existingSecret = new Secret
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = "Existing secret",
                AccountId = accountId,
                AllowedFunctionIds = new List<Guid>(),
                EncryptedValue = "EncryptedValue",
                Version = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _secretsRepositoryMock
                .Setup(x => x.GetByNameAsync(name, accountId))
                .ReturnsAsync(existingSecret);

            // Act & Assert
            await Assert.ThrowsAsync<SecretsException>(() =>
                _secretsService.CreateSecretAsync(
                    name,
                    value,
                    description,
                    accountId,
                    allowedFunctionIds));

            _secretsRepositoryMock.Verify(x => x.GetByNameAsync(name, accountId), Times.Once);
            _enclaveServiceMock.Verify(
                x => x.SendRequestAsync<object, Secret>(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()),
                Times.Never);
            _secretsRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Secret>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingSecret_ReturnsSecret()
        {
            // Arrange
            var secretId = Guid.NewGuid();
            var secret = new Secret
            {
                Id = secretId,
                Name = "TestSecret",
                Description = "Test secret description",
                AccountId = Guid.NewGuid(),
                AllowedFunctionIds = new List<Guid> { Guid.NewGuid() },
                EncryptedValue = "EncryptedValue",
                Version = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _secretsRepositoryMock
                .Setup(x => x.GetByIdAsync(secretId))
                .ReturnsAsync(secret);

            // Act
            var result = await _secretsService.GetByIdAsync(secretId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(secretId, result.Id);
            Assert.Equal(secret.Name, result.Name);
            Assert.Equal(secret.Description, result.Description);
            Assert.Equal(secret.AccountId, result.AccountId);
            Assert.Equal(secret.AllowedFunctionIds, result.AllowedFunctionIds);
            Assert.Equal(secret.EncryptedValue, result.EncryptedValue);
            Assert.Equal(secret.Version, result.Version);

            _secretsRepositoryMock.Verify(x => x.GetByIdAsync(secretId), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingSecret_ReturnsNull()
        {
            // Arrange
            var secretId = Guid.NewGuid();

            _secretsRepositoryMock
                .Setup(x => x.GetByIdAsync(secretId))
                .ReturnsAsync((Secret?)null);

            // Act
            var result = await _secretsService.GetByIdAsync(secretId);

            // Assert
            Assert.Null(result);
            _secretsRepositoryMock.Verify(x => x.GetByIdAsync(secretId), Times.Once);
        }

        [Fact]
        public async Task UpdateValueAsync_ExistingSecret_ReturnsUpdatedSecret()
        {
            // Arrange
            var secretId = Guid.NewGuid();
            var newValue = "NewSecretValue456!";

            var existingSecret = new Secret
            {
                Id = secretId,
                Name = "TestSecret",
                Description = "Test secret description",
                AccountId = Guid.NewGuid(),
                AllowedFunctionIds = new List<Guid> { Guid.NewGuid() },
                EncryptedValue = "OldEncryptedValue",
                Version = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var updatedSecret = new Secret
            {
                Id = secretId,
                Name = existingSecret.Name,
                Description = existingSecret.Description,
                AccountId = existingSecret.AccountId,
                AllowedFunctionIds = existingSecret.AllowedFunctionIds,
                EncryptedValue = "NewEncryptedValue",
                Version = 2,
                CreatedAt = existingSecret.CreatedAt,
                UpdatedAt = DateTime.UtcNow
            };

            _secretsRepositoryMock
                .Setup(x => x.GetByIdAsync(secretId))
                .ReturnsAsync(existingSecret);

            _enclaveServiceMock
                .Setup(x => x.SendRequestAsync<object, Secret>(
                    It.Is<string>(s => s == Core.Constants.EnclaveServiceTypes.Secrets),
                    It.Is<string>(s => s == Core.Constants.SecretsOperations.UpdateSecretValue),
                    It.IsAny<object>()))
                .ReturnsAsync(updatedSecret);

            _secretsRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<Secret>()))
                .ReturnsAsync((Secret s) => s);

            // Act
            var result = await _secretsService.UpdateValueAsync(secretId, newValue);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(secretId, result.Id);
            Assert.Equal(existingSecret.Name, result.Name);
            Assert.Equal(existingSecret.Description, result.Description);
            Assert.Equal(existingSecret.AccountId, result.AccountId);
            Assert.Equal(existingSecret.AllowedFunctionIds, result.AllowedFunctionIds);
            Assert.Equal("NewEncryptedValue", result.EncryptedValue);
            Assert.Equal(2, result.Version);

            _secretsRepositoryMock.Verify(x => x.GetByIdAsync(secretId), Times.Once);
            _enclaveServiceMock.Verify(
                x => x.SendRequestAsync<object, Secret>(
                    It.Is<string>(s => s == Core.Constants.EnclaveServiceTypes.Secrets),
                    It.Is<string>(s => s == Core.Constants.SecretsOperations.UpdateSecretValue),
                    It.IsAny<object>()),
                Times.Once);
            _secretsRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Secret>()), Times.Once);
        }

        [Fact]
        public async Task HasAccessAsync_SecretExists_FunctionHasAccess_ReturnsTrue()
        {
            // Arrange
            var secretId = Guid.NewGuid();
            var functionId = Guid.NewGuid();

            var secret = new Secret
            {
                Id = secretId,
                Name = "TestSecret",
                Description = "Test secret description",
                AccountId = Guid.NewGuid(),
                AllowedFunctionIds = new List<Guid> { functionId },
                EncryptedValue = "EncryptedValue",
                Version = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _secretsRepositoryMock
                .Setup(x => x.GetByIdAsync(secretId))
                .ReturnsAsync(secret);

            _enclaveServiceMock
                .Setup(x => x.SendRequestAsync<object, object>(
                    It.Is<string>(s => s == Core.Constants.EnclaveServiceTypes.Secrets),
                    It.Is<string>(s => s == Core.Constants.SecretsOperations.HasAccess),
                    It.IsAny<object>()))
                .ReturnsAsync(new { HasAccess = true });

            // Act
            var result = await _secretsService.HasAccessAsync(secretId, functionId);

            // Assert
            Assert.True(result);
            _secretsRepositoryMock.Verify(x => x.GetByIdAsync(secretId), Times.Once);
            _enclaveServiceMock.Verify(
                x => x.SendRequestAsync<object, object>(
                    It.Is<string>(s => s == Core.Constants.EnclaveServiceTypes.Secrets),
                    It.Is<string>(s => s == Core.Constants.SecretsOperations.HasAccess),
                    It.IsAny<object>()),
                Times.Once);
        }

        [Fact]
        public async Task HasAccessAsync_SecretExists_FunctionDoesNotHaveAccess_ReturnsFalse()
        {
            // Arrange
            var secretId = Guid.NewGuid();
            var functionId = Guid.NewGuid();
            var otherFunctionId = Guid.NewGuid();

            var secret = new Secret
            {
                Id = secretId,
                Name = "TestSecret",
                Description = "Test secret description",
                AccountId = Guid.NewGuid(),
                AllowedFunctionIds = new List<Guid> { otherFunctionId },
                EncryptedValue = "EncryptedValue",
                Version = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _secretsRepositoryMock
                .Setup(x => x.GetByIdAsync(secretId))
                .ReturnsAsync(secret);

            // Act
            var result = await _secretsService.HasAccessAsync(secretId, functionId);

            // Assert
            Assert.False(result);
            _secretsRepositoryMock.Verify(x => x.GetByIdAsync(secretId), Times.Once);
            _enclaveServiceMock.Verify(
                x => x.SendRequestAsync<object, object>(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()),
                Times.Never);
        }

        [Fact]
        public async Task HasAccessAsync_SecretDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var secretId = Guid.NewGuid();
            var functionId = Guid.NewGuid();

            _secretsRepositoryMock
                .Setup(x => x.GetByIdAsync(secretId))
                .ReturnsAsync((Secret?)null);

            // Act
            var result = await _secretsService.HasAccessAsync(secretId, functionId);

            // Assert
            Assert.False(result);
            _secretsRepositoryMock.Verify(x => x.GetByIdAsync(secretId), Times.Once);
            _enclaveServiceMock.Verify(
                x => x.SendRequestAsync<object, object>(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()),
                Times.Never);
        }

        [Fact]
        public async Task HasAccessAsync_SecretExpired_ReturnsFalse()
        {
            // Arrange
            var secretId = Guid.NewGuid();
            var functionId = Guid.NewGuid();

            var secret = new Secret
            {
                Id = secretId,
                Name = "TestSecret",
                Description = "Test secret description",
                AccountId = Guid.NewGuid(),
                AllowedFunctionIds = new List<Guid> { functionId },
                EncryptedValue = "EncryptedValue",
                Version = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-30),
                ExpiresAt = DateTime.UtcNow.AddDays(-1) // Expired
            };

            _secretsRepositoryMock
                .Setup(x => x.GetByIdAsync(secretId))
                .ReturnsAsync(secret);

            // Act
            var result = await _secretsService.HasAccessAsync(secretId, functionId);

            // Assert
            Assert.False(result);
            _secretsRepositoryMock.Verify(x => x.GetByIdAsync(secretId), Times.Once);
            _enclaveServiceMock.Verify(
                x => x.SendRequestAsync<object, object>(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()),
                Times.Never);
        }
    }
}
