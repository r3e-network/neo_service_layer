using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents an environment for function deployment
    /// </summary>
    public class FunctionEnvironment
    {
        /// <summary>
        /// Gets or sets the environment ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the account ID
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the name of the environment
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the environment
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the type of the environment (e.g., "development", "staging", "production")
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the network for the environment (e.g., "mainnet", "testnet")
        /// </summary>
        public string Network { get; set; }

        /// <summary>
        /// Gets or sets the environment variables for the environment
        /// </summary>
        public Dictionary<string, string> EnvironmentVariables { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the secret IDs for the environment
        /// </summary>
        public List<Guid> SecretIds { get; set; } = new List<Guid>();

        /// <summary>
        /// Gets or sets the created at timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the updated at timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the created by user ID
        /// </summary>
        public Guid CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the updated by user ID
        /// </summary>
        public Guid UpdatedBy { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the environment is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum number of functions that can be deployed to the environment
        /// </summary>
        public int MaxFunctions { get; set; } = 100;

        /// <summary>
        /// Gets or sets the maximum execution time for functions in the environment (in milliseconds)
        /// </summary>
        public int MaxExecutionTime { get; set; } = 30000;

        /// <summary>
        /// Gets or sets the maximum memory for functions in the environment (in megabytes)
        /// </summary>
        public int MaxMemory { get; set; } = 256;

        /// <summary>
        /// Gets or sets a value indicating whether the environment requires Trusted Execution Environment (TEE)
        /// </summary>
        public bool RequiresTee { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the environment requires Virtual Private Cloud (VPC)
        /// </summary>
        public bool RequiresVpc { get; set; } = false;

        /// <summary>
        /// Gets or sets the deployment settings for the environment
        /// </summary>
        public FunctionDeploymentSettings DeploymentSettings { get; set; } = new FunctionDeploymentSettings();
    }
}
