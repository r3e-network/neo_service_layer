using System;
using System.Collections.Generic;
using NeoServiceLayer.Core.Models;
using Xunit;

namespace NeoServiceLayer.CoreTests.CoreModelsTests
{
    public class AccountModelTests
    {
        [Fact]
        public void Account_Properties_Work()
        {
            // Arrange
            var account = new Account
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                PasswordSalt = "salt",
                NeoAddress = "NeoAddress123",
                IsVerified = false,
                Credits = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Act & Assert
            Assert.Equal("testuser", account.Username);
            Assert.Equal("test@example.com", account.Email);
            Assert.Equal("hashedpassword", account.PasswordHash);
            Assert.Equal("salt", account.PasswordSalt);
            Assert.Equal("NeoAddress123", account.NeoAddress);
            Assert.False(account.IsVerified);
            Assert.Equal(0, account.Credits);
        }

        // AccountVerification and AccountSession models are not defined in the Core project yet
    }
}
