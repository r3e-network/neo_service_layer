using System;

namespace NeoServiceLayer.Enclave.Enclave.Models
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
        /// Operation to perform
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// Payload of the request
        /// </summary>
        public byte[] Payload { get; set; }
    }
}
