using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents a request to access a function
    /// </summary>
    public class FunctionAccessRequest
    {
        /// <summary>
        /// Gets or sets the request ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the function ID
        /// </summary>
        public Guid FunctionId { get; set; }

        /// <summary>
        /// Gets or sets the principal ID (user or group ID)
        /// </summary>
        public string PrincipalId { get; set; }

        /// <summary>
        /// Gets or sets the requester ID (user who made the request)
        /// </summary>
        public Guid RequesterId { get; set; }

        /// <summary>
        /// Gets or sets the principal type (e.g., "user", "group", "role")
        /// </summary>
        public string PrincipalType { get; set; }

        /// <summary>
        /// Gets or sets the requested permission level (e.g., "read", "write", "execute", "admin")
        /// </summary>
        public string RequestedPermissionLevel { get; set; }

        /// <summary>
        /// Gets or sets the justification for the request
        /// </summary>
        public string Justification { get; set; }

        /// <summary>
        /// Gets or sets the status of the request (e.g., "pending", "approved", "rejected")
        /// </summary>
        public string Status { get; set; } = "pending";

        /// <summary>
        /// Gets or sets the approver ID
        /// </summary>
        public string ApproverId { get; set; }

        /// <summary>
        /// Gets or sets the approval or rejection reason
        /// </summary>
        public string ApprovalReason { get; set; }

        /// <summary>
        /// Gets or sets the created at timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the updated at timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the requested expiration date
        /// </summary>
        public DateTime? RequestedExpiresAt { get; set; }

        /// <summary>
        /// Gets or sets the granted expiration date
        /// </summary>
        public DateTime? GrantedExpiresAt { get; set; }

        /// <summary>
        /// Gets or sets the requested operations
        /// </summary>
        public List<string> RequestedOperations { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the granted operations
        /// </summary>
        public List<string> GrantedOperations { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets additional metadata for the request
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
}
