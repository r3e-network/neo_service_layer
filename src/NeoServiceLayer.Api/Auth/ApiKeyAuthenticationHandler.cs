using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NeoServiceLayer.API.Auth
{
    /// <summary>
    /// Authentication handler for API keys
    /// </summary>
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        private readonly ILogger<ApiKeyAuthenticationHandler> _logger;
        private readonly AuthOptions _authOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiKeyAuthenticationHandler"/> class
        /// </summary>
        /// <param name="options">Options</param>
        /// <param name="logger">Logger</param>
        /// <param name="encoder">URL encoder</param>
        /// <param name="clock">System clock</param>
        /// <param name="authOptions">Authentication options</param>
        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<ApiKeyAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IOptions<AuthOptions> authOptions)
            : base(options, logger, encoder, clock)
        {
            _logger = logger.CreateLogger<ApiKeyAuthenticationHandler>();
            _authOptions = authOptions.Value;
        }

        /// <inheritdoc/>
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                _logger.LogInformation("Authenticating request with API key");

                // Get API key from header
                if (!Request.Headers.TryGetValue(_authOptions.ApiKey.HeaderName, out var apiKeyHeaderValues))
                {
                    // If header not found, try query parameter if allowed
                    if (_authOptions.ApiKey.AllowQueryParam && Request.Query.TryGetValue(_authOptions.ApiKey.QueryParamName, out var apiKeyQueryValues))
                    {
                        return ValidateApiKey(apiKeyQueryValues.FirstOrDefault());
                    }

                    _logger.LogWarning("API key not found in request");
                    return Task.FromResult(AuthenticateResult.Fail("API key not found"));
                }

                var apiKey = apiKeyHeaderValues.FirstOrDefault();
                return ValidateApiKey(apiKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating request with API key: {Message}", ex.Message);
                return Task.FromResult(AuthenticateResult.Fail("Authentication failed"));
            }
        }

        private Task<AuthenticateResult> ValidateApiKey(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("API key is empty");
                return Task.FromResult(AuthenticateResult.Fail("API key is empty"));
            }

            // Check if API key exists
            if (!_authOptions.ApiKey.ApiKeys.TryGetValue(apiKey, out var apiKeyInfo))
            {
                _logger.LogWarning("Invalid API key");
                return Task.FromResult(AuthenticateResult.Fail("Invalid API key"));
            }

            // Check if API key is active
            if (!apiKeyInfo.IsActive)
            {
                _logger.LogWarning("API key is inactive");
                return Task.FromResult(AuthenticateResult.Fail("API key is inactive"));
            }

            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, apiKeyInfo.OwnerId),
                new Claim(ClaimTypes.Name, apiKeyInfo.OwnerName),
                new Claim("api_key", apiKey)
            };

            // Add roles
            foreach (var role in apiKeyInfo.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Add permissions
            foreach (var permission in apiKeyInfo.Permissions)
            {
                claims.Add(new Claim("permission", permission));
            }

            // Create identity and principal
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);

            // Create ticket
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            _logger.LogInformation("API key authentication successful for {OwnerName}", apiKeyInfo.OwnerName);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    /// <summary>
    /// Options for API key authentication
    /// </summary>
    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
    }
}
