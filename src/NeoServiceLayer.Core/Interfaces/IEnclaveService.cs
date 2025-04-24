using System;
using System.Threading.Tasks;

namespace NeoServiceLayer.Core.Interfaces
{
    /// <summary>
    /// Interface for enclave communication service
    /// </summary>
    public interface IEnclaveService
    {
        /// <summary>
        /// Initializes the enclave
        /// </summary>
        /// <returns>True if the enclave was initialized successfully, false otherwise</returns>
        Task<bool> InitializeAsync();

        /// <summary>
        /// Shuts down the enclave
        /// </summary>
        /// <returns>True if the enclave was shut down successfully, false otherwise</returns>
        Task<bool> ShutdownAsync();

        /// <summary>
        /// Sends a request to the enclave
        /// </summary>
        /// <typeparam name="TRequest">Type of the request</typeparam>
        /// <typeparam name="TResponse">Type of the response</typeparam>
        /// <param name="serviceType">Type of service to handle the request</param>
        /// <param name="operation">Operation to perform</param>
        /// <param name="request">Request data</param>
        /// <returns>Response from the enclave</returns>
        Task<TResponse> SendRequestAsync<TRequest, TResponse>(string serviceType, string operation, TRequest request);

        /// <summary>
        /// Gets the attestation document from the enclave
        /// </summary>
        /// <returns>Attestation document</returns>
        Task<byte[]> GetAttestationDocumentAsync();

        /// <summary>
        /// Verifies an attestation document
        /// </summary>
        /// <param name="attestationDocument">Attestation document to verify</param>
        /// <returns>True if the attestation document is valid, false otherwise</returns>
        Task<bool> VerifyAttestationDocumentAsync(byte[] attestationDocument);

        /// <summary>
        /// Gets the enclave status
        /// </summary>
        /// <returns>Enclave status</returns>
        Task<string> GetStatusAsync();

        /// <summary>
        /// Gets the enclave metrics
        /// </summary>
        /// <returns>Enclave metrics</returns>
        Task<object> GetMetricsAsync();
    }
}
