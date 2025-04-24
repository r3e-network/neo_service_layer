using System;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents a validation result for a function dependency
    /// </summary>
    public class FunctionDependencyValidationResult
    {
        /// <summary>
        /// Gets or sets the dependency ID
        /// </summary>
        public Guid DependencyId { get; set; }

        /// <summary>
        /// Gets or sets the function ID
        /// </summary>
        public Guid FunctionId { get; set; }

        /// <summary>
        /// Gets or sets the name of the dependency
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the version of the dependency
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the dependency is valid
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the validation message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the validation level (e.g., "info", "warning", "error")
        /// </summary>
        public string Level { get; set; }

        /// <summary>
        /// Gets or sets the validation code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the dependency has security vulnerabilities
        /// </summary>
        public bool HasVulnerabilities { get; set; }

        /// <summary>
        /// Gets or sets the number of vulnerabilities
        /// </summary>
        public int VulnerabilityCount { get; set; }

        /// <summary>
        /// Gets or sets the highest vulnerability severity (e.g., "low", "medium", "high", "critical")
        /// </summary>
        public string HighestVulnerabilitySeverity { get; set; }

        /// <summary>
        /// Gets or sets the URL to the vulnerability report
        /// </summary>
        public string VulnerabilityReportUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the dependency is deprecated
        /// </summary>
        public bool IsDeprecated { get; set; }

        /// <summary>
        /// Gets or sets the deprecation message
        /// </summary>
        public string DeprecationMessage { get; set; }

        /// <summary>
        /// Gets or sets the recommended alternative
        /// </summary>
        public string RecommendedAlternative { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the dependency is compatible with the function runtime
        /// </summary>
        public bool IsCompatible { get; set; }

        /// <summary>
        /// Gets or sets the compatibility message
        /// </summary>
        public string CompatibilityMessage { get; set; }
    }
}
