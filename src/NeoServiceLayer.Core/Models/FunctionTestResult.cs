using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents a test result for a function
    /// </summary>
    public class FunctionTestResult
    {
        /// <summary>
        /// Gets or sets the result ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the test ID
        /// </summary>
        public Guid TestId { get; set; }

        /// <summary>
        /// Gets or sets the suite ID
        /// </summary>
        public Guid SuiteId { get; set; }

        /// <summary>
        /// Gets or sets the function ID
        /// </summary>
        public Guid FunctionId { get; set; }

        /// <summary>
        /// Gets or sets the function version
        /// </summary>
        public string FunctionVersion { get; set; }

        /// <summary>
        /// Gets or sets the status of the test (e.g., "passed", "failed", "error", "skipped")
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the error message if the test failed
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the stack trace if the test failed
        /// </summary>
        public string StackTrace { get; set; }

        /// <summary>
        /// Gets or sets the actual output
        /// </summary>
        public object ActualOutput { get; set; }

        /// <summary>
        /// Gets or sets the assertion results
        /// </summary>
        public List<FunctionTestAssertionResult> AssertionResults { get; set; } = new List<FunctionTestAssertionResult>();

        /// <summary>
        /// Gets or sets the execution time in milliseconds
        /// </summary>
        public double ExecutionTimeMs { get; set; }

        /// <summary>
        /// Gets or sets the memory usage in megabytes
        /// </summary>
        public double MemoryUsageMb { get; set; }

        /// <summary>
        /// Gets or sets the logs
        /// </summary>
        public List<string> Logs { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the start time
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Gets or sets the run by user ID
        /// </summary>
        public Guid RunBy { get; set; }

        /// <summary>
        /// Gets or sets the environment ID
        /// </summary>
        public Guid? EnvironmentId { get; set; }

        /// <summary>
        /// Gets or sets the deployment ID
        /// </summary>
        public Guid? DeploymentId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the test was run as part of a deployment
        /// </summary>
        public bool IsDeploymentTest { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the test was run as part of a CI/CD pipeline
        /// </summary>
        public bool IsCiCdTest { get; set; } = false;

        /// <summary>
        /// Gets or sets the CI/CD pipeline ID
        /// </summary>
        public string CiCdPipelineId { get; set; }

        /// <summary>
        /// Gets or sets the CI/CD job ID
        /// </summary>
        public string CiCdJobId { get; set; }
    }
}
