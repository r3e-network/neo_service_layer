using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Services.Function
{
    /// <summary>
    /// Service for managing function access control
    /// </summary>
    public class FunctionAccessControlService : IFunctionAccessControlService
    {
        private readonly ILogger<FunctionAccessControlService> _logger;
        private readonly IFunctionPermissionRepository _permissionRepository;
        private readonly IFunctionAccessPolicyRepository _policyRepository;
        private readonly IFunctionAccessRequestRepository _requestRepository;
        private readonly IFunctionRepository _functionRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionAccessControlService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="permissionRepository">Permission repository</param>
        /// <param name="policyRepository">Policy repository</param>
        /// <param name="requestRepository">Request repository</param>
        /// <param name="functionRepository">Function repository</param>
        public FunctionAccessControlService(
            ILogger<FunctionAccessControlService> logger,
            IFunctionPermissionRepository permissionRepository,
            IFunctionAccessPolicyRepository policyRepository,
            IFunctionAccessRequestRepository requestRepository,
            IFunctionRepository functionRepository)
        {
            _logger = logger;
            _permissionRepository = permissionRepository;
            _policyRepository = policyRepository;
            _requestRepository = requestRepository;
            _functionRepository = functionRepository;
        }

        /// <inheritdoc/>
        public async Task<bool> HasPermissionAsync(Guid functionId, string principalId, string principalType, string operation, Dictionary<string, object> context = null)
        {
            _logger.LogInformation("Checking if principal {PrincipalId} of type {PrincipalType} has permission to perform {Operation} on function {FunctionId}", principalId, principalType, operation, functionId);

            try
            {
                // Check if the function exists
                var function = await _functionRepository.GetByIdAsync(functionId);
                if (function == null)
                {
                    _logger.LogWarning("Function not found: {FunctionId}", functionId);
                    return false;
                }

                // Check if the principal is the owner of the function
                if (function.AccountId.ToString() == principalId && principalType == "user")
                {
                    _logger.LogInformation("Principal {PrincipalId} is the owner of function {FunctionId}", principalId, functionId);
                    return true;
                }

                // Get permissions for the principal
                var permissions = await _permissionRepository.GetByFunctionIdAndPrincipalAsync(functionId, principalId, principalType);
                if (!permissions.Any())
                {
                    _logger.LogInformation("No permissions found for principal {PrincipalId} of type {PrincipalType} on function {FunctionId}", principalId, principalType, functionId);
                    return false;
                }

                // Check if any permission allows the operation
                foreach (var permission in permissions)
                {
                    // Skip inactive or expired permissions
                    if (!permission.IsActive || (permission.ExpiresAt.HasValue && permission.ExpiresAt.Value < DateTime.UtcNow))
                    {
                        continue;
                    }

                    // Check if the operation is explicitly denied
                    if (permission.DeniedOperations.Contains(operation))
                    {
                        _logger.LogInformation("Operation {Operation} is explicitly denied for principal {PrincipalId} of type {PrincipalType} on function {FunctionId}", operation, principalId, principalType, functionId);
                        return false;
                    }

                    // Check if the operation is explicitly allowed
                    if (permission.AllowedOperations.Contains(operation) || permission.AllowedOperations.Contains("*"))
                    {
                        _logger.LogInformation("Operation {Operation} is explicitly allowed for principal {PrincipalId} of type {PrincipalType} on function {FunctionId}", operation, principalId, principalType, functionId);
                        return true;
                    }

                    // Check if the permission level allows the operation
                    if (IsOperationAllowedByPermissionLevel(permission.PermissionLevel, operation))
                    {
                        _logger.LogInformation("Operation {Operation} is allowed by permission level {PermissionLevel} for principal {PrincipalId} of type {PrincipalType} on function {FunctionId}", operation, permission.PermissionLevel, principalId, principalType, functionId);
                        return true;
                    }
                }

                // If we get here, no permission allows the operation
                _logger.LogInformation("No permission allows operation {Operation} for principal {PrincipalId} of type {PrincipalType} on function {FunctionId}", operation, principalId, principalType, functionId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if principal {PrincipalId} of type {PrincipalType} has permission to perform {Operation} on function {FunctionId}", principalId, principalType, operation, functionId);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionPermission>> GetPermissionsAsync(Guid functionId)
        {
            _logger.LogInformation("Getting permissions for function {FunctionId}", functionId);

            try
            {
                return await _permissionRepository.GetByFunctionIdAsync(functionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permissions for function {FunctionId}", functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionPermission>> GetPermissionsByPrincipalAsync(string principalId, string principalType)
        {
            _logger.LogInformation("Getting permissions for principal {PrincipalId} of type {PrincipalType}", principalId, principalType);

            try
            {
                return await _permissionRepository.GetByPrincipalAsync(principalId, principalType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permissions for principal {PrincipalId} of type {PrincipalType}", principalId, principalType);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionPermission> GrantPermissionAsync(Guid functionId, string principalId, string principalType, string permissionLevel, List<string> allowedOperations = null, List<string> deniedOperations = null, DateTime? expiresAt = null)
        {
            _logger.LogInformation("Granting permission level {PermissionLevel} to principal {PrincipalId} of type {PrincipalType} for function {FunctionId}", permissionLevel, principalId, principalType, functionId);

            try
            {
                // Check if the function exists
                var function = await _functionRepository.GetByIdAsync(functionId);
                if (function == null)
                {
                    throw new Exception($"Function not found: {functionId}");
                }

                // Check if a permission already exists
                var existingPermissions = await _permissionRepository.GetByFunctionIdAndPrincipalAsync(functionId, principalId, principalType);
                var existingPermission = existingPermissions.FirstOrDefault();

                if (existingPermission != null)
                {
                    // Update the existing permission
                    existingPermission.PermissionLevel = permissionLevel;
                    existingPermission.AllowedOperations = allowedOperations ?? existingPermission.AllowedOperations;
                    existingPermission.DeniedOperations = deniedOperations ?? existingPermission.DeniedOperations;
                    existingPermission.ExpiresAt = expiresAt;
                    existingPermission.UpdatedAt = DateTime.UtcNow;
                    existingPermission.IsActive = true;

                    return await _permissionRepository.UpdateAsync(existingPermission);
                }
                else
                {
                    // Create a new permission
                    var permission = new FunctionPermission
                    {
                        Id = Guid.NewGuid(),
                        FunctionId = functionId,
                        PrincipalId = principalId,
                        PrincipalType = principalType,
                        PermissionLevel = permissionLevel,
                        AllowedOperations = allowedOperations ?? new List<string>(),
                        DeniedOperations = deniedOperations ?? new List<string>(),
                        ExpiresAt = expiresAt,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    return await _permissionRepository.CreateAsync(permission);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error granting permission level {PermissionLevel} to principal {PrincipalId} of type {PrincipalType} for function {FunctionId}", permissionLevel, principalId, principalType, functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> RevokePermissionAsync(Guid permissionId)
        {
            _logger.LogInformation("Revoking permission {PermissionId}", permissionId);

            try
            {
                return await _permissionRepository.DeleteAsync(permissionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking permission {PermissionId}", permissionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> RevokeAllPermissionsAsync(Guid functionId)
        {
            _logger.LogInformation("Revoking all permissions for function {FunctionId}", functionId);

            try
            {
                return await _permissionRepository.DeleteByFunctionIdAsync(functionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking all permissions for function {FunctionId}", functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> RevokeAllPermissionsByPrincipalAsync(string principalId, string principalType)
        {
            _logger.LogInformation("Revoking all permissions for principal {PrincipalId} of type {PrincipalType}", principalId, principalType);

            try
            {
                return await _permissionRepository.DeleteByPrincipalAsync(principalId, principalType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking all permissions for principal {PrincipalId} of type {PrincipalType}", principalId, principalType);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionPermission> UpdatePermissionAsync(FunctionPermission permission)
        {
            _logger.LogInformation("Updating permission {PermissionId}", permission.Id);

            try
            {
                // Update timestamp
                permission.UpdatedAt = DateTime.UtcNow;

                return await _permissionRepository.UpdateAsync(permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating permission {PermissionId}", permission.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionAccessPolicy> CreateAccessPolicyAsync(FunctionAccessPolicy policy)
        {
            _logger.LogInformation("Creating access policy {Name} for function {FunctionId}", policy.Name, policy.FunctionId);

            try
            {
                // Check if the function exists
                var function = await _functionRepository.GetByIdAsync(policy.FunctionId);
                if (function == null)
                {
                    throw new Exception($"Function not found: {policy.FunctionId}");
                }

                // Set default values
                policy.Id = Guid.NewGuid();
                policy.CreatedAt = DateTime.UtcNow;
                policy.UpdatedAt = DateTime.UtcNow;

                return await _policyRepository.CreateAsync(policy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating access policy {Name} for function {FunctionId}", policy.Name, policy.FunctionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionAccessPolicy> UpdateAccessPolicyAsync(FunctionAccessPolicy policy)
        {
            _logger.LogInformation("Updating access policy {PolicyId}", policy.Id);

            try
            {
                // Update timestamp
                policy.UpdatedAt = DateTime.UtcNow;

                return await _policyRepository.UpdateAsync(policy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating access policy {PolicyId}", policy.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionAccessPolicy> GetAccessPolicyByIdAsync(Guid policyId)
        {
            _logger.LogInformation("Getting access policy {PolicyId}", policyId);

            try
            {
                return await _policyRepository.GetByIdAsync(policyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting access policy {PolicyId}", policyId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionAccessPolicy>> GetAccessPoliciesAsync(Guid functionId)
        {
            _logger.LogInformation("Getting access policies for function {FunctionId}", functionId);

            try
            {
                return await _policyRepository.GetByFunctionIdAsync(functionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting access policies for function {FunctionId}", functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAccessPolicyAsync(Guid policyId)
        {
            _logger.LogInformation("Deleting access policy {PolicyId}", policyId);

            try
            {
                return await _policyRepository.DeleteAsync(policyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting access policy {PolicyId}", policyId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> EvaluateAccessPoliciesAsync(Guid functionId, Dictionary<string, object> context)
        {
            _logger.LogInformation("Evaluating access policies for function {FunctionId}", functionId);

            try
            {
                // Get the policies for the function
                var policies = await _policyRepository.GetByFunctionIdAsync(functionId);
                if (!policies.Any())
                {
                    _logger.LogInformation("No access policies found for function {FunctionId}", functionId);
                    return true; // No policies means access is allowed
                }

                // Sort policies by priority
                var sortedPolicies = policies.OrderBy(p => p.Priority).ToList();

                // Evaluate each policy
                foreach (var policy in sortedPolicies)
                {
                    // Skip disabled policies
                    if (!policy.IsEnabled)
                    {
                        continue;
                    }

                    // Check if the policy applies to the current environment
                    if (policy.ApplicableEnvironments.Any() && context.TryGetValue("environment", out var environment))
                    {
                        if (!policy.ApplicableEnvironments.Contains(environment.ToString()))
                        {
                            continue;
                        }
                    }

                    // Evaluate the policy rules
                    var policyResult = EvaluatePolicyRules(policy, context);

                    // If the policy has a result and is set to stop on match, return the result
                    if (policyResult.HasValue && policy.StopOnMatch)
                    {
                        return policyResult.Value;
                    }

                    // If the policy has a result, store it for later
                    if (policyResult.HasValue)
                    {
                        // If any policy denies access, deny access
                        if (!policyResult.Value)
                        {
                            return false;
                        }
                    }
                }

                // If we get here, no policy denied access
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating access policies for function {FunctionId}", functionId);
                return false; // Deny access on error
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionAccessRequest> CreateAccessRequestAsync(FunctionAccessRequest request)
        {
            _logger.LogInformation("Creating access request for principal {PrincipalId} of type {PrincipalType} for function {FunctionId}", request.PrincipalId, request.PrincipalType, request.FunctionId);

            try
            {
                // Check if the function exists
                var function = await _functionRepository.GetByIdAsync(request.FunctionId);
                if (function == null)
                {
                    throw new Exception($"Function not found: {request.FunctionId}");
                }

                // Set default values
                request.Id = Guid.NewGuid();
                request.CreatedAt = DateTime.UtcNow;
                request.UpdatedAt = DateTime.UtcNow;
                request.Status = "pending";

                return await _requestRepository.CreateAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating access request for principal {PrincipalId} of type {PrincipalType} for function {FunctionId}", request.PrincipalId, request.PrincipalType, request.FunctionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionAccessRequest> UpdateAccessRequestAsync(FunctionAccessRequest request)
        {
            _logger.LogInformation("Updating access request {RequestId}", request.Id);

            try
            {
                // Update timestamp
                request.UpdatedAt = DateTime.UtcNow;

                return await _requestRepository.UpdateAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating access request {RequestId}", request.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionAccessRequest> GetAccessRequestByIdAsync(Guid requestId)
        {
            _logger.LogInformation("Getting access request {RequestId}", requestId);

            try
            {
                return await _requestRepository.GetByIdAsync(requestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting access request {RequestId}", requestId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionAccessRequest>> GetAccessRequestsAsync(Guid functionId)
        {
            _logger.LogInformation("Getting access requests for function {FunctionId}", functionId);

            try
            {
                return await _requestRepository.GetByFunctionIdAsync(functionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting access requests for function {FunctionId}", functionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FunctionAccessRequest>> GetAccessRequestsByPrincipalAsync(string principalId, string principalType)
        {
            _logger.LogInformation("Getting access requests for principal {PrincipalId} of type {PrincipalType}", principalId, principalType);

            try
            {
                return await _requestRepository.GetByPrincipalAsync(principalId, principalType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting access requests for principal {PrincipalId} of type {PrincipalType}", principalId, principalType);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionAccessRequest> ApproveAccessRequestAsync(Guid requestId, string approverId, string reason, DateTime? expiresAt = null, List<string> grantedOperations = null)
        {
            _logger.LogInformation("Approving access request {RequestId} by approver {ApproverId}", requestId, approverId);

            try
            {
                // Get the request
                var request = await _requestRepository.GetByIdAsync(requestId);
                if (request == null)
                {
                    throw new Exception($"Access request not found: {requestId}");
                }

                // Check if the request is already approved or rejected
                if (request.Status != "pending")
                {
                    throw new Exception($"Access request is not pending: {requestId}");
                }

                // Update the request
                request.Status = "approved";
                request.ApproverId = approverId;
                request.ApprovalReason = reason;
                request.UpdatedAt = DateTime.UtcNow;
                request.GrantedExpiresAt = expiresAt ?? request.RequestedExpiresAt;
                request.GrantedOperations = grantedOperations ?? request.RequestedOperations;

                // Save the updated request
                var updatedRequest = await _requestRepository.UpdateAsync(request);

                // Grant the permission
                await GrantPermissionAsync(
                    request.FunctionId,
                    request.PrincipalId,
                    request.PrincipalType,
                    request.RequestedPermissionLevel,
                    request.GrantedOperations,
                    null,
                    request.GrantedExpiresAt);

                return updatedRequest;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving access request {RequestId} by approver {ApproverId}", requestId, approverId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FunctionAccessRequest> RejectAccessRequestAsync(Guid requestId, string approverId, string reason)
        {
            _logger.LogInformation("Rejecting access request {RequestId} by approver {ApproverId}", requestId, approverId);

            try
            {
                // Get the request
                var request = await _requestRepository.GetByIdAsync(requestId);
                if (request == null)
                {
                    throw new Exception($"Access request not found: {requestId}");
                }

                // Check if the request is already approved or rejected
                if (request.Status != "pending")
                {
                    throw new Exception($"Access request is not pending: {requestId}");
                }

                // Update the request
                request.Status = "rejected";
                request.ApproverId = approverId;
                request.ApprovalReason = reason;
                request.UpdatedAt = DateTime.UtcNow;

                // Save the updated request
                return await _requestRepository.UpdateAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting access request {RequestId} by approver {ApproverId}", requestId, approverId);
                throw;
            }
        }

        /// <summary>
        /// Checks if an operation is allowed by a permission level
        /// </summary>
        /// <param name="permissionLevel">Permission level</param>
        /// <param name="operation">Operation to check</param>
        /// <returns>True if the operation is allowed, false otherwise</returns>
        private bool IsOperationAllowedByPermissionLevel(string permissionLevel, string operation)
        {
            switch (permissionLevel.ToLower())
            {
                case "admin":
                    // Admin can do anything
                    return true;

                case "write":
                    // Write can read, write, and execute
                    return operation.ToLower() == "read" || operation.ToLower() == "write" || operation.ToLower() == "execute";

                case "execute":
                    // Execute can read and execute
                    return operation.ToLower() == "read" || operation.ToLower() == "execute";

                case "read":
                    // Read can only read
                    return operation.ToLower() == "read";

                default:
                    // Unknown permission level
                    return false;
            }
        }

        /// <summary>
        /// Evaluates the rules of a policy
        /// </summary>
        /// <param name="policy">Policy to evaluate</param>
        /// <param name="context">Access context</param>
        /// <returns>True if access is allowed, false if access is denied, null if no rule matches</returns>
        private bool? EvaluatePolicyRules(FunctionAccessPolicy policy, Dictionary<string, object> context)
        {
            // Sort rules by priority
            var sortedRules = policy.Rules.OrderBy(r => r.Priority).ToList();

            // Evaluate each rule
            foreach (var rule in sortedRules)
            {
                // Skip disabled rules
                if (!rule.IsEnabled)
                {
                    continue;
                }

                // Evaluate the rule
                var ruleResult = EvaluateRule(rule, context);

                // If the rule has a result and is set to stop on match, return the result
                if (ruleResult.HasValue && rule.StopOnMatch)
                {
                    return ruleResult.Value ? true : false;
                }

                // If the rule has a result, store it for later
                if (ruleResult.HasValue)
                {
                    // If any rule denies access, deny access
                    if (!ruleResult.Value)
                    {
                        return false;
                    }
                }
            }

            // If we get here, no rule denied access
            // Return the default action
            return policy.DefaultAction.ToLower() == "allow";
        }

        /// <summary>
        /// Evaluates a rule
        /// </summary>
        /// <param name="rule">Rule to evaluate</param>
        /// <param name="context">Access context</param>
        /// <returns>True if the rule allows access, false if the rule denies access, null if the rule doesn't apply</returns>
        private bool? EvaluateRule(FunctionAccessPolicyRule rule, Dictionary<string, object> context)
        {
            // Check if the context contains the condition type
            if (!context.TryGetValue(rule.ConditionType, out var contextValue))
            {
                return null; // Rule doesn't apply
            }

            // Evaluate the condition
            bool conditionMet = false;

            switch (rule.ConditionOperator.ToLower())
            {
                case "equals":
                    conditionMet = contextValue.Equals(rule.ConditionValue);
                    break;

                case "notequals":
                    conditionMet = !contextValue.Equals(rule.ConditionValue);
                    break;

                case "contains":
                    conditionMet = contextValue.ToString().Contains(rule.ConditionValue.ToString());
                    break;

                case "notcontains":
                    conditionMet = !contextValue.ToString().Contains(rule.ConditionValue.ToString());
                    break;

                case "in":
                    if (rule.ConditionValue is IEnumerable<object> values)
                    {
                        conditionMet = values.Contains(contextValue);
                    }
                    break;

                case "notin":
                    if (rule.ConditionValue is IEnumerable<object> notInValues)
                    {
                        conditionMet = !notInValues.Contains(contextValue);
                    }
                    break;

                case "between":
                    if (rule.ConditionValue is Dictionary<string, object> range)
                    {
                        if (range.TryGetValue("min", out var min) && range.TryGetValue("max", out var max))
                        {
                            if (contextValue is IComparable comparable)
                            {
                                conditionMet = comparable.CompareTo(min) >= 0 && comparable.CompareTo(max) <= 0;
                            }
                        }
                    }
                    break;

                default:
                    return null; // Unknown operator
            }

            // Return the result based on the action
            return conditionMet ? rule.Action.ToLower() == "allow" : null;
        }
    }
}
