using System;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents a wallet in the Neo Service Layer
    /// </summary>
    public class Wallet
    {
        /// <summary>
        /// Unique identifier for the wallet
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Name of the wallet
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Neo N3 address for the wallet
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Script hash for the wallet
        /// </summary>
        public string ScriptHash { get; set; }

        /// <summary>
        /// Public key for the wallet
        /// </summary>
        public string PublicKey { get; set; }

        /// <summary>
        /// Encrypted private key for the wallet (NEP-2 format)
        /// </summary>
        public string EncryptedPrivateKey { get; set; }

        /// <summary>
        /// WIF (Wallet Import Format) for the wallet
        /// </summary>
        public string WIF { get; set; }

        /// <summary>
        /// Account ID associated with the wallet
        /// </summary>
        public Guid? AccountId { get; set; }

        /// <summary>
        /// Indicates whether this is a service wallet
        /// </summary>
        public bool IsServiceWallet { get; set; }

        /// <summary>
        /// Date and time when the wallet was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date and time when the wallet was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}
