using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.API.Auth
{
    /// <summary>
    /// Service for JWT token generation and validation
    /// </summary>
    public class JwtTokenService : ITokenService
    {
        private readonly ILogger<JwtTokenService> _logger;
        private readonly AuthOptions _authOptions;
        private readonly ICacheService _cacheService;

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtTokenService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="authOptions">Authentication options</param>
        /// <param name="cacheService">Cache service</param>
        public JwtTokenService(
            ILogger<JwtTokenService> logger,
            IOptions<AuthOptions> authOptions,
            ICacheService cacheService)
        {
            _logger = logger;
            _authOptions = authOptions.Value;
            _cacheService = cacheService;
        }

        /// <inheritdoc/>
        public async Task<TokenResponse> GenerateTokenAsync(User user, IEnumerable<string> roles, IEnumerable<string> permissions)
        {
            try
            {
                _logger.LogInformation("Generating token for user: {UserId}", user.Id);

                // Create claims
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Name, user.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                };

                // Add roles
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                // Add permissions
                foreach (var permission in permissions)
                {
                    claims.Add(new Claim("permission", permission));
                }

                // Create signing credentials
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authOptions.JwtSecretKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                // Create token
                var expires = DateTime.UtcNow.AddMinutes(_authOptions.JwtExpirationMinutes);
                var token = new JwtSecurityToken(
                    issuer: _authOptions.JwtIssuer,
                    audience: _authOptions.JwtAudience,
                    claims: claims,
                    expires: expires,
                    signingCredentials: creds);

                // Generate refresh token
                var refreshToken = GenerateRefreshToken();
                var refreshTokenExpires = DateTime.UtcNow.AddDays(_authOptions.JwtRefreshTokenExpirationDays);

                // Store refresh token in cache
                await _cacheService.SetAsync(
                    $"refresh_token:{refreshToken}",
                    new RefreshTokenInfo
                    {
                        UserId = user.Id,
                        RefreshToken = refreshToken,
                        ExpiresAt = refreshTokenExpires
                    },
                    TimeSpan.FromDays(_authOptions.JwtRefreshTokenExpirationDays));

                // Return token response
                return new TokenResponse
                {
                    AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                    RefreshToken = refreshToken,
                    ExpiresAt = expires,
                    TokenType = "Bearer"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating token for user {UserId}: {Message}", user.Id, ex.Message);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                _logger.LogInformation("Refreshing token");

                // Get refresh token info from cache
                var refreshTokenInfo = await _cacheService.GetAsync<RefreshTokenInfo>($"refresh_token:{refreshToken}");
                if (refreshTokenInfo == null || refreshTokenInfo.ExpiresAt < DateTime.UtcNow)
                {
                    _logger.LogWarning("Invalid or expired refresh token");
                    throw new SecurityTokenException("Invalid or expired refresh token");
                }

                // Get user
                var user = await GetUserAsync(refreshTokenInfo.UserId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for refresh token: {UserId}", refreshTokenInfo.UserId);
                    throw new SecurityTokenException("User not found");
                }

                // Get roles and permissions
                var roles = await GetUserRolesAsync(user.Id);
                var permissions = await GetUserPermissionsAsync(user.Id);

                // Revoke old refresh token
                await _cacheService.RemoveAsync($"refresh_token:{refreshToken}");

                // Generate new token
                return await GenerateTokenAsync(user, roles, permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token: {Message}", ex.Message);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task RevokeTokenAsync(string refreshToken)
        {
            try
            {
                _logger.LogInformation("Revoking token");

                // Remove refresh token from cache
                await _cacheService.RemoveAsync($"refresh_token:{refreshToken}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token: {Message}", ex.Message);
                throw;
            }
        }

        /// <inheritdoc/>
        public ClaimsPrincipal ValidateToken(string token)
        {
            try
            {
                _logger.LogInformation("Validating token");

                // Create token validation parameters
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = _authOptions.ValidateIssuer,
                    ValidateAudience = _authOptions.ValidateAudience,
                    ValidateLifetime = _authOptions.ValidateLifetime,
                    ValidateIssuerSigningKey = _authOptions.ValidateIssuerSigningKey,
                    ValidIssuer = _authOptions.JwtIssuer,
                    ValidAudience = _authOptions.JwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authOptions.JwtSecretKey)),
                    ClockSkew = TimeSpan.FromMinutes(_authOptions.ClockSkewMinutes)
                };

                // Validate token
                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

                // Check if token is a valid JWT token
                if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogWarning("Invalid token");
                    throw new SecurityTokenException("Invalid token");
                }

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token: {Message}", ex.Message);
                throw;
            }
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<User> GetUserAsync(Guid userId)
        {
            // In a real implementation, this would get the user from a repository
            // For now, we'll just return a mock user
            return new User
            {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com"
            };
        }

        private async Task<IEnumerable<string>> GetUserRolesAsync(Guid userId)
        {
            // In a real implementation, this would get the user's roles from a repository
            // For now, we'll just return some mock roles
            return new[] { "user", "admin" };
        }

        private async Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId)
        {
            // In a real implementation, this would get the user's permissions from a repository
            // For now, we'll just return some mock permissions
            return new[] { "functions:read", "functions:write", "functions:execute" };
        }
    }

    /// <summary>
    /// Refresh token information
    /// </summary>
    public class RefreshTokenInfo
    {
        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the refresh token
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Gets or sets the expiration date
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }
}
