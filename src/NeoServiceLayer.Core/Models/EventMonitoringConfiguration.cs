using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Configuration for event monitoring
    /// </summary>
    public class EventMonitoringConfiguration
    {
        /// <summary>
        /// Gets or sets the Neo node URLs
        /// </summary>
        public List<string> NodeUrls { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the monitoring interval in seconds
        /// </summary>
        public int MonitoringIntervalSeconds { get; set; } = 15;

        /// <summary>
        /// Gets or sets the notification interval in seconds
        /// </summary>
        public int NotificationIntervalSeconds { get; set; } = 10;

        /// <summary>
        /// Gets or sets the start block height
        /// </summary>
        public long StartBlockHeight { get; set; } = 0;

        /// <summary>
        /// Gets or sets the maximum number of blocks to process in a single batch
        /// </summary>
        public int MaxBlockBatchSize { get; set; } = 100;

        /// <summary>
        /// Gets or sets the maximum number of concurrent notifications
        /// </summary>
        public int MaxConcurrentNotifications { get; set; } = 10;

        /// <summary>
        /// Gets or sets the default retry count for failed notifications
        /// </summary>
        public int DefaultRetryCount { get; set; } = 3;

        /// <summary>
        /// Gets or sets the default retry interval in seconds
        /// </summary>
        public int DefaultRetryIntervalSeconds { get; set; } = 60;

        /// <summary>
        /// Gets or sets the HTTP request timeout in seconds
        /// </summary>
        public int HttpTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets whether to auto-start monitoring on service initialization
        /// </summary>
        public bool AutoStart { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to include event data in notifications by default
        /// </summary>
        public bool IncludeEventDataByDefault { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum payload size in bytes
        /// </summary>
        public int MaxPayloadSizeBytes { get; set; } = 1048576; // 1 MB
    }
}
