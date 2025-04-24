using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;
using Xunit;

namespace NeoServiceLayer.CoreTests
{
    public class CoreModelTests
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

        [Fact]
        public void Function_Properties_Work()
        {
            // Arrange
            var function = new Function
            {
                Id = Guid.NewGuid(),
                Name = "TestFunction",
                Description = "A test function",
                Runtime = "dotnet",
                SourceCode = "public class Test { public static void Main() { } }",
                EntryPoint = "Test.Main",
                AccountId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Act & Assert
            Assert.Equal("TestFunction", function.Name);
            Assert.Equal("A test function", function.Description);
            Assert.Equal("dotnet", function.Runtime);
            Assert.Equal("public class Test { public static void Main() { } }", function.SourceCode);
            Assert.Equal("Test.Main", function.EntryPoint);
        }

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
