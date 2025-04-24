using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NeoServiceLayer.Core.Exceptions;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.MockServiceTests.TestFixtures;
using Xunit;

namespace NeoServiceLayer.MockServiceTests
{
    public class AccountErrorHandlingTests : IClassFixture<MockServiceTestFixture>
    {
        private readonly MockServiceTestFixture _fixture;
        private readonly IAccountService _accountService;

        public AccountErrorHandlingTests(MockServiceTestFixture fixture)
        {
            _fixture = fixture;
            _accountService = _fixture.ServiceProvider.GetRequiredService<IAccountService>();
        }

        [Fact]
        public async Task Register_EmptyUsername_ThrowsException()
        {
            // Arrange
            var emptyUsername = string.Empty;
            var email = "test@example.com";
            var password = "Password123!";

            _fixture.AccountServiceMock
                .Setup(x => x.RegisterAsync(
                    It.Is<string>(s => string.IsNullOrEmpty(s)),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException("Username cannot be empty"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _accountService.RegisterAsync(emptyUsername, email, password));
            
            Assert.Contains("cannot be empty", exception.Message);
        }

        [Fact]
        public async Task Register_EmptyEmail_ThrowsException()
        {
            // Arrange
            var username = "testuser";
            var emptyEmail = string.Empty;
            var password = "Password123!";

            _fixture.AccountServiceMock
                .Setup(x => x.RegisterAsync(
                    It.IsAny<string>(),
                    It.Is<string>(s => string.IsNullOrEmpty(s)),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException("Email cannot be empty"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _accountService.RegisterAsync(username, emptyEmail, password));
            
            Assert.Contains("cannot be empty", exception.Message);
        }

        [Fact]
        public async Task Register_EmptyPassword_ThrowsException()
        {
            // Arrange
            var username = "testuser";
            var email = "test@example.com";
            var emptyPassword = string.Empty;

            _fixture.AccountServiceMock
                .Setup(x => x.RegisterAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.Is<string>(s => string.IsNullOrEmpty(s)),
                    It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException("Password cannot be empty"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _accountService.RegisterAsync(username, email, emptyPassword));
            
            Assert.Contains("cannot be empty", exception.Message);
        }

        [Fact]
        public async Task Register_WeakPassword_ThrowsException()
        {
            // Arrange
            var username = "testuser";
            var email = "test@example.com";
            var weakPassword = "password";

            _fixture.AccountServiceMock
                .Setup(x => x.RegisterAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    weakPassword,
                    It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException("Password does not meet complexity requirements"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _accountService.RegisterAsync(username, email, weakPassword));
            
            Assert.Contains("complexity requirements", exception.Message);
        }

        [Fact]
        public async Task Register_InvalidEmail_ThrowsException()
        {
            // Arrange
            var username = "testuser";
            var invalidEmail = "invalid-email";
            var password = "Password123!";

            _fixture.AccountServiceMock
                .Setup(x => x.RegisterAsync(
                    It.IsAny<string>(),
                    invalidEmail,
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException("Invalid email format"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _accountService.RegisterAsync(username, invalidEmail, password));
            
            Assert.Contains("Invalid email", exception.Message);
        }

        [Fact]
        public async Task Register_DuplicateUsername_ThrowsException()
        {
            // Arrange
            var existingUsername = "existinguser";
            var email = "test@example.com";
            var password = "Password123!";

            _fixture.AccountServiceMock
                .Setup(x => x.RegisterAsync(
                    existingUsername,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ThrowsAsync(new AccountException("Username already exists"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AccountException>(() => 
                _accountService.RegisterAsync(existingUsername, email, password));
            
            Assert.Contains("already exists", exception.Message);
        }

        [Fact]
        public async Task Register_DuplicateEmail_ThrowsException()
        {
            // Arrange
            var username = "testuser";
            var existingEmail = "existing@example.com";
            var password = "Password123!";

            _fixture.AccountServiceMock
                .Setup(x => x.RegisterAsync(
                    It.IsAny<string>(),
                    existingEmail,
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ThrowsAsync(new AccountException("Email already exists"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AccountException>(() => 
                _accountService.RegisterAsync(username, existingEmail, password));
            
            Assert.Contains("already exists", exception.Message);
        }

        [Fact]
        public async Task GetById_AccountNotFound_ReturnsNull()
        {
            // Arrange
            var nonExistentAccountId = Guid.NewGuid();

            _fixture.AccountServiceMock
                .Setup(x => x.GetByIdAsync(nonExistentAccountId))
                .ReturnsAsync((Account)null);

            // Act
            var result = await _accountService.GetByIdAsync(nonExistentAccountId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeductCredits_InsufficientCredits_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var creditsToDeduct = 100.0m;

            _fixture.AccountServiceMock
                .Setup(x => x.DeductCreditsAsync(accountId, creditsToDeduct))
                .ThrowsAsync(new AccountException("Insufficient credits"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AccountException>(() => 
                _accountService.DeductCreditsAsync(accountId, creditsToDeduct));
            
            Assert.Contains("Insufficient credits", exception.Message);
        }

        [Fact]
        public async Task DeductCredits_NegativeAmount_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var negativeAmount = -10.0m;

            _fixture.AccountServiceMock
                .Setup(x => x.DeductCreditsAsync(accountId, negativeAmount))
                .ThrowsAsync(new ArgumentException("Amount to deduct must be positive"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _accountService.DeductCreditsAsync(accountId, negativeAmount));
            
            Assert.Contains("must be positive", exception.Message);
        }

        [Fact]
        public async Task AddCredits_NegativeAmount_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var negativeAmount = -10.0m;

            _fixture.AccountServiceMock
                .Setup(x => x.AddCreditsAsync(accountId, negativeAmount))
                .ThrowsAsync(new ArgumentException("Amount to add must be positive"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _accountService.AddCreditsAsync(accountId, negativeAmount));
            
            Assert.Contains("must be positive", exception.Message);
        }
    }
}
