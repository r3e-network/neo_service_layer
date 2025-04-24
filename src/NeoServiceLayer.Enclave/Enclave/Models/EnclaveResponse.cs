namespace NeoServiceLayer.Enclave.Enclave.Models
{
    /// <summary>
    /// Represents a response from the enclave
    /// </summary>
    public class EnclaveResponse
    {
        /// <summary>
        /// Unique identifier for the request
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// Indicates whether the request was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error message if the request failed
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Payload of the response
        /// </summary>
        public byte[] Payload { get; set; }
    }
}
