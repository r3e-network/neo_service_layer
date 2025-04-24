using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents a rule for a function access policy
    /// </summary>
    public class FunctionAccessPolicyRule
    {
        /// <summary>
        /// Gets or sets the name of the rule
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the rule
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the condition type (e.g., "ip", "time", "rate", "auth")
        /// </summary>
        public string ConditionType { get; set; }

        /// <summary>
        /// Gets or sets the condition operator (e.g., "equals", "contains", "in", "between")
        /// </summary>
        public string ConditionOperator { get; set; }

        /// <summary>
        /// Gets or sets the condition value
        /// </summary>
        public object ConditionValue { get; set; }

        /// <summary>
        /// Gets or sets the action to take if the condition matches (e.g., "allow", "deny")
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the rule is enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the priority of the rule
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Gets or sets a value indicating whether to stop processing other rules if this rule matches
        /// </summary>
        public bool StopOnMatch { get; set; } = false;

        /// <summary>
        /// Gets or sets the error message to return if the rule denies access
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the error code to return if the rule denies access
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets additional parameters for the rule
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }
}
