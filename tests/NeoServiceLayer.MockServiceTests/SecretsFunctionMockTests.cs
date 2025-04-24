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
    public class SecretsFunctionMockTests : IClassFixture<MockServiceTestFixture>
    {
        private readonly MockServiceTestFixture _fixture;
        private readonly IAccountService _accountService;
        private readonly ISecretsService _secretsService;
        private readonly IFunctionService _functionService;

        public SecretsFunctionMockTests(MockServiceTestFixture fixture)
        {
            _fixture = fixture;
            _accountService = _fixture.ServiceProvider.GetRequiredService<IAccountService>();
            _secretsService = _fixture.ServiceProvider.GetRequiredService<ISecretsService>();
            _functionService = _fixture.ServiceProvider.GetRequiredService<IFunctionService>();
        }

        [Fact]
        public async Task CreateSecretAndFunction_ThenAccessSecret_Success()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var secretId = Guid.NewGuid();
            var functionId = Guid.NewGuid();
            var username = "testuser";
            var email = "test@example.com";
            var password = "Password123!";
            var secretName = "api-key";
            var secretValue = "secret-value-123";
            var secretDescription = "API key for external service";
            var functionName = "test-function";
            var functionDescription = "Test function";
            var runtime = "dotnet";
            var handler = "TestFunction::Handler.Process";
            var sourceCode = "public class Handler { public static object Process(object input) { return input; } }";
            var entryPoint = "Handler.Process";

            var account = new Account
            {
                Id = accountId,
                Username = username,
                Email = email,
                PasswordHash = "hashedpassword",
                PasswordSalt = "salt",
                IsVerified = true,
                IsActive = true,
                Credits = 100,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var secret = new Secret
            {
                Id = secretId,
                Name = secretName,
                Description = secretDescription,
                EncryptedValue = "encrypted:" + secretValue,
                Version = 1,
                AccountId = accountId,
                AllowedFunctionIds = new List<Guid>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var secretWithFunction = new Secret
            {
                Id = secretId,
                Name = secretName,
                Description = secretDescription,
                EncryptedValue = "encrypted:" + secretValue,
                Version = 1,
                AccountId = accountId,
                AllowedFunctionIds = new List<Guid> { functionId },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var function = new Function
            {
                Id = functionId,
                Name = functionName,
                Description = functionDescription,
                Runtime = runtime,
                Handler = handler,
                SourceCode = sourceCode,
                EntryPoint = entryPoint,
                AccountId = accountId,
                MaxExecutionTime = 30000,
                MaxMemory = 256,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = "Active"
            };

            // Setup mocks
            _fixture.AccountServiceMock
                .Setup(x => x.RegisterAsync(username, email, password, It.IsAny<string>()))
                .ReturnsAsync(account);

            _fixture.SecretsServiceMock
                .Setup(x => x.CreateSecretAsync(
                    secretName,
                    secretValue,
                    secretDescription,
                    accountId,
                    It.IsAny<List<Guid>>(),
                    It.IsAny<DateTime?>()))
                .ReturnsAsync(secret);

            _fixture.FunctionServiceMock
                .Setup(x => x.CreateFunctionAsync(
                    functionName,
                    functionDescription,
                    FunctionRuntime.CSharp,
                    sourceCode,
                    entryPoint,
                    accountId,
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<List<Guid>>(),
                    It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(function);

            _fixture.SecretsServiceMock
                .Setup(x => x.UpdateAllowedFunctionsAsync(
                    secretId,
                    It.Is<List<Guid>>(list => list.Contains(functionId))))
                .ReturnsAsync(secretWithFunction);

            _fixture.SecretsServiceMock
                .Setup(x => x.HasAccessAsync(secretId, functionId))
                .ReturnsAsync(true);

            _fixture.SecretsServiceMock
                .Setup(x => x.GetSecretValueAsync(secretId, functionId))
                .ReturnsAsync("decrypted-secret-value");

            // Act - Create account
            var createdAccount = await _accountService.RegisterAsync(username, email, password);

            // Assert - Account created successfully
            Assert.NotNull(createdAccount);
            Assert.Equal(username, createdAccount.Username);
            Assert.Equal(email, createdAccount.Email);

            // Act - Create secret
            var createdSecret = await _secretsService.CreateSecretAsync(
                secretName,
                secretValue,
                secretDescription,
                accountId,
                new List<Guid>());

            // Assert - Secret created successfully
            Assert.NotNull(createdSecret);
            Assert.Equal(secretName, createdSecret.Name);
            Assert.Equal(secretDescription, createdSecret.Description);
            Assert.Equal(accountId, createdSecret.AccountId);
            Assert.Empty(createdSecret.AllowedFunctionIds);

            // Act - Create function
            var createdFunction = await _functionService.CreateFunctionAsync(
                functionName,
                functionDescription,
                FunctionRuntime.CSharp,
                sourceCode,
                entryPoint,
                accountId);

            // Assert - Function created successfully
            Assert.NotNull(createdFunction);
            Assert.Equal(functionName, createdFunction.Name);
            Assert.Equal(accountId, createdFunction.AccountId);

            // Act - Update secret to allow function access
            var updatedSecret = await _secretsService.UpdateAllowedFunctionsAsync(
                secretId,
                new List<Guid> { functionId });

            // Assert - Secret updated successfully
            Assert.NotNull(updatedSecret);
            Assert.Single(updatedSecret.AllowedFunctionIds);
            Assert.Contains(functionId, updatedSecret.AllowedFunctionIds);

            // Act - Check if function has access to secret
            var hasAccess = await _secretsService.HasAccessAsync(secretId, functionId);

            // Assert - Function has access to secret
            Assert.True(hasAccess);

            // Act - Get secret value
            var retrievedValue = await _secretsService.GetSecretValueAsync(secretId, functionId);

            // Assert - Secret value retrieved successfully
            Assert.NotNull(retrievedValue);
            Assert.Equal("decrypted-secret-value", retrievedValue);
        }

        [Fact]
        public async Task AccessSecretWithoutPermission_ReturnsFalse()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var secretId = Guid.NewGuid();
            var functionId = Guid.NewGuid();
            var secretName = "api-key";
            var secretValue = "secret-value-123";
            var functionName = "test-function";

            // Setup mocks
            _fixture.SecretsServiceMock
                .Setup(x => x.HasAccessAsync(secretId, functionId))
                .ReturnsAsync(false);

            _fixture.SecretsServiceMock
                .Setup(x => x.GetSecretValueAsync(secretId, functionId))
                .ThrowsAsync(new SecretsException("Function does not have access to this secret"));

            // Act - Check if function has access to secret (without explicitly granting access)
            var hasAccess = await _secretsService.HasAccessAsync(secretId, functionId);

            // Assert - Function does not have access to secret
            Assert.False(hasAccess);

            // Act & Assert - Try to get secret value without access
            var exception = await Assert.ThrowsAsync<SecretsException>(() =>
                _secretsService.GetSecretValueAsync(secretId, functionId));

            Assert.Contains("Function does not have access to this secret", exception.Message);
        }

        [Fact]
        public async Task CreateExpiredSecret_AccessDenied()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var secretId = Guid.NewGuid();
            var functionId = Guid.NewGuid();
            var secretName = "api-key";
            var secretValue = "secret-value-123";
            var expiresAt = DateTime.UtcNow.AddDays(-1); // Expired yesterday

            var expiredSecret = new Secret
            {
                Id = secretId,
                Name = secretName,
                EncryptedValue = "encrypted:" + secretValue,
                Version = 1,
                AccountId = accountId,
                AllowedFunctionIds = new List<Guid> { functionId },
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2),
                ExpiresAt = expiresAt
            };

            // Setup mocks
            _fixture.SecretsServiceMock
                .Setup(x => x.CreateSecretAsync(
                    secretName,
                    secretValue,
                    It.IsAny<string>(),
                    accountId,
                    It.Is<List<Guid>>(list => list.Contains(functionId)),
                    expiresAt))
                .ReturnsAsync(expiredSecret);

            _fixture.SecretsServiceMock
                .Setup(x => x.HasAccessAsync(secretId, functionId))
                .ReturnsAsync(false);

            _fixture.SecretsServiceMock
                .Setup(x => x.GetSecretValueAsync(secretId, functionId))
                .ThrowsAsync(new SecretsException("Secret has expired"));

            // Act - Create expired secret
            var createdSecret = await _secretsService.CreateSecretAsync(
                secretName,
                secretValue,
                "API key for external service",
                accountId,
                new List<Guid> { functionId },
                expiresAt);

            // Assert - Secret created successfully but is expired
            Assert.NotNull(createdSecret);
            Assert.Equal(expiresAt, createdSecret.ExpiresAt);

            // Act - Check if function has access to expired secret
            var hasAccess = await _secretsService.HasAccessAsync(secretId, functionId);

            // Assert - Function does not have access to expired secret
            Assert.False(hasAccess);

            // Act & Assert - Try to get expired secret value
            var exception = await Assert.ThrowsAsync<SecretsException>(() =>
                _secretsService.GetSecretValueAsync(secretId, functionId));

            Assert.Contains("Secret has expired", exception.Message);
        }

        [Fact]
        public async Task RotateSecret_Success()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var secretId = Guid.NewGuid();
            var secretName = "api-key";
            var secretValue = "secret-value-123";
            var newSecretValue = "new-secret-value-456";

            var secret = new Secret
            {
                Id = secretId,
                Name = secretName,
                EncryptedValue = "encrypted:" + secretValue,
                Version = 1,
                AccountId = accountId,
                AllowedFunctionIds = new List<Guid>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var rotatedSecret = new Secret
            {
                Id = secretId,
                Name = secretName,
                EncryptedValue = "encrypted:" + newSecretValue,
                Version = 2, // Version incremented
                AccountId = accountId,
                AllowedFunctionIds = new List<Guid>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Setup mocks
            _fixture.SecretsServiceMock
                .Setup(x => x.CreateSecretAsync(
                    secretName,
                    secretValue,
                    It.IsAny<string>(),
                    accountId,
                    It.IsAny<List<Guid>>(),
                    It.IsAny<DateTime?>()))
                .ReturnsAsync(secret);

            _fixture.SecretsServiceMock
                .Setup(x => x.RotateSecretAsync(secretId, newSecretValue))
                .ReturnsAsync(rotatedSecret);

            // Act - Create secret
            var createdSecret = await _secretsService.CreateSecretAsync(
                secretName,
                secretValue,
                "API key for external service",
                accountId,
                new List<Guid>());

            // Assert - Initial version is 1
            Assert.Equal(1, createdSecret.Version);

            // Act - Rotate secret
            var updatedSecret = await _secretsService.RotateSecretAsync(secretId, newSecretValue);

            // Assert - Secret rotated successfully
            Assert.NotNull(updatedSecret);
            Assert.Equal(secretId, updatedSecret.Id);
            Assert.Equal(2, updatedSecret.Version); // Version incremented
        }
    }
}
