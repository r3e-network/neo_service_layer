using System;
using System.Collections.Generic;
using NeoServiceLayer.Core.Models;
using Xunit;

namespace NeoServiceLayer.CoreModelsTests
{
    public class WalletModelsTests
    {
        [Fact]
        public void Wallet_Properties_Work()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var wallet = new Wallet
            {
                Id = Guid.NewGuid(),
                Name = "Service Wallet",
                Address = "NeoAddress123",
                ScriptHash = "0x1234567890abcdef",
                PublicKey = "PublicKey123",
                EncryptedPrivateKey = "EncryptedPrivateKey123",
                WIF = "WIF123",
                AccountId = accountId,
                IsServiceWallet = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Act & Assert
            Assert.Equal("Service Wallet", wallet.Name);
            Assert.Equal("NeoAddress123", wallet.Address);
            Assert.Equal("0x1234567890abcdef", wallet.ScriptHash);
            Assert.Equal("PublicKey123", wallet.PublicKey);
            Assert.Equal("EncryptedPrivateKey123", wallet.EncryptedPrivateKey);
            Assert.Equal("WIF123", wallet.WIF);
            Assert.Equal(accountId, wallet.AccountId);
            Assert.True(wallet.IsServiceWallet);
        }

        // WalletTransaction model is not defined in the Core project yet
    }
}
