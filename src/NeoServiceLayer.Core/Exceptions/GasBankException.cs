using System;

namespace NeoServiceLayer.Core.Exceptions
{
    /// <summary>
    /// Exception thrown by the GasBank service
    /// </summary>
    public class GasBankException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GasBankException"/> class
        /// </summary>
        public GasBankException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GasBankException"/> class with a specified error message
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        public GasBankException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GasBankException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public GasBankException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
