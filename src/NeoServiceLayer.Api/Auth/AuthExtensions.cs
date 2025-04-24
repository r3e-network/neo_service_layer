using System;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NeoServiceLayer.Core.Interfaces;

namespace NeoServiceLayer.API.Auth
{
    /// <summary>
    /// Extension methods for authentication and authorization
    /// </summary>
    public static class AuthExtensions
    {
        /// <summary>
        /// Adds authentication and authorization services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddAuthServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add auth options
            services.Configure<AuthOptions>(configuration.GetSection("Auth"));
            
            // Get auth options
            var authOptions = configuration.GetSection("Auth").Get<AuthOptions>();
            
            if (authOptions == null)
            {
                throw new ArgumentException("Auth options not found in configuration");
            }

            // Add token service
            services.AddScoped<ITokenService, JwtTokenService>();

            // Add authentication
            var authBuilder = services.AddAuthentication();

            // Add JWT authentication
            authBuilder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = authOptions.ValidateIssuer,
                    ValidateAudience = authOptions.ValidateAudience,
                    ValidateLifetime = authOptions.ValidateLifetime,
                    ValidateIssuerSigningKey = authOptions.ValidateIssuerSigningKey,
                    ValidIssuer = authOptions.JwtIssuer,
                    ValidAudience = authOptions.JwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.JwtSecretKey)),
                    ClockSkew = TimeSpan.FromMinutes(authOptions.ClockSkewMinutes)
                };
            });

            // Add API key authentication
            authBuilder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>("ApiKey", options => { });

            // Add authorization
            services.AddAuthorization(options =>
            {
                // Add default policy
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

                // Add policies for roles
                foreach (var role in authOptions.Rbac.Roles.Keys)
                {
                    options.AddPolicy($"Role:{role}", policy => policy.RequireRole(role));
                }

                // Add policies for permissions
                foreach (var permission in authOptions.Rbac.Permissions.Keys)
                {
                    options.AddPolicy($"Permission:{permission}", policy => 
                        policy.RequireAuthenticatedUser()
                              .AddRequirements(new PermissionRequirement(permission)));
                }
            });

            // Add authorization handlers
            services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

            // Add authorization policy provider
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

            return services;
        }
    }
}
