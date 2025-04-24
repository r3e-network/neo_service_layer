using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace NeoServiceLayer.API.Auth
{
    /// <summary>
    /// Policy provider for permission authorization
    /// </summary>
    public class PermissionAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        private const string PermissionPolicyPrefix = "Permission:";

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionAuthorizationPolicyProvider"/> class
        /// </summary>
        /// <param name="options">Authorization options</param>
        public PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
            : base(options)
        {
        }

        /// <inheritdoc/>
        public override async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            // Check if the policy name starts with the permission prefix
            if (policyName.StartsWith(PermissionPolicyPrefix, StringComparison.OrdinalIgnoreCase))
            {
                // Extract the permission from the policy name
                var permission = policyName.Substring(PermissionPolicyPrefix.Length);

                // Create a policy with the permission requirement
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new PermissionRequirement(permission))
                    .Build();

                return policy;
            }

            // If not a permission policy, use the default provider
            return await base.GetPolicyAsync(policyName);
        }
    }
}
