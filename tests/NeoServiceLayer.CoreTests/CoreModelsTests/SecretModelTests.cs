using System;
using System.Collections.Generic;
using NeoServiceLayer.Core.Models;
using Xunit;

namespace NeoServiceLayer.CoreTests.CoreModelsTests
{
    public class SecretModelTests
    {
        [Fact]
        public void Secret_Properties_Work()
        {
            // Arrange
            var secret = new Secret
            {
                Id = Guid.NewGuid(),
                Name = "TestSecret",
                Description = "A test secret",
                EncryptedValue = "encryptedsecretvalue",
                Version = 1,
                AccountId = Guid.NewGuid(),
                AllowedFunctionIds = new List<Guid> { Guid.NewGuid() },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            };

            // Act & Assert
            Assert.Equal("TestSecret", secret.Name);
            Assert.Equal("A test secret", secret.Description);
            Assert.Equal("encryptedsecretvalue", secret.EncryptedValue);
            Assert.Equal(1, secret.Version);
            Assert.Single(secret.AllowedFunctionIds);
            Assert.NotNull(secret.ExpiresAt);
        }
    }
}
