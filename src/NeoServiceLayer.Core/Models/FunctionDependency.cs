using System;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents a dependency for a function
    /// </summary>
    public class FunctionDependency
    {
        /// <summary>
        /// Gets or sets the dependency ID
        /// </summary>
        public Guid Id { get; set; }

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
        /// Gets or sets the type of the dependency (e.g., "npm", "nuget", "pip")
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the source of the dependency (e.g., "npm", "nuget", "pip", "custom")
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the URL to the dependency package
        /// </summary>
        public string PackageUrl { get; set; }

        /// <summary>
        /// Gets or sets the hash of the dependency package
        /// </summary>
        public string PackageHash { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the dependency is required
        /// </summary>
        public bool IsRequired { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the dependency is a development dependency
        /// </summary>
        public bool IsDevelopmentDependency { get; set; } = false;

        /// <summary>
        /// Gets or sets the created at timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the updated at timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the license of the dependency
        /// </summary>
        public string License { get; set; }

        /// <summary>
        /// Gets or sets the description of the dependency
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the author of the dependency
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets the homepage of the dependency
        /// </summary>
        public string Homepage { get; set; }

        /// <summary>
        /// Gets or sets the repository URL of the dependency
        /// </summary>
        public string Repository { get; set; }
    }
}
