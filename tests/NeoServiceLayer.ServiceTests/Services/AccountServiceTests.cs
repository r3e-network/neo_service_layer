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
using NeoServiceLayer.ServiceTests.Mocks;
using Xunit;

namespace NeoServiceLayer.ServiceTests.Services
{
    public class AccountServiceTests
    {
        private readonly Mock<ILogger<AccountService>> _loggerMock;
        private readonly Mock<IAccountRepository> _accountRepositoryMock;
        private readonly MockEnclaveService _enclaveService;
        private readonly IConfiguration _configuration;
        private readonly AccountService _accountService;

        public AccountServiceTests()
        {
            _loggerMock = new Mock<ILogger<AccountService>>();
            _accountRepositoryMock = new Mock<IAccountRepository>();
            _enclaveService = new MockEnclaveService();
            
            // Create configuration
            var configurationDict = new Dictionary<string, string>
            {
                { "Jwt:Secret", "test-jwt-secret-key-that-is-long-enough-for-testing" },
                { "Jwt:Issuer", "test-issuer" },
                { "Jwt:Audience", "test-audience" },
                { "Jwt:ExpiryMinutes", "60" }
            };
            
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationDict)
                .Build();

            _accountService = new AccountService(
                _loggerMock.Object,
                _configuration,
                _accountRepositoryMock.Object,
                _enclaveService);

            // Setup enclave service handlers
            _enclaveService.RegisterHandler<object, Account>(
                Constants.EnclaveServiceTypes.Account,
                Constants.AccountOperations.Register,
                request => new Account
                {
                    Id = Guid.NewGuid(),
                    Username = ((dynamic)request).Username,
                    Email = ((dynamic)request).Email,
                    PasswordHash = "hashedpassword",
                    PasswordSalt = "salt",
                    NeoAddress = ((dynamic)request).NeoAddress,
                    IsVerified = false,
                    Credits = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

            _enclaveService.RegisterHandler<object, object>(
                Constants.EnclaveServiceTypes.Account,
                Constants.AccountOperations.Authenticate,
                request => new { Success = true });

            _enclaveService.RegisterHandler<object, object>(
                Constants.EnclaveServiceTypes.Account,
                Constants.AccountOperations.ChangePassword,
                request => new { Success = true });

            _enclaveService.RegisterHandler<object, object>(
                Constants.EnclaveServiceTypes.Account,
                Constants.AccountOperations.VerifyAccount,
                request => new { Success = true });
        }

        [Fact]
        public async Task RegisterAsync_ValidInput_ReturnsCreatedAccount()
        {
            // Arrange
            var username = "testuser";
            var email = "test@example.com";
            var password = "Password123!";
            var neoAddress = "NeoAddress123";

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
                .ReturnsAsync((Account)null);

            _accountRepositoryMock
                .Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync((Account)null);

            _accountRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Account>()))
                .ReturnsAsync(createdAccount);

            // Act
            var result = await _accountService.RegisterAsync(username, email, password, neoAddress);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(username, result.Username);
            Assert.Equal(email, result.Email);
            Assert.Equal(neoAddress, result.NeoAddress);
            Assert.False(result.IsVerified);
            Assert.Equal(0, result.Credits);
        }

