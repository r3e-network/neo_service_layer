namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents a retry policy for a function composition step
    /// </summary>
    public class FunctionCompositionRetryPolicy
    {
        /// <summary>
        /// Gets or sets the maximum number of retries
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Gets or sets the initial retry delay in milliseconds
        /// </summary>
        public int InitialDelayMs { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the maximum retry delay in milliseconds
        /// </summary>
        public int MaxDelayMs { get; set; } = 10000;

        /// <summary>
        /// Gets or sets the backoff multiplier
        /// </summary>
        public double BackoffMultiplier { get; set; } = 2.0;

        /// <summary>
        /// Gets or sets a value indicating whether to use jitter
        /// </summary>
        public bool UseJitter { get; set; } = true;

        /// <summary>
        /// Gets or sets the retry on status codes
        /// </summary>
        public string[] RetryOnStatusCodes { get; set; } = new string[] { "500", "502", "503", "504" };

        /// <summary>
        /// Gets or sets the retry on error types
        /// </summary>
        public string[] RetryOnErrorTypes { get; set; } = new string[] { "timeout", "network", "server" };
    }
}
