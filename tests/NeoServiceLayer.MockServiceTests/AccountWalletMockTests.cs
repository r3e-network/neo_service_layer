using System;
using System.Collections.Generic;
using System.Linq;
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
    public class AccountWalletMockTests : IClassFixture<MockServiceTestFixture>
    {
        private readonly MockServiceTestFixture _fixture;
        private readonly IAccountService _accountService;
        private readonly IWalletService _walletService;

        public AccountWalletMockTests(MockServiceTestFixture fixture)
        {
            _fixture = fixture;
            _accountService = _fixture.ServiceProvider.GetRequiredService<IAccountService>();
            _walletService = _fixture.ServiceProvider.GetRequiredService<IWalletService>();
        }

        [Fact]
        public async Task CreateAccountAndWallet_Success()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var walletId = Guid.NewGuid();
            var username = "testuser";
            var email = "test@example.com";
            var password = "Password123!";
            var neoAddress = "NeoAddress123";
            var walletName = "TestWallet";
            var walletAddress = "WalletAddress123";
            var scriptHash = "0x1234567890abcdef";
            var publicKey = "PublicKey123";
            var encryptedPrivateKey = "EncryptedPrivateKey123";
            var wif = "WIF123";

            var account = new Account
            {
                Id = accountId,
                Username = username,
                Email = email,
                PasswordHash = "hashedpassword",
                PasswordSalt = "salt",
                NeoAddress = neoAddress,
                IsVerified = false,
                IsActive = true,
                Credits = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var wallet = new Wallet
            {
                Id = walletId,
                Name = walletName,
                Address = walletAddress,
                ScriptHash = scriptHash,
                PublicKey = publicKey,
                EncryptedPrivateKey = encryptedPrivateKey,
                WIF = wif,
                AccountId = accountId,
                IsServiceWallet = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Setup mocks
            _fixture.AccountServiceMock
                .Setup(x => x.RegisterAsync(username, email, password, neoAddress))
                .ReturnsAsync(account);

            _fixture.AccountServiceMock
                .Setup(x => x.VerifyAccountAsync(accountId))
                .ReturnsAsync(true);

            _fixture.AccountServiceMock
                .Setup(x => x.GetByIdAsync(accountId))
                .ReturnsAsync(new Account
                {
                    Id = accountId,
                    Username = username,
                    Email = email,
                    PasswordHash = "hashedpassword",
                    PasswordSalt = "salt",
                    NeoAddress = neoAddress,
                    IsVerified = true, // Now verified
                    IsActive = true,
                    Credits = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

            _fixture.WalletServiceMock
                .Setup(x => x.CreateWalletAsync(
                    walletName,
                    "password123",
                    accountId,
                    false))
                .ReturnsAsync(wallet);

            _fixture.WalletServiceMock
                .Setup(x => x.GetByAccountIdAsync(accountId))
                .ReturnsAsync(new List<Wallet> { wallet });

            // Act - Create account
            var createdAccount = await _accountService.RegisterAsync(username, email, password, neoAddress);

            // Assert - Account created successfully
            Assert.NotNull(createdAccount);
            Assert.Equal(username, createdAccount.Username);
            Assert.Equal(email, createdAccount.Email);
            Assert.Equal(neoAddress, createdAccount.NeoAddress);
            Assert.False(createdAccount.IsVerified);

            // Act - Verify account
            var verificationResult = await _accountService.VerifyAccountAsync(accountId);

            // Assert - Account verified successfully
            Assert.True(verificationResult);

            // Get the account again to check if it's verified
            var verifiedAccount = await _accountService.GetByIdAsync(accountId);
            Assert.True(verifiedAccount.IsVerified);

            // Act - Create wallet for the account
            var createdWallet = await _walletService.CreateWalletAsync(
                walletName,
                "password123",
                accountId,
                false);

            // Assert - Wallet created successfully
            Assert.NotNull(createdWallet);
            Assert.Equal(accountId, createdWallet.AccountId);
            Assert.Equal(walletName, createdWallet.Name);
            Assert.Equal(walletAddress, createdWallet.Address);
            Assert.Equal(scriptHash, createdWallet.ScriptHash);
            Assert.False(createdWallet.IsServiceWallet);

            // Act - Get wallets for the account
            var wallets = await _walletService.GetByAccountIdAsync(accountId);

            // Assert - Wallet retrieved successfully
            Assert.NotEmpty(wallets);
            Assert.Single(wallets);
            Assert.Equal(walletId, wallets.First().Id);
        }

        [Fact]
        public async Task CreateAccountAndAddCredits_Success()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var username = "testuser";
            var email = "test@example.com";
            var password = "Password123!";
            var neoAddress = "NeoAddress123";
            var initialCredits = 100.0m;
            var creditsToAdd = 50.0m;

            var account = new Account
            {
                Id = accountId,
                Username = username,
                Email = email,
                PasswordHash = "hashedpassword",
                PasswordSalt = "salt",
                NeoAddress = neoAddress,
                IsVerified = false,
                IsActive = true,
                Credits = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var accountWithCredits = new Account
            {
                Id = accountId,
                Username = username,
                Email = email,
                PasswordHash = "hashedpassword",
                PasswordSalt = "salt",
                NeoAddress = neoAddress,
                IsVerified = false,
                IsActive = true,
                Credits = initialCredits,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var accountWithMoreCredits = new Account
            {
                Id = accountId,
                Username = username,
                Email = email,
                PasswordHash = "hashedpassword",
                PasswordSalt = "salt",
                NeoAddress = neoAddress,
                IsVerified = false,
                IsActive = true,
                Credits = initialCredits + creditsToAdd,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var accountAfterDeduction = new Account
            {
                Id = accountId,
                Username = username,
                Email = email,
                PasswordHash = "hashedpassword",
                PasswordSalt = "salt",
                NeoAddress = neoAddress,
                IsVerified = false,
                IsActive = true,
                Credits = initialCredits,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Setup mocks
            _fixture.AccountServiceMock
                .Setup(x => x.RegisterAsync(username, email, password, neoAddress))
                .ReturnsAsync(account);

            _fixture.AccountServiceMock
                .Setup(x => x.AddCreditsAsync(accountId, initialCredits))
                .ReturnsAsync(accountWithCredits);

            _fixture.AccountServiceMock
                .Setup(x => x.AddCreditsAsync(accountId, creditsToAdd))
                .ReturnsAsync(accountWithMoreCredits);

            _fixture.AccountServiceMock
                .Setup(x => x.DeductCreditsAsync(accountId, creditsToAdd))
                .ReturnsAsync(accountAfterDeduction);

            // Act - Create account
            var createdAccount = await _accountService.RegisterAsync(username, email, password, neoAddress);

            // Assert - Account created successfully
            Assert.NotNull(createdAccount);
            Assert.Equal(0, createdAccount.Credits);

            // Act - Add initial credits
            var accountWithInitialCredits = await _accountService.AddCreditsAsync(accountId, initialCredits);

            // Assert - Credits added successfully
            Assert.Equal(initialCredits, accountWithInitialCredits.Credits);

            // Act - Add more credits
            var accountWithAdditionalCredits = await _accountService.AddCreditsAsync(accountId, creditsToAdd);

            // Assert - More credits added successfully
            Assert.Equal(initialCredits + creditsToAdd, accountWithAdditionalCredits.Credits);

            // Act - Deduct credits
            var accountWithDeductedCredits = await _accountService.DeductCreditsAsync(accountId, creditsToAdd);

            // Assert - Credits deducted successfully
            Assert.Equal(initialCredits, accountWithDeductedCredits.Credits);
        }

        [Fact]
        public async Task CreateDuplicateAccount_ThrowsException()
        {
            // Arrange
            var username = "existinguser";
            var email = "existing@example.com";
            var password = "Password123!";
            var differentEmail = "different@example.com";
            var differentUsername = "differentuser";

            // Setup mocks
            _fixture.AccountServiceMock
                .Setup(x => x.RegisterAsync(username, differentEmail, password, It.IsAny<string>()))
                .ThrowsAsync(new AccountException("Username already exists"));

            _fixture.AccountServiceMock
                .Setup(x => x.RegisterAsync(differentUsername, email, password, It.IsAny<string>()))
                .ThrowsAsync(new AccountException("Email already exists"));

            // Act & Assert - Try to create account with existing username
            var exception1 = await Assert.ThrowsAsync<AccountException>(() =>
                _accountService.RegisterAsync(username, differentEmail, password));

            Assert.Contains("Username already exists", exception1.Message);

            // Act & Assert - Try to create account with existing email
            var exception2 = await Assert.ThrowsAsync<AccountException>(() =>
                _accountService.RegisterAsync(differentUsername, email, password));

            Assert.Contains("Email already exists", exception2.Message);
        }

        [Fact]
        public async Task DeductTooManyCredits_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var initialCredits = 30.0m;
            var creditsToDeduct = 50.0m;

            // Setup mocks
            _fixture.AccountServiceMock
                .Setup(x => x.DeductCreditsAsync(accountId, creditsToDeduct))
                .ThrowsAsync(new AccountException("Insufficient credits"));

            // Act & Assert - Try to deduct too many credits
            var exception = await Assert.ThrowsAsync<AccountException>(() =>
                _accountService.DeductCreditsAsync(accountId, creditsToDeduct));

            Assert.Contains("Insufficient credits", exception.Message);
        }
    }
}
