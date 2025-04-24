using System;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents a retry policy for function execution
    /// </summary>
    public class FunctionRetryPolicy
    {
        /// <summary>
        /// Gets or sets the maximum number of retries
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Gets or sets the initial delay in milliseconds
        /// </summary>
        public int InitialDelayMs { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the backoff multiplier
        /// </summary>
        public double BackoffMultiplier { get; set; } = 2.0;

        /// <summary>
        /// Gets or sets the maximum delay in milliseconds
        /// </summary>
        public int MaxDelayMs { get; set; } = 30000;
    }
}
