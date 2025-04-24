namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents an assertion for a function test
    /// </summary>
    public class FunctionTestAssertion
    {
        /// <summary>
        /// Gets or sets the type of the assertion (e.g., "equals", "contains", "greaterThan", "lessThan", "regex")
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the path to the property to assert on (e.g., "result.data.value")
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the expected value
        /// </summary>
        public object ExpectedValue { get; set; }

        /// <summary>
        /// Gets or sets the message to display if the assertion fails
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the assertion is case-sensitive
        /// </summary>
        public bool CaseSensitive { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the assertion is required to pass
        /// </summary>
        public bool IsRequired { get; set; } = true;

        /// <summary>
        /// Gets or sets the tolerance for numeric comparisons
        /// </summary>
        public double Tolerance { get; set; } = 0;
    }
}
