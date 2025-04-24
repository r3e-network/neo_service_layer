using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Api.Models
{
    /// <summary>
    /// Request model for adding or updating a price source
    /// </summary>
    public class PriceSourceRequest
    {
        /// <summary>
        /// Gets or sets the name of the price source
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the price source
        /// </summary>
        [Required]
        public PriceSourceType Type { get; set; }

        /// <summary>
        /// Gets or sets the URL of the price source API
        /// </summary>
        [Required]
        [Url]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the API key for the price source (if required)
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the API secret for the price source (if required)
        /// </summary>
        public string ApiSecret { get; set; }

        /// <summary>
        /// Gets or sets the weight of this source in the aggregated price (0-100)
        /// </summary>
        [Range(0, 100)]
        public int Weight { get; set; } = 100;

        /// <summary>
        /// Gets or sets the status of the price source
        /// </summary>
        public PriceSourceStatus Status { get; set; } = PriceSourceStatus.Testing;

        /// <summary>
        /// Gets or sets the update interval in seconds
        /// </summary>
        [Range(1, 86400)]
        public int UpdateIntervalSeconds { get; set; } = 60;

        /// <summary>
        /// Gets or sets the timeout in seconds for API requests
        /// </summary>
        [Range(1, 300)]
        public int TimeoutSeconds { get; set; } = 10;

        /// <summary>
        /// Gets or sets the list of supported assets
        /// </summary>
        [Required]
        [MinLength(1)]
        public List<string> SupportedAssets { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the configuration for parsing the API response
        /// </summary>
        public PriceSourceConfig Config { get; set; } = new PriceSourceConfig();
    }
}
