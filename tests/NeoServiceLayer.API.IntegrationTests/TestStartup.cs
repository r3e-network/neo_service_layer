using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Services.Function;
using NeoServiceLayer.Services.Function.Repositories;
using NeoServiceLayer.Services.Storage.Providers;

namespace NeoServiceLayer.API.IntegrationTests
{
    /// <summary>
    /// Startup class for integration tests
    /// </summary>
    public class TestStartup
    {
        public IConfiguration Configuration { get; }

        public TestStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Add controllers
            services.AddControllers();

            // Add test authentication
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

            // Add authorization
            services.AddAuthorization();

            // Add in-memory storage provider
            services.AddSingleton<IStorageProvider, InMemoryStorageProvider>();

            // Add function services
            services.AddSingleton<IFunctionRepository, FunctionRepository>();
            services.AddSingleton<IFunctionExecutionRepository, FunctionExecutionRepository>();
            services.AddSingleton<IFunctionLogRepository, FunctionLogRepository>();
            services.AddSingleton<IFunctionTemplateRepository, FunctionTemplateRepository>();
            services.AddSingleton<IFunctionService, FunctionService>();

            // Add function test services
            services.AddSingleton<IFunctionTestRepository, FunctionTestRepository>();
            services.AddSingleton<IFunctionTestResultRepository, FunctionTestResultRepository>();
            services.AddSingleton<IFunctionTestSuiteRepository, FunctionTestSuiteRepository>();
            services.AddSingleton<IFunctionTestService, FunctionTestService>();

            // Add function access control services
            services.AddSingleton<IFunctionPermissionRepository, FunctionPermissionRepository>();
            services.AddSingleton<IFunctionAccessPolicyRepository, FunctionAccessPolicyRepository>();
            services.AddSingleton<IFunctionAccessRequestRepository, FunctionAccessRequestRepository>();
            services.AddSingleton<IFunctionAccessControlService, FunctionAccessControlService>();

            // Add function marketplace services
            services.AddSingleton<IFunctionMarketplaceItemRepository, FunctionMarketplaceItemRepository>();
            services.AddSingleton<IFunctionMarketplaceReviewRepository, FunctionMarketplaceReviewRepository>();
            services.AddSingleton<IFunctionMarketplacePurchaseRepository, FunctionMarketplacePurchaseRepository>();
            services.AddSingleton<IFunctionMarketplaceService, FunctionMarketplaceService>();

            // Add function composition services
            services.AddSingleton<IFunctionCompositionRepository, FunctionCompositionRepository>();
            services.AddSingleton<IFunctionCompositionExecutionRepository, FunctionCompositionExecutionRepository>();
            services.AddSingleton<IFunctionCompositionService, FunctionCompositionService>();
            services.AddSingleton<IFunctionExecutor, TestFunctionExecutor>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    /// <summary>
    /// Test function executor for integration tests
    /// </summary>
    public class TestFunctionExecutor : IFunctionExecutor
    {
        public Task<object> ExecuteAsync(Guid functionId, Dictionary<string, object> parameters, FunctionExecutionContext context)
        {
            // Return a mock result
            return Task.FromResult<object>(new Dictionary<string, object>
            {
                { "result", "success" },
                { "executedAt", DateTime.UtcNow },
                { "parameters", parameters }
            });
        }
    }
}
