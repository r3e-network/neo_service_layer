using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Services.Function.Repositories;
using NeoServiceLayer.Services.Function.Runtimes;
using CoreInterfaces = NeoServiceLayer.Core.Interfaces;
using ServiceRepositories = NeoServiceLayer.Services.Function.Repositories;

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
            // Register repositories
            services.AddSingleton<ServiceRepositories.IFunctionRepository, ServiceRepositories.FunctionRepository>();
            services.AddSingleton<CoreInterfaces.IFunctionExecutionRepository, ServiceRepositories.FunctionExecutionRepository>();
            services.AddSingleton<ServiceRepositories.IFunctionLogRepository, ServiceRepositories.FunctionLogRepository>();
            services.AddSingleton<ServiceRepositories.IFunctionTemplateRepository, ServiceRepositories.FunctionTemplateRepository>();
            services.AddSingleton<ServiceRepositories.IFunctionTestRepository, ServiceRepositories.FunctionTestRepository>();
            services.AddSingleton<ServiceRepositories.IFunctionTestResultRepository, ServiceRepositories.FunctionTestResultRepository>();
            services.AddSingleton<ServiceRepositories.IFunctionTestSuiteRepository, ServiceRepositories.FunctionTestSuiteRepository>();
            services.AddSingleton<ServiceRepositories.IFunctionPermissionRepository, ServiceRepositories.FunctionPermissionRepository>();
            services.AddSingleton<ServiceRepositories.IFunctionAccessPolicyRepository, ServiceRepositories.FunctionAccessPolicyRepository>();
            services.AddSingleton<ServiceRepositories.IFunctionAccessRequestRepository, ServiceRepositories.FunctionAccessRequestRepository>();
            services.AddSingleton<ServiceRepositories.IFunctionMarketplaceItemRepository, ServiceRepositories.FunctionMarketplaceItemRepository>();
            services.AddSingleton<ServiceRepositories.IFunctionMarketplaceReviewRepository, ServiceRepositories.FunctionMarketplaceReviewRepository>();
            services.AddSingleton<ServiceRepositories.IFunctionMarketplacePurchaseRepository, ServiceRepositories.FunctionMarketplacePurchaseRepository>();
            services.AddSingleton<ServiceRepositories.IFunctionCompositionRepository, ServiceRepositories.FunctionCompositionRepository>();
            services.AddSingleton<ServiceRepositories.IFunctionCompositionExecutionRepository, ServiceRepositories.FunctionCompositionExecutionRepository>();

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
            var serviceProvider = services.BuildServiceProvider();
            var templateInitializer = serviceProvider.GetRequiredService<FunctionTemplateInitializer>();
            templateInitializer.InitializeAsync().GetAwaiter().GetResult();

            return services;
        }
    }
}
