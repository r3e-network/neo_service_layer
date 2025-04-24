using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models.Analytics
{
    /// <summary>
    /// Configuration for analytics
    /// </summary>
    public class AnalyticsConfiguration
    {
        /// <summary>
        /// Gets or sets whether analytics is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the storage provider
        /// </summary>
        public string StorageProvider { get; set; } = "InMemory";

        /// <summary>
        /// Gets or sets the retention period in days
        /// </summary>
        public int RetentionDays { get; set; } = 90;

        /// <summary>
        /// Gets or sets the metric collection interval in seconds
        /// </summary>
        public int MetricCollectionIntervalSeconds { get; set; } = 60;

        /// <summary>
        /// Gets or sets the event batch size
        /// </summary>
        public int EventBatchSize { get; set; } = 100;

        /// <summary>
        /// Gets or sets the event flush interval in seconds
        /// </summary>
        public int EventFlushIntervalSeconds { get; set; } = 15;

        /// <summary>
        /// Gets or sets the alert evaluation interval in seconds
        /// </summary>
        public int AlertEvaluationIntervalSeconds { get; set; } = 60;

        /// <summary>
        /// Gets or sets the report execution interval in seconds
        /// </summary>
        public int ReportExecutionIntervalSeconds { get; set; } = 60;

        /// <summary>
        /// Gets or sets the maximum concurrent report executions
        /// </summary>
        public int MaxConcurrentReportExecutions { get; set; } = 5;

        /// <summary>
        /// Gets or sets the maximum report execution time in seconds
        /// </summary>
        public int MaxReportExecutionTimeSeconds { get; set; } = 300;

        /// <summary>
        /// Gets or sets the maximum query execution time in seconds
        /// </summary>
        public int MaxQueryExecutionTimeSeconds { get; set; } = 60;

        /// <summary>
        /// Gets or sets the maximum number of data points per query
        /// </summary>
        public int MaxDataPointsPerQuery { get; set; } = 10000;

        /// <summary>
        /// Gets or sets the maximum number of dimensions per query
        /// </summary>
        public int MaxDimensionsPerQuery { get; set; } = 10;

        /// <summary>
        /// Gets or sets the default aggregation period
        /// </summary>
        public AggregationPeriod DefaultAggregationPeriod { get; set; } = AggregationPeriod.Hour;

        /// <summary>
        /// Gets or sets the default time range
        /// </summary>
        public string DefaultTimeRange { get; set; } = "last24h";

        /// <summary>
        /// Gets or sets the default dashboard refresh interval in seconds
        /// </summary>
        public int DefaultDashboardRefreshIntervalSeconds { get; set; } = 300;

        /// <summary>
        /// Gets or sets the system metrics to collect
        /// </summary>
        public List<string> SystemMetrics { get; set; } = new List<string>
        {
            "cpu.usage",
            "memory.usage",
            "disk.usage",
            "network.in",
            "network.out",
            "api.requests",
            "api.errors",
            "api.latency"
        };

        /// <summary>
        /// Gets or sets the system events to track
        /// </summary>
        public List<string> SystemEvents { get; set; } = new List<string>
        {
            "user.login",
            "user.logout",
            "user.register",
            "account.created",
            "account.updated",
            "account.deleted",
            "wallet.created",
            "wallet.updated",
            "wallet.deleted",
            "transaction.created",
            "transaction.signed",
            "transaction.sent",
            "function.executed",
            "event.triggered",
            "notification.sent"
        };

        /// <summary>
        /// Gets or sets the default alert notification channels
        /// </summary>
        public List<NotificationChannel> DefaultAlertChannels { get; set; } = new List<NotificationChannel>
        {
            NotificationChannel.Email,
            NotificationChannel.InApp
        };

        /// <summary>
        /// Gets or sets the default report formats
        /// </summary>
        public List<ReportFormat> DefaultReportFormats { get; set; } = new List<ReportFormat>
        {
            ReportFormat.PDF,
            ReportFormat.CSV
        };

        /// <summary>
        /// Gets or sets the default report delivery methods
        /// </summary>
        public List<DeliveryMethod> DefaultReportDeliveryMethods { get; set; } = new List<DeliveryMethod>
        {
            DeliveryMethod.Email,
            DeliveryMethod.Storage
        };
    }
}
