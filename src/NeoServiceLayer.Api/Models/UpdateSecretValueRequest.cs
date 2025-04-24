using System.ComponentModel.DataAnnotations;

namespace NeoServiceLayer.Api.Models
{
    /// <summary>
    /// Request model for updating a secret's value
    /// </summary>
    public class UpdateSecretValueRequest
    {
        /// <summary>
        /// New value for the secret
        /// </summary>
        [Required]
        public string Value { get; set; }
    }
}
