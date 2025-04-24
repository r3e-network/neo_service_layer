using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents a step in a function composition
    /// </summary>
    public class FunctionCompositionStep
    {
        /// <summary>
        /// Gets or sets the step ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the step
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the step
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the function ID
        /// </summary>
        public Guid FunctionId { get; set; }

        /// <summary>
        /// Gets or sets the function version
        /// </summary>
        public string FunctionVersion { get; set; }

        /// <summary>
        /// Gets or sets the order of the step
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the input mappings
        /// </summary>
        public Dictionary<string, string> InputMappings { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the output mappings
        /// </summary>
        public Dictionary<string, string> OutputMappings { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the condition for executing the step
        /// </summary>
        public string Condition { get; set; }

        /// <summary>
        /// Gets or sets the retry policy
        /// </summary>
        public FunctionCompositionRetryPolicy RetryPolicy { get; set; }

        /// <summary>
        /// Gets or sets the timeout in milliseconds
        /// </summary>
        public int TimeoutMs { get; set; } = 5000;

        /// <summary>
        /// Gets or sets a value indicating whether the step is enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the step is required
        /// </summary>
        public bool IsRequired { get; set; } = true;

        /// <summary>
        /// Gets or sets the error handling strategy
        /// </summary>
        public string ErrorHandlingStrategy { get; set; } = "stop";

        /// <summary>
        /// Gets or sets the dependencies
        /// </summary>
        public List<Guid> Dependencies { get; set; } = new List<Guid>();

        /// <summary>
        /// Gets or sets the metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
}
