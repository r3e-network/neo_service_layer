using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Api.Models.Responses
{
    /// <summary>
    /// Response model for a function template
    /// </summary>
    public class FunctionTemplateResponse
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
        public List<string> Tags { get; set; }

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
