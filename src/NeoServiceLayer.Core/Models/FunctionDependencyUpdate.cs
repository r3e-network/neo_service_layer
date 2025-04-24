using System;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents an update for a function dependency
    /// </summary>
    public class FunctionDependencyUpdate
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
        /// Gets or sets the current version of the dependency
        /// </summary>
        public string CurrentVersion { get; set; }

        /// <summary>
        /// Gets or sets the latest version of the dependency
        /// </summary>
        public string LatestVersion { get; set; }

        /// <summary>
        /// Gets or sets the latest major version of the dependency
        /// </summary>
        public string LatestMajorVersion { get; set; }

        /// <summary>
        /// Gets or sets the latest minor version of the dependency
        /// </summary>
        public string LatestMinorVersion { get; set; }

        /// <summary>
        /// Gets or sets the latest patch version of the dependency
        /// </summary>
        public string LatestPatchVersion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the update is a major version update
        /// </summary>
        public bool IsMajorUpdate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the update is a minor version update
        /// </summary>
        public bool IsMinorUpdate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the update is a patch version update
        /// </summary>
        public bool IsPatchUpdate { get; set; }

        /// <summary>
        /// Gets or sets the release date of the latest version
        /// </summary>
        public DateTime? ReleaseDate { get; set; }

        /// <summary>
        /// Gets or sets the URL to the release notes
        /// </summary>
        public string ReleaseNotesUrl { get; set; }

        /// <summary>
        /// Gets or sets the changelog
        /// </summary>
        public string Changelog { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the update is recommended
        /// </summary>
        public bool IsRecommended { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the update is security-related
        /// </summary>
        public bool IsSecurityUpdate { get; set; }

        /// <summary>
        /// Gets or sets the severity of the update (e.g., "low", "medium", "high", "critical")
        /// </summary>
        public string Severity { get; set; }
    }
}
