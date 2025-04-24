using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NeoServiceLayer.Core.Exceptions;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.IntegrationTests.TestFixtures;
using Xunit;

namespace NeoServiceLayer.IntegrationTests
{
    public class SecretsFunctionIntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly IntegrationTestFixture _fixture;
        private readonly IAccountService _accountService;
        private readonly ISecretsService _secretsService;
        private readonly IFunctionService _functionService;

        public SecretsFunctionIntegrationTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
            _accountService = _fixture.ServiceProvider.GetRequiredService<IAccountService>();
            _secretsService = _fixture.ServiceProvider.GetRequiredService<ISecretsService>();
            _functionService = _fixture.ServiceProvider.GetRequiredService<IFunctionService>();
        }

        [Fact]
        public async Task CreateSecretAndFunction_ThenAccessSecret_Success()
        {
            // Arrange - Create account
            var username = $"testuser_{Guid.NewGuid()}";
            var email = $"test_{Guid.NewGuid()}@example.com";
            var password = "Password123!";
            var account = await _accountService.RegisterAsync(username, email, password);
            
            // Act - Create secret
            var secretName = $"api-key-{Guid.NewGuid()}";
            var secretValue = "secret-value-123";
            var secretDescription = "API key for external service";
            var secret = await _secretsService.CreateSecretAsync(
                secretName,
                secretValue,
                secretDescription,
                account.Id);
            
            // Assert - Secret created successfully
            Assert.NotNull(secret);
            Assert.Equal(secretName, secret.Name);
            Assert.Equal(secretDescription, secret.Description);
            Assert.Equal(account.Id, secret.AccountId);
            Assert.Empty(secret.AllowedFunctionIds);

            // Act - Create function
            var functionName = $"test-function-{Guid.NewGuid()}";
            var function = await _functionService.CreateFunctionAsync(
                functionName,
                "Test function",
                "dotnet",
                "TestFunction::Handler.Process",
                "public class Handler { public static object Process(object input) { return input; } }",
                "Handler.Process",
                account.Id);
            
            // Assert - Function created successfully
            Assert.NotNull(function);
            Assert.Equal(functionName, function.Name);
            Assert.Equal(account.Id, function.AccountId);

            // Act - Update secret to allow function access
            var updatedSecret = await _secretsService.UpdateAllowedFunctionsAsync(
                secret.Id,
                new List<Guid> { function.Id });
            
            // Assert - Secret updated successfully
            Assert.NotNull(updatedSecret);
            Assert.Single(updatedSecret.AllowedFunctionIds);
            Assert.Contains(function.Id, updatedSecret.AllowedFunctionIds);

            // Act - Check if function has access to secret
            var hasAccess = await _secretsService.HasAccessAsync(secret.Id, function.Id);
            
            // Assert - Function has access to secret
            Assert.True(hasAccess);

            // Act - Get secret value
            var retrievedValue = await _secretsService.GetSecretValueAsync(secret.Id, function.Id);
            
            // Assert - Secret value retrieved successfully
            Assert.NotNull(retrievedValue);
            Assert.Equal("decrypted-secret-value", retrievedValue); // Mock value from MockEnclaveService
        }

        [Fact]
        public async Task AccessSecretWithoutPermission_ReturnsFalse()
        {
            // Arrange - Create account
            var username = $"testuser_{Guid.NewGuid()}";
            var email = $"test_{Guid.NewGuid()}@example.com";
            var password = "Password123!";
            var account = await _accountService.RegisterAsync(username, email, password);
            
            // Create secret
            var secretName = $"api-key-{Guid.NewGuid()}";
            var secretValue = "secret-value-123";
            var secret = await _secretsService.CreateSecretAsync(
                secretName,
                secretValue,
                "API key for external service",
                account.Id);
            
            // Create function
            var functionName = $"test-function-{Guid.NewGuid()}";
            var function = await _functionService.CreateFunctionAsync(
                functionName,
                "Test function",
                "dotnet",
                "TestFunction::Handler.Process",
                "public class Handler { public static object Process(object input) { return input; } }",
                "Handler.Process",
                account.Id);
            
            // Act - Check if function has access to secret (without explicitly granting access)
            var hasAccess = await _secretsService.HasAccessAsync(secret.Id, function.Id);
            
            // Assert - Function does not have access to secret
            Assert.False(hasAccess);

            // Act & Assert - Try to get secret value without access
            await Assert.ThrowsAsync<SecretsException>(() => 
                _secretsService.GetSecretValueAsync(secret.Id, function.Id));
        }

        [Fact]
        public async Task CreateExpiredSecret_AccessDenied()
        {
            // Arrange - Create account
            var username = $"testuser_{Guid.NewGuid()}";
            var email = $"test_{Guid.NewGuid()}@example.com";
            var password = "Password123!";
            var account = await _accountService.RegisterAsync(username, email, password);
            
            // Create function
            var functionName = $"test-function-{Guid.NewGuid()}";
            var function = await _functionService.CreateFunctionAsync(
                functionName,
                "Test function",
                "dotnet",
                "TestFunction::Handler.Process",
                "public class Handler { public static object Process(object input) { return input; } }",
                "Handler.Process",
                account.Id);
            
            // Create expired secret
            var secretName = $"api-key-{Guid.NewGuid()}";
            var secretValue = "secret-value-123";
            var expiresAt = DateTime.UtcNow.AddDays(-1); // Expired yesterday
            var secret = await _secretsService.CreateSecretAsync(
                secretName,
                secretValue,
                "API key for external service",
                account.Id,
                new List<Guid> { function.Id },
                expiresAt);
            
            // Act - Check if function has access to expired secret
            var hasAccess = await _secretsService.HasAccessAsync(secret.Id, function.Id);
            
            // Assert - Function does not have access to expired secret
            Assert.False(hasAccess);

            // Act & Assert - Try to get expired secret value
            await Assert.ThrowsAsync<SecretsException>(() => 
                _secretsService.GetSecretValueAsync(secret.Id, function.Id));
        }

        [Fact]
        public async Task RotateSecret_Success()
        {
            // Arrange - Create account
            var username = $"testuser_{Guid.NewGuid()}";
            var email = $"test_{Guid.NewGuid()}@example.com";
            var password = "Password123!";
            var account = await _accountService.RegisterAsync(username, email, password);
            
            // Create secret
            var secretName = $"api-key-{Guid.NewGuid()}";
            var secretValue = "secret-value-123";
            var secret = await _secretsService.CreateSecretAsync(
                secretName,
                secretValue,
                "API key for external service",
                account.Id);
            
            // Assert - Initial version is 1
            Assert.Equal(1, secret.Version);

            // Act - Rotate secret
            var newSecretValue = "new-secret-value-456";
            var rotatedSecret = await _secretsService.RotateSecretAsync(secret.Id, newSecretValue);
            
            // Assert - Secret rotated successfully
            Assert.NotNull(rotatedSecret);
            Assert.Equal(secret.Id, rotatedSecret.Id);
            Assert.Equal(2, rotatedSecret.Version); // Version incremented
        }
    }
}