        [Fact]
        public async Task RegisterAsync_ExistingUsername_ThrowsAccountException()
        {
            // Arrange
            var username = "existinguser";
            var email = "test@example.com";
            var password = "Password123!";

            var existingAccount = new Account
            {
                Id = Guid.NewGuid(),
                Username = username,
                Email = "existing@example.com",
                PasswordHash = "hashedpassword",
                PasswordSalt = "salt",
                IsVerified = true,
                Credits = 100,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _accountRepositoryMock
                .Setup(x => x.GetByUsernameAsync(username))
                .ReturnsAsync(existingAccount);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AccountException>(() => 
                _accountService.RegisterAsync(username, email, password));
            
            Assert.Contains("Username already exists", exception.Message);
        }

        [Fact]
        public async Task RegisterAsync_ExistingEmail_ThrowsAccountException()
        {
            // Arrange
            var username = "testuser";
            var email = "existing@example.com";
            var password = "Password123!";

            var existingAccount = new Account
            {
                Id = Guid.NewGuid(),
                Username = "existinguser",
                Email = email,
                PasswordHash = "hashedpassword",
                PasswordSalt = "salt",
                IsVerified = true,
                Credits = 100,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _accountRepositoryMock
                .Setup(x => x.GetByUsernameAsync(username))
                .ReturnsAsync((Account)null);

            _accountRepositoryMock
                .Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(existingAccount);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AccountException>(() => 
                _accountService.RegisterAsync(username, email, password));
            
            Assert.Contains("Email already exists", exception.Message);
        }

        [Fact]
        public async Task AuthenticateAsync_ValidCredentials_ReturnsJwtToken()
        {
            // Arrange
            var username = "testuser";
            var email = "test@example.com";
            var password = "Password123!";

            var account = new Account
            {
                Id = Guid.NewGuid(),
                Username = username,
                Email = email,
                PasswordHash = "hashedpassword",
                PasswordSalt = "salt",
                IsVerified = true,
                Credits = 100,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _accountRepositoryMock
                .Setup(x => x.GetByUsernameAsync(username))
                .ReturnsAsync(account);

            // Act
            var token = await _accountService.AuthenticateAsync(username, password);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsAccount()
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
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _accountRepositoryMock
                .Setup(x => x.GetByIdAsync(accountId))
                .ReturnsAsync(account);

            // Act
            var result = await _accountService.GetByIdAsync(accountId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(accountId, result.Id);
            Assert.Equal("testuser", result.Username);
            Assert.Equal("test@example.com", result.Email);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            var accountId = Guid.NewGuid();

            _accountRepositoryMock
                .Setup(x => x.GetByIdAsync(accountId))
                .ReturnsAsync((Account)null);

            // Act
            var result = await _accountService.GetByIdAsync(accountId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ChangePasswordAsync_ValidInput_ReturnsTrue()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var currentPassword = "OldPassword123!";
            var newPassword = "NewPassword123!";

            var account = new Account
            {
                Id = accountId,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                PasswordSalt = "salt",
                IsVerified = true,
                Credits = 100,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _accountRepositoryMock
                .Setup(x => x.GetByIdAsync(accountId))
                .ReturnsAsync(account);

            // Act
            var result = await _accountService.ChangePasswordAsync(accountId, currentPassword, newPassword);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task VerifyAccountAsync_ValidInput_ReturnsTrue()
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
                IsVerified = false,
                Credits = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _accountRepositoryMock
                .Setup(x => x.GetByIdAsync(accountId))
                .ReturnsAsync(account);

            _accountRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<Account>()))
                .ReturnsAsync((Account a) => a);

            // Act
            var result = await _accountService.VerifyAccountAsync(accountId);

            // Assert
            Assert.True(result);
            _accountRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Account>(a => a.IsVerified)), Times.Once);
        }

        [Fact]
        public async Task AddCreditsAsync_ValidInput_ReturnsUpdatedAccount()
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
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _accountRepositoryMock
                .Setup(x => x.GetByIdAsync(accountId))
                .ReturnsAsync(account);

            _accountRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<Account>()))
                .ReturnsAsync((Account a) => a);

            // Act
            var result = await _accountService.AddCreditsAsync(accountId, creditsToAdd);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(initialCredits + creditsToAdd, result.Credits);
        }

        [Fact]
        public async Task DeductCreditsAsync_ValidInput_ReturnsUpdatedAccount()
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
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _accountRepositoryMock
                .Setup(x => x.GetByIdAsync(accountId))
                .ReturnsAsync(account);

            _accountRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<Account>()))
                .ReturnsAsync((Account a) => a);

            // Act
            var result = await _accountService.DeductCreditsAsync(accountId, creditsToDeduct);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(initialCredits - creditsToDeduct, result.Credits);
        }

        [Fact]
        public async Task DeductCreditsAsync_InsufficientCredits_ThrowsAccountException()
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
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _accountRepositoryMock
                .Setup(x => x.GetByIdAsync(accountId))
                .ReturnsAsync(account);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AccountException>(() => 
                _accountService.DeductCreditsAsync(accountId, creditsToDeduct));
            
            Assert.Contains("Insufficient credits", exception.Message);
        }

        [Fact]
        public async Task DeleteAsync_ExistingAccount_ReturnsTrue()
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
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _accountRepositoryMock
                .Setup(x => x.GetByIdAsync(accountId))
                .ReturnsAsync(account);

            _accountRepositoryMock
                .Setup(x => x.DeleteAsync(accountId))
                .ReturnsAsync(true);

            // Act
            var result = await _accountService.DeleteAsync(accountId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_NonExistingAccount_ThrowsAccountException()
        {
            // Arrange
            var accountId = Guid.NewGuid();

            _accountRepositoryMock
                .Setup(x => x.GetByIdAsync(accountId))
                .ReturnsAsync((Account)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AccountException>(() => 
                _accountService.DeleteAsync(accountId));
            
            Assert.Contains("Account not found", exception.Message);
        }
    }
}
