using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents a test for a function
    /// </summary>
    public class FunctionTest
    {
        /// <summary>
        /// Gets or sets the test ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the function ID
        /// </summary>
        public Guid FunctionId { get; set; }

        /// <summary>
        /// Gets or sets the suite ID
        /// </summary>
        public Guid SuiteId { get; set; }

        /// <summary>
        /// Gets or sets the name of the test
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the test
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the type of the test (e.g., "unit", "integration", "performance")
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the test input parameters
        /// </summary>
        public Dictionary<string, object> InputParameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the expected output
        /// </summary>
        public object ExpectedOutput { get; set; }

        /// <summary>
        /// Gets or sets the test assertions
        /// </summary>
        public List<FunctionTestAssertion> Assertions { get; set; } = new List<FunctionTestAssertion>();

        /// <summary>
        /// Gets or sets the test setup code
        /// </summary>
        public string SetupCode { get; set; }

        /// <summary>
        /// Gets or sets the test teardown code
        /// </summary>
        public string TeardownCode { get; set; }

        /// <summary>
        /// Gets or sets the test timeout in milliseconds
        /// </summary>
        public int TimeoutMs { get; set; } = 5000;

        /// <summary>
        /// Gets or sets the test order
        /// </summary>
        public int Order { get; set; } = 0;

        /// <summary>
        /// Gets or sets a value indicating whether the test is enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the test is required to pass for deployment
        /// </summary>
        public bool IsRequired { get; set; } = true;

        /// <summary>
        /// Gets or sets the tags for the test
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
        /// Gets or sets the environment variables for the test
        /// </summary>
        public Dictionary<string, string> EnvironmentVariables { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the mock data for the test
        /// </summary>
        public Dictionary<string, object> MockData { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the test dependencies
        /// </summary>
        public List<Guid> Dependencies { get; set; } = new List<Guid>();
    }
}
