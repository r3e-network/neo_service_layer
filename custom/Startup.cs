using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Configuration;
using NeoServiceLayer.Services;
using NeoServiceLayer.Services.Account;
using NeoServiceLayer.Services.Function;
using NeoServiceLayer.Services.Wallet;
using NeoServiceLayer.Services.Secrets;
using NeoServiceLayer.Services.PriceFeed;
using NeoServiceLayer.Services.Storage;
using NeoServiceLayer.Services.EventMonitoring;
using NeoServiceLayer.Services.GasBank;


namespace NeoServiceLayer.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "NeoServiceLayer.Api", Version = "v1" });
            });

            // Add CORS
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            // Add health checks
            services.AddHealthChecks();

            // Configure database
            services.Configure<DatabaseConfiguration>(Configuration.GetSection("Database"));
            services.AddSingleton<IDatabaseService, DatabaseService>();
            services.AddSingleton<IStorageProvider, MongoDbStorageProvider>();
            services.AddSingleton<IStorageProvider, RedisStorageProvider>();

            // Add core services
            services.AddAccountServices();
            services.AddWalletServices();
            services.AddGasBankServices();
            services.AddEventMonitoringServices();
            services.AddPriceFeedServices();
            services.AddSecretsServices();
            services.AddStorageServices();

            // Add function services
            if (Configuration.GetValue<bool>("Function:Enabled", false))
            {
                services.AddFunctionServices(Configuration);
            }
            else
            {
                // Register minimal services needed for dependency resolution
                services.AddSingleton<IFunctionService, FunctionService>();
                services.AddSingleton<NeoServiceLayer.Services.Function.Repositories.IFunctionTemplateRepository, NeoServiceLayer.Services.Function.Repositories.FunctionTemplateRepository>();
                services.AddSingleton<NeoServiceLayer.Services.Function.FunctionTemplateInitializer>();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "NeoServiceLayer.Api v1"));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}
