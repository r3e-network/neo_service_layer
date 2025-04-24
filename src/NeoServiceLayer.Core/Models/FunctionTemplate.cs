using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents a function template
    /// </summary>
    public class FunctionTemplate
    {
        /// <summary>
        /// Gets or sets the template ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the template name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the template description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the runtime
        /// </summary>
        public string Runtime { get; set; }

        /// <summary>
        /// Gets or sets the category
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the tags
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the source code
        /// </summary>
        public string SourceCode { get; set; }

        /// <summary>
        /// Gets or sets the handler
        /// </summary>
        public string Handler { get; set; }

        /// <summary>
        /// Gets or sets the entry point
        /// </summary>
        public string EntryPoint { get; set; }

        /// <summary>
        /// Gets or sets the default environment variables
        /// </summary>
        public Dictionary<string, string> DefaultEnvironmentVariables { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the required secrets
        /// </summary>
        public List<string> RequiredSecrets { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the created at timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the updated at timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the author
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets the version
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the documentation URL
        /// </summary>
        public string DocumentationUrl { get; set; }
    }
}
