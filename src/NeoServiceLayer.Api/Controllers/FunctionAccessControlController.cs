using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Interfaces;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.API.Controllers
{
    /// <summary>
    /// Controller for function access control
    /// </summary>
    [ApiController]
    [Route("api/functions/access")]
    [Authorize]
    public class FunctionAccessControlController : ControllerBase
    {
        private readonly ILogger<FunctionAccessControlController> _logger;
        private readonly IFunctionAccessControlService _accessControlService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionAccessControlController"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="accessControlService">Function access control service</param>
        public FunctionAccessControlController(ILogger<FunctionAccessControlController> logger, IFunctionAccessControlService accessControlService)
        {
            _logger = logger;
            _accessControlService = accessControlService;
        }

        /// <summary>
        /// Checks if a principal has permission to perform an operation on a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="principalId">Principal ID</param>
        /// <param name="principalType">Principal type</param>
        /// <param name="operation">Operation to check</param>
        /// <returns>True if the principal has permission, false otherwise</returns>
        [HttpGet("check")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> HasPermissionAsync([FromQuery] Guid functionId, [FromQuery] string principalId, [FromQuery] string principalType, [FromQuery] string operation)
        {
            var hasPermission = await _accessControlService.HasPermissionAsync(functionId, principalId, principalType, operation);
            return Ok(hasPermission);
        }

        /// <summary>
        /// Gets permissions for a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>List of permissions</returns>
        [HttpGet("permissions/function/{functionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FunctionPermission>>> GetPermissionsAsync(Guid functionId)
        {
            var permissions = await _accessControlService.GetPermissionsAsync(functionId);
            return Ok(permissions);
        }

        /// <summary>
        /// Gets permissions for a principal
        /// </summary>
        /// <param name="principalId">Principal ID</param>
        /// <param name="principalType">Principal type</param>
        /// <returns>List of permissions</returns>
        [HttpGet("permissions/principal")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FunctionPermission>>> GetPermissionsByPrincipalAsync([FromQuery] string principalId, [FromQuery] string principalType)
        {
            var permissions = await _accessControlService.GetPermissionsByPrincipalAsync(principalId, principalType);
            return Ok(permissions);
        }

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
        [HttpPost("permissions")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<FunctionPermission>> GrantPermissionAsync(
            [FromQuery] Guid functionId,
            [FromQuery] string principalId,
            [FromQuery] string principalType,
            [FromQuery] string permissionLevel,
            [FromQuery] List<string> allowedOperations = null,
            [FromQuery] List<string> deniedOperations = null,
            [FromQuery] DateTime? expiresAt = null)
        {
            var permission = await _accessControlService.GrantPermissionAsync(functionId, principalId, principalType, permissionLevel, allowedOperations, deniedOperations, expiresAt);
            return CreatedAtAction(nameof(GetPermissionsAsync), new { functionId }, permission);
        }

        /// <summary>
        /// Revokes a permission
        /// </summary>
        /// <param name="permissionId">Permission ID</param>
        /// <returns>No content</returns>
        [HttpDelete("permissions/{permissionId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> RevokePermissionAsync(Guid permissionId)
        {
            var result = await _accessControlService.RevokePermissionAsync(permissionId);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Revokes all permissions for a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>No content</returns>
        [HttpDelete("permissions/function/{functionId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> RevokeAllPermissionsAsync(Guid functionId)
        {
            await _accessControlService.RevokeAllPermissionsAsync(functionId);
            return NoContent();
        }

        /// <summary>
        /// Revokes all permissions for a principal
        /// </summary>
        /// <param name="principalId">Principal ID</param>
        /// <param name="principalType">Principal type</param>
        /// <returns>No content</returns>
        [HttpDelete("permissions/principal")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> RevokeAllPermissionsByPrincipalAsync([FromQuery] string principalId, [FromQuery] string principalType)
        {
            await _accessControlService.RevokeAllPermissionsByPrincipalAsync(principalId, principalType);
            return NoContent();
        }

        /// <summary>
        /// Updates a permission
        /// </summary>
        /// <param name="permissionId">Permission ID</param>
        /// <param name="permission">Permission to update</param>
        /// <returns>The updated permission</returns>
        [HttpPut("permissions/{permissionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FunctionPermission>> UpdatePermissionAsync(Guid permissionId, [FromBody] FunctionPermission permission)
        {
            // Ensure the ID in the path matches the ID in the body
            if (permissionId != permission.Id)
            {
                return BadRequest("ID in the path does not match ID in the body");
            }

            var updatedPermission = await _accessControlService.UpdatePermissionAsync(permission);
            return Ok(updatedPermission);
        }

        /// <summary>
        /// Creates an access policy for a function
        /// </summary>
        /// <param name="policy">Access policy to create</param>
        /// <returns>The created access policy</returns>
        [HttpPost("policies")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<FunctionAccessPolicy>> CreateAccessPolicyAsync([FromBody] FunctionAccessPolicy policy)
        {
            var createdPolicy = await _accessControlService.CreateAccessPolicyAsync(policy);
            return CreatedAtAction(nameof(GetAccessPolicyByIdAsync), new { policyId = createdPolicy.Id }, createdPolicy);
        }

        /// <summary>
        /// Updates an access policy
        /// </summary>
        /// <param name="policyId">Policy ID</param>
        /// <param name="policy">Access policy to update</param>
        /// <returns>The updated access policy</returns>
        [HttpPut("policies/{policyId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FunctionAccessPolicy>> UpdateAccessPolicyAsync(Guid policyId, [FromBody] FunctionAccessPolicy policy)
        {
            // Ensure the ID in the path matches the ID in the body
            if (policyId != policy.Id)
            {
                return BadRequest("ID in the path does not match ID in the body");
            }

            var updatedPolicy = await _accessControlService.UpdateAccessPolicyAsync(policy);
            return Ok(updatedPolicy);
        }

        /// <summary>
        /// Gets an access policy by ID
        /// </summary>
        /// <param name="policyId">Policy ID</param>
        /// <returns>The access policy</returns>
        [HttpGet("policies/{policyId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FunctionAccessPolicy>> GetAccessPolicyByIdAsync(Guid policyId)
        {
            var policy = await _accessControlService.GetAccessPolicyByIdAsync(policyId);
            if (policy == null)
            {
                return NotFound();
            }

            return Ok(policy);
        }

        /// <summary>
        /// Gets access policies for a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>List of access policies</returns>
        [HttpGet("policies/function/{functionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FunctionAccessPolicy>>> GetAccessPoliciesAsync(Guid functionId)
        {
            var policies = await _accessControlService.GetAccessPoliciesAsync(functionId);
            return Ok(policies);
        }

        /// <summary>
        /// Deletes an access policy
        /// </summary>
        /// <param name="policyId">Policy ID</param>
        /// <returns>No content</returns>
        [HttpDelete("policies/{policyId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteAccessPolicyAsync(Guid policyId)
        {
            var result = await _accessControlService.DeleteAccessPolicyAsync(policyId);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Evaluates access policies for a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <param name="context">Access context</param>
        /// <returns>True if access is allowed, false otherwise</returns>
        [HttpPost("policies/evaluate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> EvaluateAccessPoliciesAsync([FromQuery] Guid functionId, [FromBody] Dictionary<string, object> context)
        {
            var result = await _accessControlService.EvaluateAccessPoliciesAsync(functionId, context);
            return Ok(result);
        }

        /// <summary>
        /// Creates an access request
        /// </summary>
        /// <param name="request">Access request to create</param>
        /// <returns>The created access request</returns>
        [HttpPost("requests")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<FunctionAccessRequest>> CreateAccessRequestAsync([FromBody] FunctionAccessRequest request)
        {
            var createdRequest = await _accessControlService.CreateAccessRequestAsync(request);
            return CreatedAtAction(nameof(GetAccessRequestByIdAsync), new { requestId = createdRequest.Id }, createdRequest);
        }

        /// <summary>
        /// Gets an access request by ID
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <returns>The access request</returns>
        [HttpGet("requests/{requestId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FunctionAccessRequest>> GetAccessRequestByIdAsync(Guid requestId)
        {
            var request = await _accessControlService.GetAccessRequestByIdAsync(requestId);
            if (request == null)
            {
                return NotFound();
            }

            return Ok(request);
        }

        /// <summary>
        /// Gets access requests for a function
        /// </summary>
        /// <param name="functionId">Function ID</param>
        /// <returns>List of access requests</returns>
        [HttpGet("requests/function/{functionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FunctionAccessRequest>>> GetAccessRequestsAsync(Guid functionId)
        {
            var requests = await _accessControlService.GetAccessRequestsAsync(functionId);
            return Ok(requests);
        }

        /// <summary>
        /// Gets access requests by principal
        /// </summary>
        /// <param name="principalId">Principal ID</param>
        /// <param name="principalType">Principal type</param>
        /// <returns>List of access requests</returns>
        [HttpGet("requests/principal")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FunctionAccessRequest>>> GetAccessRequestsByPrincipalAsync([FromQuery] string principalId, [FromQuery] string principalType)
        {
            var requests = await _accessControlService.GetAccessRequestsByPrincipalAsync(principalId, principalType);
            return Ok(requests);
        }

        /// <summary>
        /// Approves an access request
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <param name="approverId">Approver ID</param>
        /// <param name="reason">Approval reason</param>
        /// <param name="expiresAt">Expiration date</param>
        /// <param name="grantedOperations">Granted operations</param>
        /// <returns>The approved access request</returns>
        [HttpPost("requests/{requestId}/approve")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FunctionAccessRequest>> ApproveAccessRequestAsync(
            Guid requestId,
            [FromQuery] string approverId,
            [FromQuery] string reason,
            [FromQuery] DateTime? expiresAt = null,
            [FromQuery] List<string> grantedOperations = null)
        {
            var request = await _accessControlService.ApproveAccessRequestAsync(requestId, approverId, reason, expiresAt, grantedOperations);
            return Ok(request);
        }

        /// <summary>
        /// Rejects an access request
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <param name="approverId">Approver ID</param>
        /// <param name="reason">Rejection reason</param>
        /// <returns>The rejected access request</returns>
        [HttpPost("requests/{requestId}/reject")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FunctionAccessRequest>> RejectAccessRequestAsync(
            Guid requestId,
            [FromQuery] string approverId,
            [FromQuery] string reason)
        {
            var request = await _accessControlService.RejectAccessRequestAsync(requestId, approverId, reason);
            return Ok(request);
        }
    }
}
