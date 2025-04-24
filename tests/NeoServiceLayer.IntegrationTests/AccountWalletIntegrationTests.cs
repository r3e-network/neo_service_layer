using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NeoServiceLayer.Core.Exceptions;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.IntegrationTests.TestFixtures;
using Xunit;

namespace NeoServiceLayer.IntegrationTests
{
    public class AccountWalletIntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly IntegrationTestFixture _fixture;
        private readonly IAccountService _accountService;
        private readonly IWalletService _walletService;

        public AccountWalletIntegrationTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
            _accountService = _fixture.ServiceProvider.GetRequiredService<IAccountService>();
            _walletService = _fixture.ServiceProvider.GetRequiredService<IWalletService>();
        }

        [Fact]
        public async Task CreateAccountAndWallet_Success()
        {
            // Arrange
            var username = $"testuser_{Guid.NewGuid()}";
            var email = $"test_{Guid.NewGuid()}@example.com";
            var password = "Password123!";
            var neoAddress = $"NeoAddress_{Guid.NewGuid()}";

            // Act - Create account
            var account = await _accountService.RegisterAsync(username, email, password, neoAddress);
            
            // Assert - Account created successfully
            Assert.NotNull(account);
            Assert.Equal(username, account.Username);
            Assert.Equal(email, account.Email);
            Assert.Equal(neoAddress, account.NeoAddress);
            Assert.False(account.IsVerified);
            Assert.Equal(0, account.Credits);

            // Act - Verify account
            var verificationResult = await _accountService.VerifyAccountAsync(account.Id);
            
            // Assert - Account verified successfully
            Assert.True(verificationResult);
            
            // Get the account again to check if it's verified
            var verifiedAccount = await _accountService.GetByIdAsync(account.Id);
            Assert.True(verifiedAccount.IsVerified);

            // Act - Create wallet for the account
            var wallet = await _walletService.CreateWalletAsync(
                $"Wallet_{Guid.NewGuid()}",
                $"NeoAddress_{Guid.NewGuid()}",
                $"0x{Guid.NewGuid():N}",
                "PublicKey123",
                "EncryptedPrivateKey123",
                "WIF123",
                account.Id,
                false);
            
            // Assert - Wallet created successfully
            Assert.NotNull(wallet);
            Assert.Equal(account.Id, wallet.AccountId);
            Assert.False(wallet.IsServiceWallet);

            // Act - Get wallets for the account
            var wallets = await _walletService.GetWalletsByAccountAsync(account.Id);
            
            // Assert - Wallet retrieved successfully
            Assert.NotEmpty(wallets);
            Assert.Contains(wallets, w => w.Id == wallet.Id);
        }

        [Fact]
        public async Task CreateAccountAndAddCredits_Success()
        {
            // Arrange
            var username = $"testuser_{Guid.NewGuid()}";
            var email = $"test_{Guid.NewGuid()}@example.com";
            var password = "Password123!";
            var neoAddress = $"NeoAddress_{Guid.NewGuid()}";
            var initialCredits = 100.0m;
            var creditsToAdd = 50.0m;

            // Act - Create account
            var account = await _accountService.RegisterAsync(username, email, password, neoAddress);
            
            // Assert - Account created successfully
            Assert.NotNull(account);
            Assert.Equal(0, account.Credits);

            // Act - Add initial credits
            var accountWithCredits = await _accountService.AddCreditsAsync(account.Id, initialCredits);
            
            // Assert - Credits added successfully
            Assert.Equal(initialCredits, accountWithCredits.Credits);

            // Act - Add more credits
            var accountWithMoreCredits = await _accountService.AddCreditsAsync(account.Id, creditsToAdd);
            
            // Assert - More credits added successfully
            Assert.Equal(initialCredits + creditsToAdd, accountWithMoreCredits.Credits);

            // Act - Deduct credits
            var accountAfterDeduction = await _accountService.DeductCreditsAsync(account.Id, creditsToAdd);
            
            // Assert - Credits deducted successfully
            Assert.Equal(initialCredits, accountAfterDeduction.Credits);
        }

        [Fact]
        public async Task CreateDuplicateAccount_ThrowsException()
        {
            // Arrange
            var username = $"testuser_{Guid.NewGuid()}";
            var email = $"test_{Guid.NewGuid()}@example.com";
            var password = "Password123!";
            var neoAddress = $"NeoAddress_{Guid.NewGuid()}";

            // Act - Create first account
            var account = await _accountService.RegisterAsync(username, email, password, neoAddress);
            
            // Assert - Account created successfully
            Assert.NotNull(account);

            // Act & Assert - Try to create account with same username
            var exception1 = await Assert.ThrowsAsync<AccountException>(() => 
                _accountService.RegisterAsync(username, $"different_{Guid.NewGuid()}@example.com", password));
            
            Assert.Contains("Username already exists", exception1.Message);

            // Act & Assert - Try to create account with same email
            var exception2 = await Assert.ThrowsAsync<AccountException>(() => 
                _accountService.RegisterAsync($"different_{Guid.NewGuid()}", email, password));
            
            Assert.Contains("Email already exists", exception2.Message);
        }

        [Fact]
        public async Task DeductTooManyCredits_ThrowsException()
        {
            // Arrange
            var username = $"testuser_{Guid.NewGuid()}";
            var email = $"test_{Guid.NewGuid()}@example.com";
            var password = "Password123!";
            var neoAddress = $"NeoAddress_{Guid.NewGuid()}";
            var initialCredits = 30.0m;
            var creditsToDeduct = 50.0m;

            // Act - Create account and add credits
            var account = await _accountService.RegisterAsync(username, email, password, neoAddress);
            await _accountService.AddCreditsAsync(account.Id, initialCredits);
            
            // Act & Assert - Try to deduct too many credits
            var exception = await Assert.ThrowsAsync<AccountException>(() => 
                _accountService.DeductCreditsAsync(account.Id, creditsToDeduct));
            
            Assert.Contains("Insufficient credits", exception.Message);
        }
    }
}
