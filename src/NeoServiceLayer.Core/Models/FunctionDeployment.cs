using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents a function deployment
    /// </summary>
    public class FunctionDeployment
    {
        /// <summary>
        /// Gets or sets the deployment ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the function ID
        /// </summary>
        public Guid FunctionId { get; set; }

        /// <summary>
        /// Gets or sets the environment ID
        /// </summary>
        public Guid EnvironmentId { get; set; }

        /// <summary>
        /// Gets or sets the account ID
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the version of the function
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the status of the deployment
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the deployment URL
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the deployment logs
        /// </summary>
        public List<string> Logs { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the error message if the deployment failed
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the created at timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the updated at timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the deployed at timestamp
        /// </summary>
        public DateTime? DeployedAt { get; set; }

        /// <summary>
        /// Gets or sets the created by user ID
        /// </summary>
        public Guid CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the updated by user ID
        /// </summary>
        public Guid UpdatedBy { get; set; }

        /// <summary>
        /// Gets or sets the environment variables for the deployment
        /// </summary>
        public Dictionary<string, string> EnvironmentVariables { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the secret IDs for the deployment
        /// </summary>
        public List<Guid> SecretIds { get; set; } = new List<Guid>();

        /// <summary>
        /// Gets or sets the deployment settings
        /// </summary>
        public FunctionDeploymentSettings Settings { get; set; } = new FunctionDeploymentSettings();

        /// <summary>
        /// Gets or sets the deployment resources
        /// </summary>
        public FunctionDeploymentResources Resources { get; set; } = new FunctionDeploymentResources();

        /// <summary>
        /// Gets or sets the deployment metrics
        /// </summary>
        public FunctionDeploymentMetrics Metrics { get; set; } = new FunctionDeploymentMetrics();
    }
}
