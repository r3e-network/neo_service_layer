using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models.Deployment
{
    /// <summary>
    /// Represents a deployment in the system
    /// </summary>
    public class Deployment
    {
        /// <summary>
        /// Gets or sets the unique identifier for the deployment
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the deployment
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the deployment
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the account ID that owns this deployment
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the user ID that created this deployment
        /// </summary>
        public Guid CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the function ID associated with this deployment
        /// </summary>
        public Guid FunctionId { get; set; }

        /// <summary>
        /// Gets or sets the environment ID for this deployment
        /// </summary>
        public Guid EnvironmentId { get; set; }

        /// <summary>
        /// Gets or sets the current version ID for this deployment
        /// </summary>
        public Guid CurrentVersionId { get; set; }

        /// <summary>
        /// Gets or sets the previous version ID for this deployment
        /// </summary>
        public Guid? PreviousVersionId { get; set; }

        /// <summary>
        /// Gets or sets the deployment strategy
        /// </summary>
        public DeploymentStrategy Strategy { get; set; }

        /// <summary>
        /// Gets or sets the deployment configuration
        /// </summary>
        public DeploymentConfiguration Configuration { get; set; }

        /// <summary>
        /// Gets or sets the current status of the deployment
        /// </summary>
        public DeploymentStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last deployment timestamp
        /// </summary>
        public DateTime? LastDeployedAt { get; set; }

        /// <summary>
        /// Gets or sets the tags associated with the deployment
        /// </summary>
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the deployment metrics
        /// </summary>
        public DeploymentMetrics Metrics { get; set; }

        /// <summary>
        /// Gets or sets the deployment health
        /// </summary>
        public DeploymentHealth Health { get; set; }
    }

    /// <summary>
    /// Represents the metrics for a deployment
    /// </summary>
    public class DeploymentMetrics
    {
        /// <summary>
        /// Gets or sets the total number of requests
        /// </summary>
        public long TotalRequests { get; set; }

        /// <summary>
        /// Gets or sets the number of successful requests
        /// </summary>
        public long SuccessfulRequests { get; set; }

        /// <summary>
        /// Gets or sets the number of failed requests
        /// </summary>
        public long FailedRequests { get; set; }

        /// <summary>
        /// Gets or sets the average response time in milliseconds
        /// </summary>
        public double AverageResponseTimeMs { get; set; }

        /// <summary>
        /// Gets or sets the 95th percentile response time in milliseconds
        /// </summary>
        public double P95ResponseTimeMs { get; set; }

        /// <summary>
        /// Gets or sets the 99th percentile response time in milliseconds
        /// </summary>
        public double P99ResponseTimeMs { get; set; }

        /// <summary>
        /// Gets or sets the error rate as a percentage
        /// </summary>
        public double ErrorRate { get; set; }

        /// <summary>
        /// Gets or sets the average CPU usage as a percentage
        /// </summary>
        public double AverageCpuUsage { get; set; }

        /// <summary>
        /// Gets or sets the average memory usage in MB
        /// </summary>
        public double AverageMemoryUsageMb { get; set; }

        /// <summary>
        /// Gets or sets the last update timestamp
        /// </summary>
        public DateTime LastUpdatedAt { get; set; }
    }

    /// <summary>
    /// Represents the health of a deployment
    /// </summary>
    public class DeploymentHealth
    {
        /// <summary>
        /// Gets or sets the overall health status
        /// </summary>
        public HealthStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the health checks
        /// </summary>
        public List<HealthCheck> Checks { get; set; } = new List<HealthCheck>();

        /// <summary>
        /// Gets or sets the last check timestamp
        /// </summary>
        public DateTime LastCheckedAt { get; set; }
    }

    /// <summary>
    /// Represents a health check for a deployment
    /// </summary>
    public class HealthCheck
    {
        /// <summary>
        /// Gets or sets the name of the health check
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the status of the health check
        /// </summary>
        public HealthStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the message for the health check
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the health check
        /// </summary>
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Represents the health status of a deployment
    /// </summary>
    public enum HealthStatus
    {
        /// <summary>
        /// Healthy status
        /// </summary>
        Healthy,

        /// <summary>
        /// Degraded status
        /// </summary>
        Degraded,

        /// <summary>
        /// Unhealthy status
        /// </summary>
        Unhealthy,

        /// <summary>
        /// Unknown status
        /// </summary>
        Unknown
    }

    /// <summary>
    /// Represents the status of a deployment
    /// </summary>
    public enum DeploymentStatus
    {
        /// <summary>
        /// Pending status
        /// </summary>
        Pending,

        /// <summary>
        /// Validating status
        /// </summary>
        Validating,

        /// <summary>
        /// Deploying status
        /// </summary>
        Deploying,

        /// <summary>
        /// Deployed status
        /// </summary>
        Deployed,

        /// <summary>
        /// Failed status
        /// </summary>
        Failed,

        /// <summary>
        /// Rolling back status
        /// </summary>
        RollingBack,

        /// <summary>
        /// Rolled back status
        /// </summary>
        RolledBack,

        /// <summary>
        /// Stopped status
        /// </summary>
        Stopped
    }
}
