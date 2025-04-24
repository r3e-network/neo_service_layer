using System;

namespace NeoServiceLayer.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when a price feed operation fails
    /// </summary>
    public class PriceFeedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceFeedException"/> class
        /// </summary>
        public PriceFeedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceFeedException"/> class with a specified error message
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        public PriceFeedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceFeedException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public PriceFeedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
