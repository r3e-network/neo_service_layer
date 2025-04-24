using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NeoServiceLayer.Api.Models
{
    /// <summary>
    /// Request model for updating a function's secret access
    /// </summary>
    public class UpdateSecretAccessRequest
    {
        /// <summary>
        /// List of secret IDs that this function can access
        /// </summary>
        [Required]
        public List<Guid> SecretIds { get; set; }
    }
}
