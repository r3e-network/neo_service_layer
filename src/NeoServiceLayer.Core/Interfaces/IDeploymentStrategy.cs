using System;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models.Deployment;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for deployment strategy
    /// </summary>
    public interface IDeploymentStrategy
    {
        /// <summary>
        /// Gets the type of the strategy
        /// </summary>
        DeploymentStrategy StrategyType { get; }

        /// <summary>
        /// Deploys a version
        /// </summary>
        /// <param name="deploymentId">Deployment ID</param>
        /// <param name="versionId">Version ID</param>
        /// <param name="configuration">Deployment configuration</param>
        /// <returns>The deployment status</returns>
        Task<DeploymentStatus> DeployAsync(Guid deploymentId, Guid versionId, DeploymentConfiguration configuration);

        /// <summary>
        /// Rolls back a deployment
        /// </summary>
        /// <param name="deploymentId">Deployment ID</param>
        /// <returns>The deployment status</returns>
        Task<DeploymentStatus> RollbackAsync(Guid deploymentId);

        /// <summary>
        /// Updates the traffic routing for a deployment
        /// </summary>
        /// <param name="deploymentId">Deployment ID</param>
        /// <param name="trafficRouting">Traffic routing configuration</param>
        /// <returns>The deployment status</returns>
        Task<DeploymentStatus> UpdateTrafficRoutingAsync(Guid deploymentId, TrafficRoutingConfiguration trafficRouting);

        /// <summary>
        /// Validates a deployment configuration
        /// </summary>
        /// <param name="configuration">Deployment configuration</param>
        /// <returns>True if the configuration is valid, false otherwise</returns>
        bool ValidateConfiguration(DeploymentConfiguration configuration);

        /// <summary>
        /// Gets the default configuration for the strategy
        /// </summary>
        /// <returns>The default configuration</returns>
        DeploymentConfiguration GetDefaultConfiguration();
    }
}
