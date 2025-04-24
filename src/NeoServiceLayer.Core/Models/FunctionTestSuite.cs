using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents a test suite for a function
    /// </summary>
    public class FunctionTestSuite
    {
        /// <summary>
        /// Gets or sets the suite ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the function ID
        /// </summary>
        public Guid FunctionId { get; set; }

        /// <summary>
        /// Gets or sets the name of the suite
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the suite
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the test IDs in the suite
        /// </summary>
        public List<Guid> TestIds { get; set; } = new List<Guid>();

        /// <summary>
        /// Gets or sets a value indicating whether the suite is enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the suite is required to pass for deployment
        /// </summary>
        public bool IsRequired { get; set; } = true;

        /// <summary>
        /// Gets or sets the tags for the suite
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

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
        /// Gets or sets the environment variables for the suite
        /// </summary>
        public Dictionary<string, string> EnvironmentVariables { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the setup code for the suite
        /// </summary>
        public string SetupCode { get; set; }

        /// <summary>
        /// Gets or sets the teardown code for the suite
        /// </summary>
        public string TeardownCode { get; set; }

        /// <summary>
        /// Gets or sets the timeout for the suite in milliseconds
        /// </summary>
        public int TimeoutMs { get; set; } = 30000;

        /// <summary>
        /// Gets or sets the parallel execution count
        /// </summary>
        public int ParallelCount { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value indicating whether to stop on first failure
        /// </summary>
        public bool StopOnFirstFailure { get; set; } = false;
    }
}
