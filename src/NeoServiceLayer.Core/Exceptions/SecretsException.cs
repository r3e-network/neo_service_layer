using System;

namespace NeoServiceLayer.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when a secrets operation fails
    /// </summary>
    public class SecretsException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecretsException"/> class
        /// </summary>
        public SecretsException() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecretsException"/> class with a specified error message
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        public SecretsException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecretsException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public SecretsException(string message, Exception innerException) : base(message, innerException) { }
    }
}
