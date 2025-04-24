using System.Threading.Tasks;
using NeoServiceLayer.Core.Models.Deployment;

namespace NeoServiceLayer.Services.Deployment.Validators
{
    /// <summary>
    /// Interface for deployment validator
    /// </summary>
    public interface IDeploymentValidator
    {
        /// <summary>
        /// Validates a deployment environment
        /// </summary>
        /// <param name="environment">Environment to validate</param>
        /// <returns>Validation result</returns>
        Task<ValidationResult> ValidateEnvironmentAsync(DeploymentEnvironment environment);

        /// <summary>
        /// Validates a deployment version
        /// </summary>
        /// <param name="version">Version to validate</param>
        /// <returns>Validation result</returns>
        Task<ValidationResult> ValidateVersionAsync(DeploymentVersion version);
    }
}
