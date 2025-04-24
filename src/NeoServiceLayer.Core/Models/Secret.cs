using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents a secret in the Neo Service Layer
    /// </summary>
    public class Secret
    {
        /// <summary>
        /// Unique identifier for the secret
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Name of the secret
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the secret
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Encrypted value of the secret
        /// </summary>
        public string EncryptedValue { get; set; }

        /// <summary>
        /// Version of the secret
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Account ID that owns the secret
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// List of function IDs that have access to the secret
        /// </summary>
        public List<Guid> AllowedFunctionIds { get; set; }

        /// <summary>
        /// Date and time when the secret was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date and time when the secret was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Date and time when the secret expires
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    }
}
