using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models.Deployment
{
    /// <summary>
    /// Represents a deployment version in the system
    /// </summary>
    public class DeploymentVersion
    {
        /// <summary>
        /// Gets or sets the unique identifier for the version
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the deployment ID
        /// </summary>
        public Guid DeploymentId { get; set; }

        /// <summary>
        /// Gets or sets the function ID
        /// </summary>
        public Guid FunctionId { get; set; }

        /// <summary>
        /// Gets or sets the version number
        /// </summary>
        public string VersionNumber { get; set; } = "0";

        /// <summary>
        /// Gets or sets the version label
        /// </summary>
        public string VersionLabel { get; set; }

        /// <summary>
        /// Gets or sets the description of the version
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the account ID that owns this version
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the user ID that created this version
        /// </summary>
        public Guid CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the source code package URL
        /// </summary>
        public string SourceCodePackageUrl { get; set; }

        /// <summary>
        /// Gets or sets the source code hash
        /// </summary>
        public string SourceCodeHash { get; set; }

        /// <summary>
        /// Gets or sets the artifact URL
        /// </summary>
        public string ArtifactUrl { get; set; }

        /// <summary>
        /// Gets or sets the artifact hash
        /// </summary>
        public string ArtifactHash { get; set; }

        /// <summary>
        /// Gets or sets the configuration for the version
        /// </summary>
        public VersionConfiguration Configuration { get; set; }

        /// <summary>
        /// Gets or sets the environment variables
        /// </summary>
        public Dictionary<string, string> EnvironmentVariables { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the status of the version
        /// </summary>
        public VersionStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the deployment timestamp
        /// </summary>
        public DateTime? DeployedAt { get; set; }

        /// <summary>
        /// Gets or sets the tags associated with the version
        /// </summary>
        public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the validation results
        /// </summary>
        public ValidationResults ValidationResults { get; set; }

        /// <summary>
        /// Gets or sets the deployment logs
        /// </summary>
        public List<DeploymentLog> Logs { get; set; } = new List<DeploymentLog>();

        /// <summary>
        /// Gets or sets the dependencies
        /// </summary>
        public List<Dependency> Dependencies { get; set; } = new List<Dependency>();
    }

    /// <summary>
    /// Represents the configuration for a version
    /// </summary>
    public class VersionConfiguration
    {
        /// <summary>
        /// Gets or sets the runtime for the version
        /// </summary>
        public string Runtime { get; set; }

        /// <summary>
        /// Gets or sets the memory size in MB
        /// </summary>
        public int MemorySizeMb { get; set; }

        /// <summary>
        /// Gets or sets the CPU size in vCPUs
        /// </summary>
        public double CpuSize { get; set; }

        /// <summary>
        /// Gets or sets the timeout in seconds
        /// </summary>
        public int TimeoutSeconds { get; set; }

        /// <summary>
        /// Gets or sets the entry point
        /// </summary>
        public string EntryPoint { get; set; }

        /// <summary>
        /// Gets or sets the handler
        /// </summary>
        public string Handler { get; set; }

        /// <summary>
        /// Gets or sets the build command
        /// </summary>
        public string BuildCommand { get; set; }

        /// <summary>
        /// Gets or sets the start command
        /// </summary>
        public string StartCommand { get; set; }

        /// <summary>
        /// Gets or sets the health check path
        /// </summary>
        public string HealthCheckPath { get; set; }

        /// <summary>
        /// Gets or sets the health check timeout in seconds
        /// </summary>
        public int HealthCheckTimeoutSeconds { get; set; }

        /// <summary>
        /// Gets or sets the version-specific configuration
        /// </summary>
        public Dictionary<string, string> VersionSpecificConfig { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Represents the validation results for a version
    /// </summary>
    public class ValidationResults
    {
        /// <summary>
        /// Gets or sets whether the validation passed
        /// </summary>
        public bool Passed { get; set; }

        /// <summary>
        /// Gets or sets the validation checks
        /// </summary>
        public List<ValidationCheck> Checks { get; set; } = new List<ValidationCheck>();

        /// <summary>
        /// Gets or sets the validation timestamp
        /// </summary>
        public DateTime ValidatedAt { get; set; }
    }

    /// <summary>
    /// Represents a validation check for a version
    /// </summary>
    public class ValidationCheck
    {
        /// <summary>
        /// Gets or sets the name of the validation check
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the validation check
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets whether the validation check passed
        /// </summary>
        public bool Passed { get; set; }

        /// <summary>
        /// Gets or sets the message for the validation check
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the severity of the validation check
        /// </summary>
        public ValidationSeverity Severity { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the validation check
        /// </summary>
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Represents a deployment log entry
    /// </summary>
    public class DeploymentLog
    {
        /// <summary>
        /// Gets or sets the timestamp of the log entry
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the level of the log entry
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// Gets or sets the message of the log entry
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the source of the log entry
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the details of the log entry
        /// </summary>
        public Dictionary<string, string> Details { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Represents a dependency for a version
    /// </summary>
    public class Dependency
    {
        /// <summary>
        /// Gets or sets the name of the dependency
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the version of the dependency
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the type of the dependency
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the source of the dependency
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the license of the dependency
        /// </summary>
        public string License { get; set; }
    }

    /// <summary>
    /// Represents the severity of a validation check
    /// </summary>
    public enum ValidationSeverity
    {
        /// <summary>
        /// Info severity
        /// </summary>
        Info,

        /// <summary>
        /// Warning severity
        /// </summary>
        Warning,

        /// <summary>
        /// Error severity
        /// </summary>
        Error,

        /// <summary>
        /// Critical severity
        /// </summary>
        Critical
    }

    /// <summary>
    /// Represents the level of a log entry
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Debug level
        /// </summary>
        Debug,

        /// <summary>
        /// Info level
        /// </summary>
        Info,

        /// <summary>
        /// Warning level
        /// </summary>
        Warning,

        /// <summary>
        /// Error level
        /// </summary>
        Error,

        /// <summary>
        /// Critical level
        /// </summary>
        Critical
    }

    /// <summary>
    /// Represents the status of a version
    /// </summary>
    public enum VersionStatus
    {
        /// <summary>
        /// Created status
        /// </summary>
        Created,

        /// <summary>
        /// Validating status
        /// </summary>
        Validating,

        /// <summary>
        /// Validated status
        /// </summary>
        Validated,

        /// <summary>
        /// Building status
        /// </summary>
        Building,

        /// <summary>
        /// Built status
        /// </summary>
        Built,

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
        /// Archived status
        /// </summary>
        Archived
    }
}
