using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NeoServiceLayer.Api.Models.Requests
{
    /// <summary>
    /// Request model for updating a function
    /// </summary>
    public class UpdateFunctionRequest
    {
        /// <summary>
        /// Gets or sets the name of the function
        /// </summary>
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the function
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the runtime for the function (e.g., "javascript", "python", "csharp")
        /// </summary>
        public string Runtime { get; set; }

        /// <summary>
        /// Gets or sets the source code of the function
        /// </summary>
        public string SourceCode { get; set; }

        /// <summary>
        /// Gets or sets the entry point for the function
        /// </summary>
        public string EntryPoint { get; set; }

        /// <summary>
        /// Gets or sets the handler for the function
        /// </summary>
        public string Handler { get; set; }

        /// <summary>
        /// Gets or sets the URL to the source code package
        /// </summary>
        public string SourceCodeUrl { get; set; }

        /// <summary>
        /// Gets or sets the hash of the source code package
        /// </summary>
        public string SourceCodeHash { get; set; }

        /// <summary>
        /// Gets or sets the maximum execution time in milliseconds
        /// </summary>
        [Range(100, 300000)]
        public int? MaxExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the maximum memory usage in megabytes
        /// </summary>
        [Range(32, 1024)]
        public int? MaxMemory { get; set; }

        /// <summary>
        /// Gets or sets the list of secret IDs that the function has access to
        /// </summary>
        public List<Guid> SecretIds { get; set; }

        /// <summary>
        /// Gets or sets the environment variables for the function
        /// </summary>
        public Dictionary<string, string> EnvironmentVariables { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the function requires Trusted Execution Environment (TEE)
        /// </summary>
        public bool? RequiresTee { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the function requires Virtual Private Cloud (VPC)
        /// </summary>
        public bool? RequiresVpc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to create a new version
        /// </summary>
        public bool CreateNewVersion { get; set; } = false;

        /// <summary>
        /// Gets or sets the version of the function
        /// </summary>
        [RegularExpression(@"^\d+\.\d+\.\d+$")]
        public string Version { get; set; }
    }
}
