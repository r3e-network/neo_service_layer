using System;
using System.Collections.Generic;

namespace NeoServiceLayer.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when validation fails
    /// </summary>
    public class ValidationException : Exception
    {
        /// <summary>
        /// Gets the validation errors
        /// </summary>
        public Dictionary<string, List<string>> Errors { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class
        /// </summary>
        /// <param name="errors">Validation errors</param>
        public ValidationException(Dictionary<string, List<string>> errors)
            : base("Validation failed")
        {
            Errors = errors;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="errors">Validation errors</param>
        public ValidationException(string message, Dictionary<string, List<string>> errors)
            : base(message)
        {
            Errors = errors;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class
        /// </summary>
        /// <param name="message">Error message</param>
        public ValidationException(string message)
            : base(message)
        {
            Errors = new Dictionary<string, List<string>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="innerException">Inner exception</param>
        public ValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
            Errors = new Dictionary<string, List<string>>();
        }
    }
}
