using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NeoServiceLayer.Api.Models
{
    /// <summary>
    /// Request model for secret creation
    /// </summary>
    public class CreateSecretRequest
    {
        /// <summary>
        /// Name for the secret
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        /// <summary>
        /// Value of the secret
        /// </summary>
        [Required]
        public string Value { get; set; }

        /// <summary>
        /// Description of the secret
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// List of function IDs that are allowed to access this secret
        /// </summary>
        [Required]
        public List<Guid> AllowedFunctionIds { get; set; }

        /// <summary>
        /// Expiration date for the secret
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    }
}
