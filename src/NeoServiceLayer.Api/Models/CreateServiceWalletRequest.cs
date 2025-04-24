using System.ComponentModel.DataAnnotations;

namespace NeoServiceLayer.Api.Models
{
    /// <summary>
    /// Request model for service wallet creation
    /// </summary>
    public class CreateServiceWalletRequest
    {
        /// <summary>
        /// Name for the service wallet
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        /// <summary>
        /// Password for the service wallet
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; }
    }
}
