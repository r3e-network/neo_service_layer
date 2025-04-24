using System.ComponentModel.DataAnnotations;

namespace NeoServiceLayer.Api.Models
{
    /// <summary>
    /// Request model for rotating a secret's value
    /// </summary>
    public class RotateSecretRequest
    {
        /// <summary>
        /// New value for the secret
        /// </summary>
        [Required]
        public string NewValue { get; set; }
    }
}
