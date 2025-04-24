using System;

namespace NeoServiceLayer.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when access is forbidden
    /// </summary>
    public class ForbiddenAccessException : Exception
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
        /// Gets the principal identifier
        /// </summary>
        public string PrincipalId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForbiddenAccessException"/> class
        /// </summary>
        /// <param name="resourceType">Resource type</param>
        /// <param name="resourceId">Resource identifier</param>
        /// <param name="principalId">Principal identifier</param>
        public ForbiddenAccessException(string resourceType, string resourceId, string principalId)
            : base($"Access to {resourceType} with ID {resourceId} is forbidden for principal {principalId}")
        {
            ResourceType = resourceType;
            ResourceId = resourceId;
            PrincipalId = principalId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForbiddenAccessException"/> class
        /// </summary>
        /// <param name="message">Error message</param>
        public ForbiddenAccessException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForbiddenAccessException"/> class
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="innerException">Inner exception</param>
        public ForbiddenAccessException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
