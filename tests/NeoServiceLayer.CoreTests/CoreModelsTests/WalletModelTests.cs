using System;
using System.Collections.Generic;
using NeoServiceLayer.Core.Models;
using Xunit;

namespace NeoServiceLayer.CoreTests.CoreModelsTests
{
    public class WalletModelTests
    {
        [Fact]
        public void Wallet_Properties_Work()
        {
            // Arrange
            var wallet = new Wallet
            {
                Id = Guid.NewGuid(),
                Name = "TestWallet",
                Address = "NeoAddress123",
                ScriptHash = "scripthash123",
                PublicKey = "publickey123",
                EncryptedPrivateKey = "encryptedprivatekey123",
                WIF = "wif123",
                AccountId = Guid.NewGuid(),
                IsServiceWallet = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Act & Assert
            Assert.Equal("TestWallet", wallet.Name);
            Assert.Equal("NeoAddress123", wallet.Address);
            Assert.Equal("scripthash123", wallet.ScriptHash);
            Assert.Equal("publickey123", wallet.PublicKey);
            Assert.Equal("encryptedprivatekey123", wallet.EncryptedPrivateKey);
            Assert.Equal("wif123", wallet.WIF);
            Assert.True(wallet.IsServiceWallet);
        }

        // Transaction model is not defined in the Core project yet
    }
}
