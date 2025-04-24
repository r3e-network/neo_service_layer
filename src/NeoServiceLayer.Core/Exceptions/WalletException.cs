using System;

namespace NeoServiceLayer.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when a wallet operation fails
    /// </summary>
    public class WalletException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WalletException"/> class
        /// </summary>
        public WalletException() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="WalletException"/> class with a specified error message
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        public WalletException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="WalletException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public WalletException(string message, Exception innerException) : base(message, innerException) { }
    }
}
