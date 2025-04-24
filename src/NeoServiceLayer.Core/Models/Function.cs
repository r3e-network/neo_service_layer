using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents a function in the Neo Service Layer
    /// </summary>
    public class Function
    {
        /// <summary>
        /// Unique identifier for the function
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Name of the function
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the function
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Runtime for the function (JavaScript, Python, C#)
        /// </summary>
        public string Runtime { get; set; }

        /// <summary>
        /// Source code of the function
        /// </summary>
        public string SourceCode { get; set; }

        /// <summary>
        /// Entry point for the function
        /// </summary>
        public string EntryPoint { get; set; }

        /// <summary>
        /// Account ID that owns the function
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Maximum execution time in milliseconds
        /// </summary>
        public int MaxExecutionTime { get; set; }

        /// <summary>
        /// Maximum memory usage in megabytes
        /// </summary>
        public int MaxMemory { get; set; }

        /// <summary>
        /// URL to the source code package
        /// </summary>
        public string SourceCodeUrl { get; set; }

        /// <summary>
        /// Hash of the source code package
        /// </summary>
        public string SourceCodeHash { get; set; }

        /// <summary>
        /// Memory requirement in megabytes
        /// </summary>
        public int MemoryRequirementMb { get; set; }

        /// <summary>
        /// CPU requirement
        /// </summary>
        public int CpuRequirement { get; set; }

        /// <summary>
        /// Timeout in seconds
        /// </summary>
        public int TimeoutSeconds { get; set; }

        /// <summary>
        /// Handler for the function
        /// </summary>
        public string Handler { get; set; }

        /// <summary>
        /// User who created the function
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// List of secret IDs that the function has access to
        /// </summary>
        public List<Guid> SecretIds { get; set; }

        /// <summary>
        /// Environment variables for the function
        /// </summary>
        public Dictionary<string, string> EnvironmentVariables { get; set; }

        /// <summary>
        /// Date and time when the function was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date and time when the function was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Date and time when the function was last executed
        /// </summary>
        public DateTime? LastExecutedAt { get; set; }

        /// <summary>
        /// Status of the function (Active, Inactive, Error)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Indicates whether the function requires Trusted Execution Environment (TEE)
        /// </summary>
        public bool RequiresTee { get; set; }

        /// <summary>
        /// Indicates whether the function requires Virtual Private Cloud (VPC)
        /// </summary>
        public bool RequiresVpc { get; set; }

        /// <summary>
        /// Version of the function
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// Parent function ID for versioned functions
        /// </summary>
        public Guid? ParentFunctionId { get; set; }

        /// <summary>
        /// List of version IDs for this function
        /// </summary>
        public List<Guid> VersionIds { get; set; } = new List<Guid>();

        /// <summary>
        /// Indicates whether this is the latest version
        /// </summary>
        public bool IsLatestVersion { get; set; } = true;

        /// <summary>
        /// Number of times the function has been executed
        /// </summary>
        public int ExecutionCount { get; set; } = 0;

        /// <summary>
        /// Average execution time in milliseconds
        /// </summary>
        public double AverageExecutionTime { get; set; } = 0;

        /// <summary>
        /// Maximum memory usage in megabytes
        /// </summary>
        public double MaxMemoryUsage { get; set; } = 0;

        /// <summary>
        /// Last execution time in milliseconds
        /// </summary>
        public double? LastExecutionTime { get; set; }

        /// <summary>
        /// Compiled assembly for the function
        /// </summary>
        public object CompiledAssembly { get; set; }

        /// <summary>
        /// Source of the function (e.g., "marketplace", "custom")
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Tags associated with the function
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Dependencies required by the function
        /// </summary>
        public List<string> Dependencies { get; set; } = new List<string>();

        /// <summary>
        /// Additional metadata for the function
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// The function code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the input schema for the function
        /// </summary>
        public string InputSchema { get; set; }

        /// <summary>
        /// Gets or sets the output schema for the function
        /// </summary>
        public string OutputSchema { get; set; }
    }
}
