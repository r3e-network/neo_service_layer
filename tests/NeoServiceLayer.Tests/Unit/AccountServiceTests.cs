using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NeoServiceLayer.Core;
using NeoServiceLayer.Core.Exceptions;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Services.Account;
using NeoServiceLayer.Services.Account.Repositories;
using Xunit;

namespace NeoServiceLayer.Tests.Unit
{
    public class AccountServiceTests
    {
        private readonly Mock<ILogger<AccountService>> _loggerMock;
        private readonly Mock<IAccountRepository> _accountRepositoryMock;
        private readonly Mock<IEnclaveService> _enclaveServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly AccountService _accountService;

        public AccountServiceTests()
        {
            _loggerMock = new Mock<ILogger<AccountService>>();
            _accountRepositoryMock = new Mock<IAccountRepository>();
            _enclaveServiceMock = new Mock<IEnclaveService>();
            _configurationMock = new Mock<IConfiguration>();

            // Setup configuration for JWT
            var configSection = new Mock<IConfigurationSection>();
            configSection.Setup(x => x.Value).Returns("test-jwt-secret-key-that-is-long-enough-for-testing");
            _configurationMock.Setup(x => x.GetSection("JwtSettings:Secret")).Returns(configSection.Object);

            _accountService = new AccountService(
                _loggerMock.Object,
                _configurationMock.Object,
                _accountRepositoryMock.Object,
                _enclaveServiceMock.Object);
        }

        [Fact]
        public async Task RegisterAsync_ValidInput_ReturnsCreatedAccount()
        {
            // Arrange
            var username = "testuser";
            var email = "test@example.com";
            var password = "Password123!";
            var neoAddress = "N4wBJgJnYvGYiZCYHgYJfvXkUEFedE1xFz";

            var createdAccount = new Account
            {
                Id = Guid.NewGuid(),
                Username = username,
                Email = email,
                PasswordHash = "hashedpassword",
                PasswordSalt = "salt",
                NeoAddress = neoAddress,
                IsVerified = false,
                Credits = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _accountRepositoryMock
                .Setup(x => x.GetByUsernameAsync(username))
                .ReturnsAsync((Account?)null);

            _accountRepositoryMock
                .Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync((Account?)null);

            _enclaveServiceMock
                .Setup(x => x.SendRequestAsync<object, Account>(
                    It.Is<string>(s => s == Core.Constants.EnclaveServiceTypes.Account),
                    It.Is<string>(s => s == Core.Constants.AccountOperations.Register),
                    It.IsAny<object>()))
                .ReturnsAsync(createdAccount);

            _accountRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Account>()))
                .ReturnsAsync(createdAccount);

            // Act
            var result = await _accountService.RegisterAsync(username, email, password, neoAddress);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createdAccount.Id, result.Id);
            Assert.Equal(username, result.Username);
            Assert.Equal(email, result.Email);
            Assert.Equal(neoAddress, result.NeoAddress);
            Assert.False(result.IsVerified);
            Assert.Equal(0, result.Credits);

