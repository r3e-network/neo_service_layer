using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents a permission for a function
    /// </summary>
    public class FunctionPermission
    {
        /// <summary>
        /// Gets or sets the permission ID
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
        /// Gets or sets the principal type (e.g., "user", "group", "role")
        /// </summary>
        public string PrincipalType { get; set; }

        /// <summary>
        /// Gets or sets the permission level (e.g., "read", "write", "execute", "admin")
        /// </summary>
        public string PermissionLevel { get; set; }

        /// <summary>
        /// Gets or sets the permission type (e.g., "direct", "inherited", "role-based")
        /// </summary>
        public string PermissionType { get; set; } = "direct";

        /// <summary>
        /// Gets or sets a value indicating whether the permission is inherited
        /// </summary>
        public bool IsInherited { get; set; } = false;

        /// <summary>
        /// Gets or sets the source of the inherited permission
        /// </summary>
        public string InheritedFrom { get; set; }

        /// <summary>
        /// Gets or sets the created at timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the updated at timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the created by user ID
        /// </summary>
        public Guid CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the updated by user ID
        /// </summary>
        public Guid UpdatedBy { get; set; }

        /// <summary>
        /// Gets or sets the expiration date
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the permission is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the conditions for the permission
        /// </summary>
        public Dictionary<string, object> Conditions { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the specific operations allowed
        /// </summary>
        public List<string> AllowedOperations { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the specific operations denied
        /// </summary>
        public List<string> DeniedOperations { get; set; } = new List<string>();
    }
}
