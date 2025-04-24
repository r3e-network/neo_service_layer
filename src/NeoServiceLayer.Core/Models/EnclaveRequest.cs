using System;

namespace NeoServiceLayer.Core.Models
{
    /// <summary>
    /// Represents a request to the enclave
    /// </summary>
    public class EnclaveRequest
    {
        /// <summary>
        /// Unique identifier for the request
        /// </summary>
        public string RequestId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Type of service to handle the request
        /// </summary>
        public string ServiceType { get; set; }

        /// <summary>
        /// Operation to perform
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// Payload of the request
        /// </summary>
        public byte[] Payload { get; set; }
    }
}