            _accountRepositoryMock.Verify(x => x.GetByUsernameAsync(username), Times.Once);
            _accountRepositoryMock.Verify(x => x.GetByEmailAsync(email), Times.Once);
            _enclaveServiceMock.Verify(
                x => x.SendRequestAsync<object, Account>(
                    It.Is<string>(s => s == Core.Constants.EnclaveServiceTypes.Account),
                    It.Is<string>(s => s == Core.Constants.AccountOperations.Register),
                    It.IsAny<object>()),
                Times.Once);
            _accountRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Account>()), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_DuplicateUsername_ThrowsException()
        {
            // Arrange
            var username = "testuser";
            var email = "test@example.com";
            var password = "Password123!";

            var existingAccount = new Account
            {
                Id = Guid.NewGuid(),
                Username = username,
                Email = "other@example.com",
                PasswordHash = "hashedpassword",
                PasswordSalt = "salt",
                IsVerified = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _accountRepositoryMock
                .Setup(x => x.GetByUsernameAsync(username))
                .ReturnsAsync(existingAccount);

            // Act & Assert
            await Assert.ThrowsAsync<AccountException>(() =>
                _accountService.RegisterAsync(username, email, password));

            _accountRepositoryMock.Verify(x => x.GetByUsernameAsync(username), Times.Once);
            _accountRepositoryMock.Verify(x => x.GetByEmailAsync(email), Times.Never);
            _enclaveServiceMock.Verify(
                x => x.SendRequestAsync<object, Account>(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()),
                Times.Never);
            _accountRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Account>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_DuplicateEmail_ThrowsException()
        {
            // Arrange
            var username = "testuser";
            var email = "test@example.com";
            var password = "Password123!";

            var existingAccount = new Account
            {
                Id = Guid.NewGuid(),
                Username = "otheruser",
                Email = email,
                PasswordHash = "hashedpassword",
                PasswordSalt = "salt",
                IsVerified = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _accountRepositoryMock
                .Setup(x => x.GetByUsernameAsync(username))
                .ReturnsAsync((Account?)null);

            _accountRepositoryMock
                .Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(existingAccount);

            // Act & Assert
            await Assert.ThrowsAsync<AccountException>(() =>
                _accountService.RegisterAsync(username, email, password));

            _accountRepositoryMock.Verify(x => x.GetByUsernameAsync(username), Times.Once);
            _accountRepositoryMock.Verify(x => x.GetByEmailAsync(email), Times.Once);
            _enclaveServiceMock.Verify(
                x => x.SendRequestAsync<object, Account>(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()),
                Times.Never);
            _accountRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Account>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingAccount_ReturnsAccount()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new Account
            {
                Id = accountId,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                PasswordSalt = "salt",
                IsVerified = true,
                Credits = 100,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _accountRepositoryMock
                .Setup(x => x.GetByIdAsync(accountId))
                .ReturnsAsync(account);

            // Act
            var result = await _accountService.GetByIdAsync(accountId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(accountId, result.Id);
            Assert.Equal(account.Username, result.Username);
            Assert.Equal(account.Email, result.Email);
            Assert.Equal(account.IsVerified, result.IsVerified);
            Assert.Equal(account.Credits, result.Credits);

            _accountRepositoryMock.Verify(x => x.GetByIdAsync(accountId), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingAccount_ReturnsNull()
        {
            // Arrange
            var accountId = Guid.NewGuid();

            _accountRepositoryMock
                .Setup(x => x.GetByIdAsync(accountId))
                .ReturnsAsync((Account?)null);

            // Act
            var result = await _accountService.GetByIdAsync(accountId);

            // Assert
            Assert.Null(result);
            _accountRepositoryMock.Verify(x => x.GetByIdAsync(accountId), Times.Once);
        }

        [Fact]
        public async Task AddCreditsAsync_ExistingAccount_ReturnsUpdatedAccount()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var initialCredits = 100m;
            var creditsToAdd = 50m;

            var account = new Account
            {
                Id = accountId,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                PasswordSalt = "salt",
                IsVerified = true,
                Credits = initialCredits,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var updatedAccount = new Account
            {
                Id = accountId,
                Username = account.Username,
                Email = account.Email,
                PasswordHash = account.PasswordHash,
                PasswordSalt = account.PasswordSalt,
                IsVerified = account.IsVerified,
                Credits = initialCredits + creditsToAdd,
                CreatedAt = account.CreatedAt,
                UpdatedAt = DateTime.UtcNow
            };

            _accountRepositoryMock
                .Setup(x => x.GetByIdAsync(accountId))
                .ReturnsAsync(account);

            _accountRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<Account>()))
                .ReturnsAsync(updatedAccount);

            // Act
            var result = await _accountService.AddCreditsAsync(accountId, creditsToAdd);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(accountId, result.Id);
            Assert.Equal(initialCredits + creditsToAdd, result.Credits);

            _accountRepositoryMock.Verify(x => x.GetByIdAsync(accountId), Times.Once);
            _accountRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Account>()), Times.Once);
        }

        [Fact]
        public async Task DeductCreditsAsync_ExistingAccountWithSufficientCredits_ReturnsUpdatedAccount()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var initialCredits = 100m;
            var creditsToDeduct = 50m;

            var account = new Account
            {
                Id = accountId,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                PasswordSalt = "salt",
                IsVerified = true,
                Credits = initialCredits,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var updatedAccount = new Account
            {
                Id = accountId,
                Username = account.Username,
                Email = account.Email,
                PasswordHash = account.PasswordHash,
                PasswordSalt = account.PasswordSalt,
                IsVerified = account.IsVerified,
                Credits = initialCredits - creditsToDeduct,
                CreatedAt = account.CreatedAt,
                UpdatedAt = DateTime.UtcNow
            };

            _accountRepositoryMock
                .Setup(x => x.GetByIdAsync(accountId))
                .ReturnsAsync(account);

            _accountRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<Account>()))
                .ReturnsAsync(updatedAccount);

            // Act
            var result = await _accountService.DeductCreditsAsync(accountId, creditsToDeduct);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(accountId, result.Id);
            Assert.Equal(initialCredits - creditsToDeduct, result.Credits);

            _accountRepositoryMock.Verify(x => x.GetByIdAsync(accountId), Times.Once);
            _accountRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Account>()), Times.Once);
        }

        [Fact]
        public async Task DeductCreditsAsync_ExistingAccountWithInsufficientCredits_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var initialCredits = 30m;
            var creditsToDeduct = 50m;

            var account = new Account
            {
                Id = accountId,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                PasswordSalt = "salt",
                IsVerified = true,
                Credits = initialCredits,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _accountRepositoryMock
                .Setup(x => x.GetByIdAsync(accountId))
                .ReturnsAsync(account);

            // Act & Assert
            await Assert.ThrowsAsync<AccountException>(() =>
                _accountService.DeductCreditsAsync(accountId, creditsToDeduct));

            _accountRepositoryMock.Verify(x => x.GetByIdAsync(accountId), Times.Once);
            _accountRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Account>()), Times.Never);
        }
    }
}
