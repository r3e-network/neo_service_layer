using Microsoft.Extensions.DependencyInjection;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Services.Account.Repositories;

namespace NeoServiceLayer.Services.Account
{
    /// <summary>
    /// Extension methods for registering account services
    /// </summary>
    public static class AccountServiceExtensions
    {
        /// <summary>
        /// Adds account services to the service collection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddAccountServices(this IServiceCollection services)
        {
            // Register repositories
            services.AddSingleton<IAccountRepository, AccountRepository>();

            // Register services
            services.AddSingleton<IAccountService, AccountService>();

            return services;
        }
    }
}
