using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents an access policy for a function
    /// </summary>
    public class FunctionAccessPolicy
    {
        /// <summary>
        /// Gets or sets the policy ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the function ID
        /// </summary>
        public Guid FunctionId { get; set; }

        /// <summary>
        /// Gets or sets the account ID that owns this policy
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the name of the policy
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the policy
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the policy is enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the policy type (e.g., "ip", "time", "rate", "auth")
        /// </summary>
        public string PolicyType { get; set; }

        /// <summary>
        /// Gets or sets the policy rules
        /// </summary>
        public List<FunctionAccessPolicyRule> Rules { get; set; } = new List<FunctionAccessPolicyRule>();

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
        /// Gets or sets the priority of the policy
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Gets or sets a value indicating whether to stop processing other policies if this policy matches
        /// </summary>
        public bool StopOnMatch { get; set; } = false;

        /// <summary>
        /// Gets or sets the default action if no rule matches (e.g., "allow", "deny")
        /// </summary>
        public string DefaultAction { get; set; } = "deny";

        /// <summary>
        /// Gets or sets the environments this policy applies to
        /// </summary>
        public List<string> ApplicableEnvironments { get; set; } = new List<string>();
    }
}
