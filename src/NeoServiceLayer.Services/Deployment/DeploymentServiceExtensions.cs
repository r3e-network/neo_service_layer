using Microsoft.Extensions.DependencyInjection;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Services.Deployment.Repositories;
using NeoServiceLayer.Services.Deployment.Strategies;
using NeoServiceLayer.Services.Deployment.Validators;

namespace NeoServiceLayer.Services.Deployment
{
    /// <summary>
    /// Extension methods for registering deployment services
    /// </summary>
    public static class DeploymentServiceExtensions
    {
        /// <summary>
        /// Adds deployment services to the service collection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddDeploymentServices(this IServiceCollection services)
        {
            // Register repositories
            services.AddSingleton<IDeploymentRepository, DeploymentRepository>();
            services.AddSingleton<IDeploymentVersionRepository, DeploymentVersionRepository>();
            services.AddSingleton<IDeploymentEnvironmentRepository, DeploymentEnvironmentRepository>();

            // Register validators
            services.AddSingleton<IDeploymentValidator, DeploymentValidator>();

            // Register strategies
            services.AddSingleton<IDeploymentStrategy, AllAtOnceDeploymentStrategy>();
            services.AddSingleton<IDeploymentStrategy, BlueGreenDeploymentStrategy>();
            services.AddSingleton<IDeploymentStrategy, CanaryDeploymentStrategy>();

            // Register services
            services.AddSingleton<IDeploymentService, DeploymentService>();

            return services;
        }
    }
}
