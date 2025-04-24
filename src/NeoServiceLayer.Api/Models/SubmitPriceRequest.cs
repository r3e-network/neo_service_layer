using System.ComponentModel.DataAnnotations;

namespace NeoServiceLayer.Api.Models
{
    /// <summary>
    /// Request model for submitting a price to the oracle
    /// </summary>
    public class SubmitPriceRequest
    {
        /// <summary>
        /// Gets or sets the asset symbol (e.g., "BTC", "NEO", "GAS")
        /// </summary>
        [Required]
        public string Symbol { get; set; }

        /// <summary>
        /// Gets or sets the base currency (e.g., "USD", "EUR")
        /// </summary>
        [Required]
        public string BaseCurrency { get; set; } = "USD";

        /// <summary>
        /// Gets or sets the price value
        /// </summary>
        [Required]
        [Range(0.00000001, double.MaxValue)]
        public decimal Value { get; set; }
    }
}
