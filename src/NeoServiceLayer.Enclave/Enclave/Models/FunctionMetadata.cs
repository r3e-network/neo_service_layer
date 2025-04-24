using System;
using System.Collections.Generic;
using System.Reflection;

namespace NeoServiceLayer.Enclave.Enclave.Models
{
    /// <summary>
    /// Metadata for a function
    /// </summary>
    public class FunctionMetadata
    {
        /// <summary>
        /// Gets or sets the function ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the account ID
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the function name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the function description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the function runtime
        /// </summary>
        public string Runtime { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the function source code
        /// </summary>
        public string SourceCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the function entry point
        /// </summary>
        public string EntryPoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the function memory limit in MB
        /// </summary>
        public int MemoryLimitMB { get; set; }

        /// <summary>
        /// Gets or sets the function timeout in seconds
        /// </summary>
        public int TimeoutSeconds { get; set; }

        /// <summary>
        /// Gets or sets the maximum execution time in seconds
        /// </summary>
        public int MaxExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the maximum memory in MB
        /// </summary>
        public int MaxMemory { get; set; }

        /// <summary>
        /// Gets or sets the function secret IDs
        /// </summary>
        public List<Guid> SecretIds { get; set; } = new List<Guid>();

        /// <summary>
        /// Gets or sets the function environment variables
        /// </summary>
        public Dictionary<string, string> EnvironmentVariables { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the function creation date
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the function last update date
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the function status
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the compiled assembly
        /// </summary>
        public string CompiledAssembly { get; set; } = string.Empty;
    }
}
