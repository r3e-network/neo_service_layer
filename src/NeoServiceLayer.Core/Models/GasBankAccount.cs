using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents a GasBank account
    /// </summary>
    public class GasBankAccount
    {
        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the account ID
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the balance
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// Gets or sets the allocated amount
        /// </summary>
        public decimal AllocatedAmount { get; set; }

        /// <summary>
        /// Gets or sets the wallet ID
        /// </summary>
        public Guid WalletId { get; set; }

        /// <summary>
        /// Gets or sets the Neo address
        /// </summary>
        public string NeoAddress { get; set; }

        /// <summary>
        /// Gets or sets the created at timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the updated at timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the tags
        /// </summary>
        public Dictionary<string, string> Tags { get; set; }
    }
}
