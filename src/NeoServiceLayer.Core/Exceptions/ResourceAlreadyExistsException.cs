using System;

namespace NeoServiceLayer.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when a resource already exists
    /// </summary>
    public class ResourceAlreadyExistsException : Exception
    {
        /// <summary>
        /// Gets the resource type
        /// </summary>
        public string ResourceType { get; }

        /// <summary>
        /// Gets the resource identifier
        /// </summary>
        public string ResourceId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceAlreadyExistsException"/> class
        /// </summary>
        /// <param name="resourceType">Resource type</param>
        /// <param name="resourceId">Resource identifier</param>
        public ResourceAlreadyExistsException(string resourceType, string resourceId)
            : base($"{resourceType} with ID {resourceId} already exists")
        {
            ResourceType = resourceType;
            ResourceId = resourceId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceAlreadyExistsException"/> class
        /// </summary>
        /// <param name="message">Error message</param>
        public ResourceAlreadyExistsException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceAlreadyExistsException"/> class
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="innerException">Inner exception</param>
        public ResourceAlreadyExistsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
