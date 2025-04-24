using System.Collections.Generic;

namespace NeoServiceLayer.API.Auth
{
    /// <summary>
    /// Options for authentication and authorization
    /// </summary>
    public class AuthOptions
    {
        /// <summary>
        /// Gets or sets the JWT issuer
        /// </summary>
        public string JwtIssuer { get; set; }

        /// <summary>
        /// Gets or sets the JWT audience
        /// </summary>
        public string JwtAudience { get; set; }

        /// <summary>
        /// Gets or sets the JWT secret key
        /// </summary>
        public string JwtSecretKey { get; set; }

        /// <summary>
        /// Gets or sets the JWT expiration in minutes
        /// </summary>
        public int JwtExpirationMinutes { get; set; } = 60;

        /// <summary>
        /// Gets or sets the JWT refresh token expiration in days
        /// </summary>
        public int JwtRefreshTokenExpirationDays { get; set; } = 7;

        /// <summary>
        /// Gets or sets whether to validate the issuer
        /// </summary>
        public bool ValidateIssuer { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to validate the audience
        /// </summary>
        public bool ValidateAudience { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to validate the lifetime
        /// </summary>
        public bool ValidateLifetime { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to validate the signing key
        /// </summary>
        public bool ValidateIssuerSigningKey { get; set; } = true;

        /// <summary>
        /// Gets or sets the clock skew in minutes
        /// </summary>
        public int ClockSkewMinutes { get; set; } = 5;

        /// <summary>
        /// Gets or sets the authentication provider type
        /// </summary>
        public string ProviderType { get; set; } = "JWT";

        /// <summary>
        /// Gets or sets the OAuth2 options
        /// </summary>
        public OAuth2Options OAuth2 { get; set; } = new OAuth2Options();

        /// <summary>
        /// Gets or sets the API key options
        /// </summary>
        public ApiKeyOptions ApiKey { get; set; } = new ApiKeyOptions();

        /// <summary>
        /// Gets or sets the role-based access control options
        /// </summary>
        public RbacOptions Rbac { get; set; } = new RbacOptions();
    }

    /// <summary>
    /// Options for OAuth2 authentication
    /// </summary>
    public class OAuth2Options
    {
        /// <summary>
        /// Gets or sets the authority URL
        /// </summary>
        public string Authority { get; set; }

        /// <summary>
        /// Gets or sets the client ID
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the client secret
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the required scopes
        /// </summary>
        public List<string> RequiredScopes { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the callback path
        /// </summary>
        public string CallbackPath { get; set; } = "/signin-oauth";

        /// <summary>
        /// Gets or sets whether to save tokens
        /// </summary>
        public bool SaveTokens { get; set; } = true;
    }

    /// <summary>
    /// Options for API key authentication
    /// </summary>
    public class ApiKeyOptions
    {
        /// <summary>
        /// Gets or sets the header name
        /// </summary>
        public string HeaderName { get; set; } = "X-API-Key";

        /// <summary>
        /// Gets or sets the query parameter name
        /// </summary>
        public string QueryParamName { get; set; } = "api_key";

        /// <summary>
        /// Gets or sets whether to allow query parameter
        /// </summary>
        public bool AllowQueryParam { get; set; } = false;

        /// <summary>
        /// Gets or sets the API keys
        /// </summary>
        public Dictionary<string, ApiKeyInfo> ApiKeys { get; set; } = new Dictionary<string, ApiKeyInfo>();
    }

    /// <summary>
    /// API key information
    /// </summary>
    public class ApiKeyInfo
    {
        /// <summary>
        /// Gets or sets the owner name
        /// </summary>
        public string OwnerName { get; set; }

        /// <summary>
        /// Gets or sets the owner ID
        /// </summary>
        public string OwnerId { get; set; }

        /// <summary>
        /// Gets or sets the roles
        /// </summary>
        public List<string> Roles { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the permissions
        /// </summary>
        public List<string> Permissions { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets whether the API key is active
        /// </summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Options for role-based access control
    /// </summary>
    public class RbacOptions
    {
        /// <summary>
        /// Gets or sets the roles
        /// </summary>
        public Dictionary<string, RoleInfo> Roles { get; set; } = new Dictionary<string, RoleInfo>();

        /// <summary>
        /// Gets or sets the permissions
        /// </summary>
        public Dictionary<string, PermissionInfo> Permissions { get; set; } = new Dictionary<string, PermissionInfo>();
    }

    /// <summary>
    /// Role information
    /// </summary>
    public class RoleInfo
    {
        /// <summary>
        /// Gets or sets the role name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the role description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the permissions
        /// </summary>
        public List<string> Permissions { get; set; } = new List<string>();
    }

    /// <summary>
    /// Permission information
    /// </summary>
    public class PermissionInfo
    {
        /// <summary>
        /// Gets or sets the permission name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the permission description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the resource type
        /// </summary>
        public string ResourceType { get; set; }

        /// <summary>
        /// Gets or sets the action
        /// </summary>
        public string Action { get; set; }
    }
}
