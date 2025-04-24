using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Api.Models.Responses
{
    /// <summary>
    /// Response model for detailed function information
    /// </summary>
    public class FunctionDetailResponse
    {
        /// <summary>
        /// Gets or sets the function ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the function
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the function
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the runtime for the function
        /// </summary>
        public string Runtime { get; set; }

        /// <summary>
        /// Gets or sets the handler for the function
        /// </summary>
        public string Handler { get; set; }

        /// <summary>
        /// Gets or sets the entry point for the function
        /// </summary>
        public string EntryPoint { get; set; }

        /// <summary>
        /// Gets or sets the source code of the function
        /// </summary>
        public string SourceCode { get; set; }

        /// <summary>
        /// Gets or sets the URL to the source code package
        /// </summary>
        public string SourceCodeUrl { get; set; }

        /// <summary>
        /// Gets or sets the hash of the source code package
        /// </summary>
        public string SourceCodeHash { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the function was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the function was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the status of the function
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the version of the function
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the function was last executed
        /// </summary>
        public DateTime? LastExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the number of times the function has been executed
        /// </summary>
        public int ExecutionCount { get; set; }

        /// <summary>
        /// Gets or sets the average execution time in milliseconds
        /// </summary>
        public double AverageExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the maximum memory usage in megabytes
        /// </summary>
        public double MaxMemoryUsage { get; set; }

        /// <summary>
        /// Gets or sets the environment variables for the function
        /// </summary>
        public Dictionary<string, string> EnvironmentVariables { get; set; }

        /// <summary>
        /// Gets or sets the list of secret IDs that the function has access to
        /// </summary>
        public List<Guid> SecretIds { get; set; }

        /// <summary>
        /// Gets or sets the maximum execution time in milliseconds
        /// </summary>
        public int MaxExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the maximum memory usage in megabytes
        /// </summary>
        public int MaxMemory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the function requires Trusted Execution Environment (TEE)
        /// </summary>
        public bool RequiresTee { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the function requires Virtual Private Cloud (VPC)
        /// </summary>
        public bool RequiresVpc { get; set; }

        /// <summary>
        /// Gets or sets the parent function ID for versioned functions
        /// </summary>
        public Guid? ParentFunctionId { get; set; }

        /// <summary>
        /// Gets or sets the list of version IDs for this function
        /// </summary>
        public List<Guid> VersionIds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is the latest version
        /// </summary>
        public bool IsLatestVersion { get; set; }
    }
}
