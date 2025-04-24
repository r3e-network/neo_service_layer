using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents settings for a function deployment
    /// </summary>
    public class FunctionDeploymentSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether to automatically deploy new versions
        /// </summary>
        public bool AutoDeploy { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to automatically roll back failed deployments
        /// </summary>
        public bool AutoRollback { get; set; } = true;

        /// <summary>
        /// Gets or sets the deployment strategy (e.g., "all-at-once", "blue-green", "canary")
        /// </summary>
        public string DeploymentStrategy { get; set; } = "all-at-once";

        /// <summary>
        /// Gets or sets the percentage of traffic to route to the new version during canary deployments
        /// </summary>
        public int CanaryPercentage { get; set; } = 10;

        /// <summary>
        /// Gets or sets the number of minutes to wait before promoting a canary deployment
        /// </summary>
        public int CanaryPromotionMinutes { get; set; } = 30;

        /// <summary>
        /// Gets or sets a value indicating whether to run pre-deployment tests
        /// </summary>
        public bool RunPreDeploymentTests { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to run post-deployment tests
        /// </summary>
        public bool RunPostDeploymentTests { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to require approval before deployment
        /// </summary>
        public bool RequireApproval { get; set; } = false;

        /// <summary>
        /// Gets or sets the list of user IDs that can approve deployments
        /// </summary>
        public List<string> ApproverIds { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets a value indicating whether to notify on deployment events
        /// </summary>
        public bool NotifyOnDeployment { get; set; } = true;

        /// <summary>
        /// Gets or sets the notification channels for deployment events
        /// </summary>
        public List<string> NotificationChannels { get; set; } = new List<string> { "email" };

        /// <summary>
        /// Gets or sets the notification recipients for deployment events
        /// </summary>
        public List<string> NotificationRecipients { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the maximum number of concurrent deployments
        /// </summary>
        public int MaxConcurrentDeployments { get; set; } = 1;

        /// <summary>
        /// Gets or sets the maximum number of versions to keep
        /// </summary>
        public int MaxVersionsToKeep { get; set; } = 5;

        /// <summary>
        /// Gets or sets a value indicating whether to enable deployment history
        /// </summary>
        public bool EnableDeploymentHistory { get; set; } = true;

        /// <summary>
        /// Gets or sets the deployment timeout in seconds
        /// </summary>
        public int DeploymentTimeoutSeconds { get; set; } = 300;

        /// <summary>
        /// Gets or sets a value indicating whether to enable deployment metrics
        /// </summary>
        public bool EnableDeploymentMetrics { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to enable deployment logs
        /// </summary>
        public bool EnableDeploymentLogs { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to enable deployment alerts
        /// </summary>
        public bool EnableDeploymentAlerts { get; set; } = true;

        /// <summary>
        /// Gets or sets the deployment alert thresholds
        /// </summary>
        public Dictionary<string, double> DeploymentAlertThresholds { get; set; } = new Dictionary<string, double>
        {
            { "ErrorRate", 0.1 },
            { "LatencyMs", 1000 },
            { "MemoryUsageMb", 256 }
        };
    }
}
