namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents an assertion result for a function test
    /// </summary>
    public class FunctionTestAssertionResult
    {
        /// <summary>
        /// Gets or sets the type of the assertion
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the path to the property that was asserted on
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the expected value
        /// </summary>
        public object ExpectedValue { get; set; }

        /// <summary>
        /// Gets or sets the actual value
        /// </summary>
        public object ActualValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the assertion passed
        /// </summary>
        public bool Passed { get; set; }

        /// <summary>
        /// Gets or sets the message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the assertion was required to pass
        /// </summary>
        public bool IsRequired { get; set; }
    }
}
