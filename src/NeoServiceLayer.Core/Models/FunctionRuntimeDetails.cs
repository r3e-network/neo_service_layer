using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Function runtime details
    /// </summary>
    public class FunctionRuntimeDetails
    {
        /// <summary>
        /// Gets or sets the runtime name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the runtime version
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the runtime description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the runtime supported file extensions
        /// </summary>
        public List<string> SupportedFileExtensions { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the runtime supported handlers
        /// </summary>
        public List<string> SupportedHandlers { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the runtime supported features
        /// </summary>
        public List<string> SupportedFeatures { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the runtime supported libraries
        /// </summary>
        public List<string> SupportedLibraries { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the runtime supported frameworks
        /// </summary>
        public List<string> SupportedFrameworks { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the runtime supported platforms
        /// </summary>
        public List<string> SupportedPlatforms { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the runtime supported architectures
        /// </summary>
        public List<string> SupportedArchitectures { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the runtime supported operating systems
        /// </summary>
        public List<string> SupportedOperatingSystems { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the runtime supported memory sizes
        /// </summary>
        public List<int> SupportedMemorySizes { get; set; } = new List<int>();

        /// <summary>
        /// Gets or sets the runtime supported timeouts
        /// </summary>
        public List<int> SupportedTimeouts { get; set; } = new List<int>();

        /// <summary>
        /// Gets or sets the runtime supported environment variables
        /// </summary>
        public List<string> SupportedEnvironmentVariables { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the runtime supported input types
        /// </summary>
        public List<string> SupportedInputTypes { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the runtime supported output types
        /// </summary>
        public List<string> SupportedOutputTypes { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the runtime supported error types
        /// </summary>
        public List<string> SupportedErrorTypes { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the runtime supported log levels
        /// </summary>
        public List<string> SupportedLogLevels { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the runtime supported metrics
        /// </summary>
        public List<string> SupportedMetrics { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the runtime supported tags
        /// </summary>
        public List<string> SupportedTags { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the runtime supported metadata
        /// </summary>
        public List<string> SupportedMetadata { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the runtime supported debug modes
        /// </summary>
        public List<string> SupportedDebugModes { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the runtime supported test modes
        /// </summary>
        public List<string> SupportedTestModes { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the runtime supported trace modes
        /// </summary>
        public List<string> SupportedTraceModes { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the runtime supported profile modes
        /// </summary>
        public List<string> SupportedProfileModes { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the runtime supported verbose modes
        /// </summary>
        public List<string> SupportedVerboseModes { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the runtime supported silent modes
        /// </summary>
        public List<string> SupportedSilentModes { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the runtime supported async modes
        /// </summary>
        public List<string> SupportedAsyncModes { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the runtime supported batch modes
        /// </summary>
        public List<string> SupportedBatchModes { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the runtime supported stream modes
        /// </summary>
        public List<string> SupportedStreamModes { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the runtime supported retry modes
        /// </summary>
        public List<string> SupportedRetryModes { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the runtime supported retry counts
        /// </summary>
        public List<int> SupportedRetryCounts { get; set; } = new List<int>();

        /// <summary>
        /// Gets or sets the runtime supported retry intervals
        /// </summary>
        public List<int> SupportedRetryIntervals { get; set; } = new List<int>();

        /// <summary>
        /// Gets or sets the runtime supported retry max counts
        /// </summary>
        public List<int> SupportedRetryMaxCounts { get; set; } = new List<int>();

        /// <summary>
        /// Gets or sets the runtime supported retry max intervals
        /// </summary>
        public List<int> SupportedRetryMaxIntervals { get; set; } = new List<int>();

        /// <summary>
        /// Gets or sets the runtime supported retry backoff factors
        /// </summary>
        public List<double> SupportedRetryBackoffFactors { get; set; } = new List<double>();

        /// <summary>
        /// Gets or sets the runtime supported retry jitter factors
        /// </summary>
        public List<double> SupportedRetryJitterFactors { get; set; } = new List<double>();
    }
}
