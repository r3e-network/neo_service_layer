using System;
using NeoServiceLayer.Core.Models;
using Xunit;

namespace NeoServiceLayer.CoreModelsTests
{
    public class AccountModelsTests
    {
        [Fact]
        public void Account_Properties_Work()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var createdAt = DateTime.UtcNow;
            var updatedAt = createdAt.AddHours(1);
            
            var account = new Account
            {
                Id = accountId,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashedpassword123",
                PasswordSalt = "salt123",
                NeoAddress = "NeoAddress123",
                IsVerified = true,
                IsActive = true,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
                Credits = 100.50m
            };

            // Act & Assert
            Assert.Equal(accountId, account.Id);
            Assert.Equal("testuser", account.Username);
            Assert.Equal("test@example.com", account.Email);
            Assert.Equal("hashedpassword123", account.PasswordHash);
            Assert.Equal("salt123", account.PasswordSalt);
            Assert.Equal("NeoAddress123", account.NeoAddress);
            Assert.True(account.IsVerified);
            Assert.True(account.IsActive);
            Assert.Equal(createdAt, account.CreatedAt);
            Assert.Equal(updatedAt, account.UpdatedAt);
            Assert.Equal(100.50m, account.Credits);
        }
    }
}
