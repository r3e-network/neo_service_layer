using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for token service
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a token for a user
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="roles">Roles</param>
        /// <param name="permissions">Permissions</param>
        /// <returns>Token response</returns>
        Task<TokenResponse> GenerateTokenAsync(User user, IEnumerable<string> roles, IEnumerable<string> permissions);

        /// <summary>
        /// Refreshes a token
        /// </summary>
        /// <param name="refreshToken">Refresh token</param>
        /// <returns>Token response</returns>
        Task<TokenResponse> RefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Revokes a token
        /// </summary>
        /// <param name="refreshToken">Refresh token</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task RevokeTokenAsync(string refreshToken);

        /// <summary>
        /// Validates a token
        /// </summary>
        /// <param name="token">Token</param>
        /// <returns>Claims principal</returns>
        ClaimsPrincipal ValidateToken(string token);
    }
}
