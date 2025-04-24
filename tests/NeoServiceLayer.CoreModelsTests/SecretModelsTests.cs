using System;
using System.Collections.Generic;
using NeoServiceLayer.Core.Models;
using Xunit;

namespace NeoServiceLayer.CoreModelsTests
{
    public class SecretModelsTests
    {
        [Fact]
        public void Secret_Properties_Work()
        {
            // Arrange
            var secretId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var functionId1 = Guid.NewGuid();
            var functionId2 = Guid.NewGuid();
            var createdAt = DateTime.UtcNow;
            var updatedAt = createdAt.AddHours(1);
            var expiresAt = createdAt.AddDays(30);
            
            var secret = new Secret
            {
                Id = secretId,
                Name = "api-key",
                Description = "API key for external service",
                EncryptedValue = "encrypted:abc123",
                Version = 2,
                AccountId = accountId,
                AllowedFunctionIds = new List<Guid> { functionId1, functionId2 },
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
                ExpiresAt = expiresAt
            };

            // Act & Assert
            Assert.Equal(secretId, secret.Id);
            Assert.Equal("api-key", secret.Name);
            Assert.Equal("API key for external service", secret.Description);
            Assert.Equal("encrypted:abc123", secret.EncryptedValue);
            Assert.Equal(2, secret.Version);
            Assert.Equal(accountId, secret.AccountId);
            Assert.Equal(2, secret.AllowedFunctionIds.Count);
            Assert.Contains(functionId1, secret.AllowedFunctionIds);
            Assert.Contains(functionId2, secret.AllowedFunctionIds);
            Assert.Equal(createdAt, secret.CreatedAt);
            Assert.Equal(updatedAt, secret.UpdatedAt);
            Assert.Equal(expiresAt, secret.ExpiresAt);
        }
    }
}
