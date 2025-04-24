using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Enclave.Enclave.Models
{
    /// <summary>
    /// Secret data
    /// </summary>
    public class Secret
    {
        /// <summary>
        /// Gets or sets the secret ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the account ID
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the secret name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the secret description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the encrypted value
        /// </summary>
        public string EncryptedValue { get; set; }

        /// <summary>
        /// Gets or sets the decrypted value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the version of the secret
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the creation date
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update date
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date when the secret expires
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Gets or sets the date when the secret was last rotated
        /// </summary>
        public DateTime? LastRotatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date when the secret should be rotated next
        /// </summary>
        public DateTime? NextRotationAt { get; set; }

        /// <summary>
        /// Gets or sets the rotation period in days
        /// </summary>
        public int? RotationPeriod { get; set; }

        /// <summary>
        /// Gets or sets the allowed function IDs
        /// </summary>
        public List<Guid> AllowedFunctionIds { get; set; }

        /// <summary>
        /// Gets or sets the tags associated with the secret
        /// </summary>
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
    }
}
