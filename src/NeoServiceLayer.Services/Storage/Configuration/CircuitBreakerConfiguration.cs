namespace NeoServiceLayer.Services.Storage.Configuration
{
    /// <summary>
    /// Configuration for circuit breakers
    /// </summary>
    public class CircuitBreakerConfiguration
    {
        /// <summary>
        /// Gets or sets whether circuit breakers are enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the failure threshold
        /// </summary>
        public int FailureThreshold { get; set; } = 5;

        /// <summary>
        /// Gets or sets the reset timeout in seconds
        /// </summary>
        public int ResetTimeoutSeconds { get; set; } = 60;
    }
}
