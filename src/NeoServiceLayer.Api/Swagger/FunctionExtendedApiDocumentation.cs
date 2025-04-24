using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NeoServiceLayer.API.Swagger
{
    /// <summary>
    /// Swagger documentation for the extended function API endpoints
    /// </summary>
    public static class FunctionExtendedApiDocumentation
    {
        /// <summary>
        /// Adds Swagger documentation for the extended function API endpoints
        /// </summary>
        /// <param name="options">Swagger generation options</param>
        public static void AddFunctionExtendedApiDocumentation(this SwaggerGenOptions options)
        {
            // Function Test API
            options.SwaggerDoc("function-test-api", new OpenApiInfo
            {
                Title = "Neo Service Layer - Function Test API",
                Version = "v1",
                Description = "API for managing function tests and test suites",
                Contact = new OpenApiContact
                {
                    Name = "Neo Service Layer Team",
                    Email = "support@neoservicelayer.com",
                    Url = new Uri("https://neoservicelayer.com")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            // Function Access Control API
            options.SwaggerDoc("function-access-control-api", new OpenApiInfo
            {
                Title = "Neo Service Layer - Function Access Control API",
                Version = "v1",
                Description = "API for managing function permissions, access policies, and access requests",
                Contact = new OpenApiContact
                {
                    Name = "Neo Service Layer Team",
                    Email = "support@neoservicelayer.com",
                    Url = new Uri("https://neoservicelayer.com")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            // Function Marketplace API
            options.SwaggerDoc("function-marketplace-api", new OpenApiInfo
            {
                Title = "Neo Service Layer - Function Marketplace API",
                Version = "v1",
                Description = "API for browsing, publishing, and purchasing functions in the marketplace",
                Contact = new OpenApiContact
                {
                    Name = "Neo Service Layer Team",
                    Email = "support@neoservicelayer.com",
                    Url = new Uri("https://neoservicelayer.com")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            // Function Composition API
            options.SwaggerDoc("function-composition-api", new OpenApiInfo
            {
                Title = "Neo Service Layer - Function Composition API",
                Version = "v1",
                Description = "API for creating and executing function compositions",
                Contact = new OpenApiContact
                {
                    Name = "Neo Service Layer Team",
                    Email = "support@neoservicelayer.com",
                    Url = new Uri("https://neoservicelayer.com")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            // Add security definitions
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { }
                }
            });
        }

        /// <summary>
        /// Uses Swagger UI for the extended function API endpoints
        /// </summary>
        /// <param name="app">Application builder</param>
        public static void UseFunctionExtendedApiSwaggerUI(this IApplicationBuilder app)
        {
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/function-test-api/swagger.json", "Function Test API");
                c.SwaggerEndpoint("/swagger/function-access-control-api/swagger.json", "Function Access Control API");
                c.SwaggerEndpoint("/swagger/function-marketplace-api/swagger.json", "Function Marketplace API");
                c.SwaggerEndpoint("/swagger/function-composition-api/swagger.json", "Function Composition API");
            });
        }
    }
}
