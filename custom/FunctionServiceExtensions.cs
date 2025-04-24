using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Services.Function.Repositories;
using NeoServiceLayer.Services.Function.Runtimes;
using ServiceRepositories = NeoServiceLayer.Services.Function.Repositories;
using CoreInterfaces = NeoServiceLayer.Core.Interfaces;

namespace NeoServiceLayer.Services.Function
{
    /// <summary>
    /// Extension methods for registering function services
    /// </summary>
    public static class FunctionServiceExtensions
    {
        /// <summary>
        /// Adds function services to the service collection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection AddFunctionServices(this IServiceCollection services, IConfiguration configuration = null)
        {
            // Check if function service is enabled
            if (configuration != null && !configuration.GetValue<bool>("Function:Enabled", false))
            {
                // Register minimal services needed for dependency resolution
                services.AddSingleton<CoreInterfaces.IFunctionService, FunctionService>();
                services.AddSingleton<ServiceRepositories.IFunctionTemplateRepository, ServiceRepositories.FunctionTemplateRepository>();
                services.AddSingleton<FunctionTemplateInitializer>();
                return services;
            }

            // Register repositories
            services.AddSingleton<ServiceRepositories.IFunctionRepository, ServiceRepositories.FunctionRepository>();
            services.AddSingleton<ServiceRepositories.IFunctionVersionRepository, ServiceRepositories.FunctionVersionRepository>();
            services.AddSingleton<ServiceRepositories.IFunctionExecutionRepository, ServiceRepositories.FunctionExecutionRepository>();
            services.AddSingleton<ServiceRepositories.IFunctionTemplateRepository, ServiceRepositories.FunctionTemplateRepository>();
            services.AddSingleton<ServiceRepositories.IFunctionAccessControlRepository, ServiceRepositories.FunctionAccessControlRepository>();
            services.AddSingleton<ServiceRepositories.IFunctionMarketplaceRepository, ServiceRepositories.FunctionMarketplaceRepository>();
            services.AddSingleton<ServiceRepositories.IFunctionCompositionRepository, ServiceRepositories.FunctionCompositionRepository>();

            // Register services
            services.AddSingleton<CoreInterfaces.IFunctionService, FunctionService>();
            services.AddSingleton<FunctionTemplateInitializer>();
            services.AddSingleton<CoreInterfaces.IFunctionTestService, FunctionTestService>();
            services.AddSingleton<CoreInterfaces.IFunctionAccessControlService, FunctionAccessControlService>();
            services.AddSingleton<CoreInterfaces.IFunctionMarketplaceService, FunctionMarketplaceService>();
            services.AddSingleton<CoreInterfaces.IFunctionCompositionService, FunctionCompositionService>();

            // Register function runtimes
            services.AddSingleton<JavaScriptRuntime>();
            services.AddSingleton<PythonRuntime>();
            services.AddSingleton<CSharpRuntime>();
            services.AddSingleton<CoreInterfaces.IFunctionRuntimeFactory, FunctionRuntimeFactory>();

            // Configure function runtimes
            if (configuration != null)
            {
                services.Configure<JavaScriptRuntimeOptions>(options => configuration.GetSection("Function:Runtimes:JavaScript").Bind(options));
                services.Configure<PythonRuntimeOptions>(options => configuration.GetSection("Function:Runtimes:Python").Bind(options));
                services.Configure<CSharpRuntimeOptions>(options => configuration.GetSection("Function:Runtimes:CSharp").Bind(options));
            }

            // Initialize templates
            if (configuration != null && configuration.GetValue<bool>("Function:Enabled", false))
            {
                var serviceProvider = services.BuildServiceProvider();
                try
                {
                    var templateInitializer = serviceProvider.GetRequiredService<FunctionTemplateInitializer>();
                    templateInitializer.InitializeAsync().GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    var logger = serviceProvider.GetService<ILogger<FunctionServiceExtensions>>();
                    logger?.LogError(ex, "Error initializing function templates");
                }
            }

            return services;
        }
    }
}
