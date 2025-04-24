using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NeoServiceLayer.API.IntegrationTests.Models;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;
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
        public Task<FunctionExecutionResult> ExecuteAsync(Guid functionId, object input, FunctionExecutionContext context)
        {
            // Return a mock result
            return Task.FromResult(new FunctionExecutionResult
            {
                Success = true,
                Result = new Dictionary<string, object>
                {
                    { "result", "success" },
                    { "executedAt", DateTime.UtcNow },
                    { "parameters", input }
                }
            });
        }

        public Task<FunctionExecutionResult> ExecuteByNameAsync(Guid accountId, string functionName, object input, FunctionExecutionContext context)
        {
            // Return a mock result
            return Task.FromResult(new FunctionExecutionResult
            {
                Success = true,
                Result = new Dictionary<string, object>
                {
                    { "result", "success" },
                    { "executedAt", DateTime.UtcNow },
                    { "parameters", input }
                }
            });
        }

        public Task<FunctionExecutionResult> ExecuteSourceAsync(string source, string runtime, string handler, object input, FunctionExecutionContext context)
        {
            // Return a mock result
            return Task.FromResult(new FunctionExecutionResult
            {
                Success = true,
                Result = new Dictionary<string, object>
                {
                    { "result", "success" },
                    { "executedAt", DateTime.UtcNow },
                    { "parameters", input }
                }
            });
        }

        public Task<FunctionValidationResult> ValidateAsync(string source, string runtime, string handler)
        {
            // Return a mock result
            return Task.FromResult(new FunctionValidationResult
            {
                IsValid = true,
                Messages = new List<string> { "Validation successful" }
            });
        }

        public Task<IEnumerable<string>> GetSupportedRuntimesAsync()
        {
            // Return a mock result
            return Task.FromResult<IEnumerable<string>>(new[] { "node", "dotnet", "python" });
        }

        public Task<FunctionRuntimeDetails> GetRuntimeDetailsAsync(string runtime)
        {
            // Return a mock result
            return Task.FromResult(new FunctionRuntimeDetails
            {
                Name = runtime,
                Version = "1.0.0",
                Description = $"{runtime} runtime",
                SupportedLanguages = new[] { runtime }
            });
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            return Task.CompletedTask;
        }
    }
}
