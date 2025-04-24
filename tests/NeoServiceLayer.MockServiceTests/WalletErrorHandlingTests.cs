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
    public class WalletErrorHandlingTests : IClassFixture<MockServiceTestFixture>
    {
        private readonly MockServiceTestFixture _fixture;
        private readonly IWalletService _walletService;

        public WalletErrorHandlingTests(MockServiceTestFixture fixture)
        {
            _fixture = fixture;
            _walletService = _fixture.ServiceProvider.GetRequiredService<IWalletService>();
        }

        [Fact]
        public async Task CreateWallet_EmptyName_ThrowsException()
        {
            // Arrange
            var emptyName = string.Empty;
            var password = "Password123!";
            var accountId = Guid.NewGuid();

            _fixture.WalletServiceMock
                .Setup(x => x.CreateWalletAsync(
                    It.Is<string>(s => string.IsNullOrEmpty(s)),
                    It.IsAny<string>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<bool>()))
                .ThrowsAsync(new ArgumentException("Wallet name cannot be empty"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _walletService.CreateWalletAsync(emptyName, password, accountId));
            
            Assert.Contains("cannot be empty", exception.Message);
        }

        [Fact]
        public async Task CreateWallet_EmptyPassword_ThrowsException()
        {
            // Arrange
            var name = "test-wallet";
            var emptyPassword = string.Empty;
            var accountId = Guid.NewGuid();

            _fixture.WalletServiceMock
                .Setup(x => x.CreateWalletAsync(
                    It.IsAny<string>(),
                    It.Is<string>(s => string.IsNullOrEmpty(s)),
                    It.IsAny<Guid?>(),
                    It.IsAny<bool>()))
                .ThrowsAsync(new ArgumentException("Password cannot be empty"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _walletService.CreateWalletAsync(name, emptyPassword, accountId));
            
            Assert.Contains("cannot be empty", exception.Message);
        }

        [Fact]
        public async Task CreateWallet_WeakPassword_ThrowsException()
        {
            // Arrange
            var name = "test-wallet";
            var weakPassword = "password";
            var accountId = Guid.NewGuid();

            _fixture.WalletServiceMock
                .Setup(x => x.CreateWalletAsync(
                    It.IsAny<string>(),
                    weakPassword,
                    It.IsAny<Guid?>(),
                    It.IsAny<bool>()))
                .ThrowsAsync(new ArgumentException("Password does not meet complexity requirements"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _walletService.CreateWalletAsync(name, weakPassword, accountId));
            
            Assert.Contains("complexity requirements", exception.Message);
        }

        [Fact]
        public async Task CreateWallet_DuplicateName_ThrowsException()
        {
            // Arrange
            var existingName = "existing-wallet";
            var password = "Password123!";
            var accountId = Guid.NewGuid();

            _fixture.WalletServiceMock
                .Setup(x => x.CreateWalletAsync(
                    existingName,
                    It.IsAny<string>(),
                    accountId,
                    It.IsAny<bool>()))
                .ThrowsAsync(new InvalidOperationException("A wallet with this name already exists for this account"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _walletService.CreateWalletAsync(existingName, password, accountId));
            
            Assert.Contains("already exists", exception.Message);
        }

        [Fact]
        public async Task GetById_WalletNotFound_ReturnsNull()
        {
            // Arrange
            var nonExistentWalletId = Guid.NewGuid();

            _fixture.WalletServiceMock
                .Setup(x => x.GetByIdAsync(nonExistentWalletId))
                .ReturnsAsync((Wallet)null);

            // Act
            var result = await _walletService.GetByIdAsync(nonExistentWalletId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ImportFromWIF_InvalidWIF_ThrowsException()
        {
            // Arrange
            var invalidWIF = "invalid-wif";
            var password = "Password123!";
            var name = "imported-wallet";
            var accountId = Guid.NewGuid();

            _fixture.WalletServiceMock
                .Setup(x => x.ImportFromWIFAsync(
                    invalidWIF,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<bool>()))
                .ThrowsAsync(new ArgumentException("Invalid WIF format"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _walletService.ImportFromWIFAsync(invalidWIF, password, name, accountId));
            
            Assert.Contains("Invalid WIF", exception.Message);
        }

        [Fact]
        public async Task TransferNeo_InsufficientBalance_ThrowsException()
        {
            // Arrange
            var walletId = Guid.NewGuid();
            var password = "Password123!";
            var toAddress = "NeoAddress123";
            var amount = 100.0m;

            _fixture.WalletServiceMock
                .Setup(x => x.TransferNeoAsync(
                    walletId,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    amount))
                .ThrowsAsync(new InvalidOperationException("Insufficient NEO balance"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _walletService.TransferNeoAsync(walletId, password, toAddress, amount));
            
            Assert.Contains("Insufficient", exception.Message);
        }

        [Fact]
        public async Task TransferGas_InsufficientBalance_ThrowsException()
        {
            // Arrange
            var walletId = Guid.NewGuid();
            var password = "Password123!";
            var toAddress = "NeoAddress123";
            var amount = 100.0m;

            _fixture.WalletServiceMock
                .Setup(x => x.TransferGasAsync(
                    walletId,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    amount))
                .ThrowsAsync(new InvalidOperationException("Insufficient GAS balance"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _walletService.TransferGasAsync(walletId, password, toAddress, amount));
            
            Assert.Contains("Insufficient", exception.Message);
        }

        [Fact]
        public async Task TransferToken_InvalidTokenHash_ThrowsException()
        {
            // Arrange
            var walletId = Guid.NewGuid();
            var password = "Password123!";
            var toAddress = "NeoAddress123";
            var invalidTokenHash = "invalid-token-hash";
            var amount = 100.0m;

            _fixture.WalletServiceMock
                .Setup(x => x.TransferTokenAsync(
                    walletId,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    invalidTokenHash,
                    amount))
                .ThrowsAsync(new ArgumentException("Invalid token hash format"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _walletService.TransferTokenAsync(walletId, password, toAddress, invalidTokenHash, amount));
            
            Assert.Contains("Invalid token hash", exception.Message);
        }

        [Fact]
        public async Task SignData_IncorrectPassword_ThrowsException()
        {
            // Arrange
            var walletId = Guid.NewGuid();
            var incorrectPassword = "WrongPassword123!";
            var data = new byte[] { 1, 2, 3, 4, 5 };

            _fixture.WalletServiceMock
                .Setup(x => x.SignDataAsync(
                    walletId,
                    incorrectPassword,
                    It.IsAny<byte[]>()))
                .ThrowsAsync(new InvalidOperationException("Incorrect password"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _walletService.SignDataAsync(walletId, incorrectPassword, data));
            
            Assert.Contains("Incorrect password", exception.Message);
        }
    }
}
