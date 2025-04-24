using System;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Response for token generation
    /// </summary>
    public class TokenResponse
    {
        /// <summary>
        /// Gets or sets the access token
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the refresh token
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Gets or sets the expiration date
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Gets or sets the token type
        /// </summary>
        public string TokenType { get; set; }
    }
}
