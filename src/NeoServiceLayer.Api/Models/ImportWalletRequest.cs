using System.ComponentModel.DataAnnotations;

namespace NeoServiceLayer.Api.Models
{
    /// <summary>
    /// Request model for wallet import
    /// </summary>
    public class ImportWalletRequest
    {
        /// <summary>
        /// WIF (Wallet Import Format) for the wallet
        /// </summary>
        [Required]
        public string WIF { get; set; }

        /// <summary>
        /// Password for the wallet
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; }

        /// <summary>
        /// Name for the wallet
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }
    }
}
