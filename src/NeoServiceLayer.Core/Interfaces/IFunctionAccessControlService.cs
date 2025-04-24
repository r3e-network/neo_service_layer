using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for function access control service
    /// </summary>
    public interface IFunctionAccessControlService
    {
        /// <summary>
        /// Checks if a principal has permission to perform an operation on a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="principalId">Principal ID</param>
        /// <param name="principalType">Principal type</param>
        /// <param name="operation">Operation to check</param>
        /// <param name="context">Access context</param>
        /// <returns>True if the principal has permission, false otherwise</returns>
        Task<bool> HasPermissionAsync(Guid functionId, string principalId, string principalType, string operation, Dictionary<string, object> context = null);

        /// <summary>
        /// Gets permissions for a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>List of permissions</returns>
        Task<IEnumerable<FunctionPermission>> GetPermissionsAsync(Guid functionId);

        /// <summary>
        /// Gets permissions for a principal
        /// </summary>
        /// <param name="principalId">Principal ID</param>
        /// <param name="principalType">Principal type</param>
        /// <returns>List of permissions</returns>
        Task<IEnumerable<FunctionPermission>> GetPermissionsByPrincipalAsync(string principalId, string principalType);

        /// <summary>
        /// Grants a permission to a principal for a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="principalId">Principal ID</param>
        /// <param name="principalType">Principal type</param>
        /// <param name="permissionLevel">Permission level</param>
        /// <param name="allowedOperations">Allowed operations</param>
        /// <param name="deniedOperations">Denied operations</param>
        /// <param name="expiresAt">Expiration date</param>
        /// <returns>The granted permission</returns>
        Task<FunctionPermission> GrantPermissionAsync(Guid functionId, string principalId, string principalType, string permissionLevel, List<string> allowedOperations = null, List<string> deniedOperations = null, DateTime? expiresAt = null);

        /// <summary>
        /// Revokes a permission
        /// </summary>
        /// <param name="permissionId">Permission ID</param>
        /// <returns>True if the permission was revoked successfully, false otherwise</returns>
        Task<bool> RevokePermissionAsync(Guid permissionId);

        /// <summary>
        /// Revokes all permissions for a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>True if the permissions were revoked successfully, false otherwise</returns>
        Task<bool> RevokeAllPermissionsAsync(Guid functionId);

        /// <summary>
        /// Revokes all permissions for a principal
        /// </summary>
        /// <param name="principalId">Principal ID</param>
        /// <param name="principalType">Principal type</param>
        /// <returns>True if the permissions were revoked successfully, false otherwise</returns>
        Task<bool> RevokeAllPermissionsByPrincipalAsync(string principalId, string principalType);

        /// <summary>
        /// Updates a permission
        /// </summary>
        /// <param name="permission">Permission to update</param>
        /// <returns>The updated permission</returns>
        Task<FunctionPermission> UpdatePermissionAsync(FunctionPermission permission);

        /// <summary>
        /// Creates an access policy for a function
        /// </summary>
        /// <param name="policy">Access policy to create</param>
        /// <returns>The created access policy</returns>
        Task<FunctionAccessPolicy> CreateAccessPolicyAsync(FunctionAccessPolicy policy);

        /// <summary>
        /// Updates an access policy
        /// </summary>
        /// <param name="policy">Access policy to update</param>
        /// <returns>The updated access policy</returns>
        Task<FunctionAccessPolicy> UpdateAccessPolicyAsync(FunctionAccessPolicy policy);

        /// <summary>
        /// Gets an access policy by ID
        /// </summary>
        /// <param name="policyId">Policy ID</param>
        /// <returns>The access policy if found, null otherwise</returns>
        Task<FunctionAccessPolicy> GetAccessPolicyByIdAsync(Guid policyId);

        /// <summary>
        /// Gets access policies for a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>List of access policies</returns>
        Task<IEnumerable<FunctionAccessPolicy>> GetAccessPoliciesAsync(Guid functionId);

        /// <summary>
        /// Deletes an access policy
        /// </summary>
        /// <param name="policyId">Policy ID</param>
        /// <returns>True if the policy was deleted successfully, false otherwise</returns>
        Task<bool> DeleteAccessPolicyAsync(Guid policyId);

        /// <summary>
        /// Evaluates access policies for a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="context">Access context</param>
        /// <returns>True if access is allowed, false otherwise</returns>
        Task<bool> EvaluateAccessPoliciesAsync(Guid functionId, Dictionary<string, object> context);

        /// <summary>
        /// Creates an access request
        /// </summary>
        /// <param name="request">Access request to create</param>
        /// <returns>The created access request</returns>
        Task<FunctionAccessRequest> CreateAccessRequestAsync(FunctionAccessRequest request);

        /// <summary>
        /// Updates an access request
        /// </summary>
        /// <param name="request">Access request to update</param>
        /// <returns>The updated access request</returns>
        Task<FunctionAccessRequest> UpdateAccessRequestAsync(FunctionAccessRequest request);

        /// <summary>
        /// Gets an access request by ID
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <returns>The access request if found, null otherwise</returns>
        Task<FunctionAccessRequest> GetAccessRequestByIdAsync(Guid requestId);

        /// <summary>
        /// Gets access requests for a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>List of access requests</returns>
        Task<IEnumerable<FunctionAccessRequest>> GetAccessRequestsAsync(Guid functionId);

        /// <summary>
        /// Gets access requests by principal
        /// </summary>
        /// <param name="principalId">Principal ID</param>
        /// <param name="principalType">Principal type</param>
        /// <returns>List of access requests</returns>
        Task<IEnumerable<FunctionAccessRequest>> GetAccessRequestsByPrincipalAsync(string principalId, string principalType);

        /// <summary>
        /// Approves an access request
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <param name="approverId">Approver ID</param>
        /// <param name="reason">Approval reason</param>
        /// <param name="expiresAt">Expiration date</param>
        /// <param name="grantedOperations">Granted operations</param>
        /// <returns>The approved access request</returns>
        Task<FunctionAccessRequest> ApproveAccessRequestAsync(Guid requestId, string approverId, string reason, DateTime? expiresAt = null, List<string> grantedOperations = null);

        /// <summary>
        /// Rejects an access request
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <param name="approverId">Approver ID</param>
        /// <param name="reason">Rejection reason</param>
        /// <returns>The rejected access request</returns>
        Task<FunctionAccessRequest> RejectAccessRequestAsync(Guid requestId, string approverId, string reason);
    }
}
