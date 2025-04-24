using System.ComponentModel.DataAnnotations;

namespace NeoServiceLayer.Api.Models
{
    /// <summary>
    /// Request model for token transfer
    /// </summary>
    public class TransferRequest
    {
        /// <summary>
        /// Password for the wallet
        /// </summary>
        [Required]
        public string Password { get; set; }

        /// <summary>
        /// Destination address
        /// </summary>
        [Required]
        public string ToAddress { get; set; }

        /// <summary>
        /// Amount to transfer
        /// </summary>
        [Required]
        [Range(0.00000001, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }
    }
}
