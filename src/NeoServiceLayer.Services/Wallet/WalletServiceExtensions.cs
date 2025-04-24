using Microsoft.Extensions.DependencyInjection;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Services.Wallet.Repositories;

namespace NeoServiceLayer.Services.Wallet
{
    /// <summary>
    /// Extension methods for registering wallet services
    /// </summary>
    public static class WalletServiceExtensions
    {
        /// <summary>
        /// Adds wallet services to the service collection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddWalletServices(this IServiceCollection services)
        {
            // Register repositories
            services.AddSingleton<IWalletRepository, WalletRepository>();

            // Register services
            services.AddSingleton<IWalletService, WalletService>();

            return services;
        }
    }
}
