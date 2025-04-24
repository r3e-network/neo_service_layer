using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NeoServiceLayer.API.Auth
{
    /// <summary>
    /// Authorization handler for permissions
    /// </summary>
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly ILogger<PermissionAuthorizationHandler> _logger;
        private readonly AuthOptions _authOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionAuthorizationHandler"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="authOptions">Authentication options</param>
        public PermissionAuthorizationHandler(
            ILogger<PermissionAuthorizationHandler> logger,
            IOptions<AuthOptions> authOptions)
        {
            _logger = logger;
            _authOptions = authOptions.Value;
        }

        /// <inheritdoc/>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            try
            {
                _logger.LogInformation("Checking permission: {Permission}", requirement.Permission);

                // Check if user has the required permission directly
                if (context.User.HasClaim(c => c.Type == "permission" && c.Value == requirement.Permission))
                {
                    _logger.LogInformation("User has permission: {Permission}", requirement.Permission);
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }

                // Check if user has a role that has the required permission
                var userRoles = context.User.Claims
                    .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();

                foreach (var role in userRoles)
                {
                    if (_authOptions.Rbac.Roles.TryGetValue(role, out var roleInfo) &&
                        roleInfo.Permissions.Contains(requirement.Permission))
                    {
                        _logger.LogInformation("User has role {Role} with permission: {Permission}", role, requirement.Permission);
                        context.Succeed(requirement);
                        return Task.CompletedTask;
                    }
                }

                _logger.LogWarning("User does not have permission: {Permission}", requirement.Permission);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permission {Permission}: {Message}", requirement.Permission, ex.Message);
                return Task.CompletedTask;
            }
        }
    }

    /// <summary>
    /// Requirement for permission authorization
    /// </summary>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Gets the permission
        /// </summary>
        public string Permission { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionRequirement"/> class
        /// </summary>
        /// <param name="permission">Permission</param>
        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }
}
