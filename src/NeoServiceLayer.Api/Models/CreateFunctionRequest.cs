using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NeoServiceLayer.Core.Enums;

namespace NeoServiceLayer.Api.Models
{
    /// <summary>
    /// Request model for function creation
    /// </summary>
    public class CreateFunctionRequest
    {
        /// <summary>
        /// Name for the function
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        /// <summary>
        /// Description of the function
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Runtime for the function
        /// </summary>
        [Required]
        public FunctionRuntime Runtime { get; set; }

        /// <summary>
        /// Source code of the function
        /// </summary>
        [Required]
        public string SourceCode { get; set; }

        /// <summary>
        /// Entry point for the function
        /// </summary>
        [Required]
        public string EntryPoint { get; set; }

        /// <summary>
        /// Maximum execution time in milliseconds
        /// </summary>
        [Range(1000, 300000)]
        public int MaxExecutionTime { get; set; } = 30000;

        /// <summary>
        /// Maximum memory in megabytes
        /// </summary>
        [Range(64, 1024)]
        public int MaxMemory { get; set; } = 128;

        /// <summary>
        /// List of secret IDs that this function can access
        /// </summary>
        public List<Guid> SecretIds { get; set; } = new List<Guid>();

        /// <summary>
        /// Environment variables for the function
        /// </summary>
        public Dictionary<string, string> EnvironmentVariables { get; set; } = new Dictionary<string, string>();
    }
}
