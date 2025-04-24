using System.Threading.Tasks;
using NeoServiceLayer.Enclave.Enclave.Models;

namespace NeoServiceLayer.Enclave.Enclave.Interfaces
{
    /// <summary>
    /// Interface for enclave services
    /// </summary>
    public interface IEnclaveService
    {
        /// <summary>
        /// Initializes the service
        /// </summary>
        /// <returns>True if initialization was successful, false otherwise</returns>
        Task<bool> InitializeAsync();

        /// <summary>
        /// Processes a request
        /// </summary>
        /// <param name="request">Enclave request</param>
        /// <returns>Enclave response</returns>
        Task<EnclaveResponse> ProcessRequestAsync(EnclaveRequest request);

        /// <summary>
        /// Shuts down the service
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        Task ShutdownAsync();
    }
}
