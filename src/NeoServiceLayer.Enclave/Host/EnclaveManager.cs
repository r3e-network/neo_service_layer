using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core;
using NeoServiceLayer.Core.Exceptions;
using NeoServiceLayer.Core.Models;

namespace NeoServiceLayer.Enclave.Host
{
    /// <summary>
    /// Manager for enclave lifecycle
    /// </summary>
    public class EnclaveManager
    {
        private readonly ILogger<EnclaveManager> _logger;
        private readonly IConfiguration _configuration;
        private readonly VsockClient _vsockClient;
        private Process _enclaveProcess;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnclaveManager"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="configuration">Configuration</param>
        /// <param name="vsockClient">VSOCK client</param>
        public EnclaveManager(ILogger<EnclaveManager> logger, IConfiguration configuration, VsockClient vsockClient)
        {
            _logger = logger;
            _configuration = configuration;
            _vsockClient = vsockClient;
        }

        /// <summary>
        /// Starts the enclave
        /// </summary>
        /// <returns>True if the enclave was started successfully, false otherwise</returns>
        public async Task<bool> StartEnclaveAsync()
        {
            try
            {
                var enclavePath = _configuration["Enclave:Path"];
                var enclaveMemory = _configuration["Enclave:Memory"];
                var enclaveCpus = _configuration["Enclave:Cpus"];

                var startInfo = new ProcessStartInfo
                {
                    FileName = "nitro-cli",
                    Arguments = $"run-enclave --eif-path {enclavePath} --memory {enclaveMemory} --cpu-count {enclaveCpus}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                _enclaveProcess = new Process { StartInfo = startInfo };
                _enclaveProcess.Start();

                var output = await _enclaveProcess.StandardOutput.ReadToEndAsync();
                var error = await _enclaveProcess.StandardError.ReadToEndAsync();

                if (!string.IsNullOrEmpty(error))
                {
                    _logger.LogError("Error starting enclave: {Error}", error);
                    return false;
                }

                _logger.LogInformation("Enclave started: {Output}", output);

                // Wait for enclave to initialize
                await Task.Delay(5000);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting enclave");
                return false;
            }
        }

        /// <summary>
        /// Sends a request to the enclave
        /// </summary>
        /// <typeparam name="TRequest">Type of the request</typeparam>
        /// <typeparam name="TResponse">Type of the response</typeparam>
        /// <param name="serviceType">Type of service to handle the request</param>
        /// <param name="operation">Operation to perform</param>
        /// <param name="request">Request data</param>
        /// <returns>Response from the enclave</returns>
        public async Task<TResponse> SendRequestAsync<TRequest, TResponse>(string serviceType, string operation, TRequest request)
        {
            try
            {
                var enclaveRequest = new EnclaveRequest
                {
                    ServiceType = serviceType,
                    Operation = operation,
                    Payload = JsonSerializer.SerializeToUtf8Bytes(request)
                };

                var requestBytes = JsonSerializer.SerializeToUtf8Bytes(enclaveRequest);
                var responseBytes = await _vsockClient.SendMessageAsync(requestBytes);
                var enclaveResponse = JsonSerializer.Deserialize<EnclaveResponse>(responseBytes);

                if (!enclaveResponse.Success)
                {
                    throw new EnclaveException(enclaveResponse.ErrorMessage);
                }

                return JsonSerializer.Deserialize<TResponse>(enclaveResponse.Payload);
            }
            catch (EnclaveException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending request to enclave");
                throw new EnclaveException("Error sending request to enclave", ex);
            }
        }

        /// <summary>
        /// Gets the attestation document from the enclave
        /// </summary>
        /// <returns>Attestation document</returns>
        public async Task<byte[]> GetAttestationDocumentAsync()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "nitro-cli",
                    Arguments = "describe-enclaves",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = new Process { StartInfo = startInfo };
                process.Start();

                var output = await process.StandardOutput.ReadToEndAsync();
                var enclaveInfo = JsonSerializer.Deserialize<JsonElement[]>(output);
                var enclaveId = enclaveInfo[0].GetProperty("EnclaveID").GetString();

                startInfo = new ProcessStartInfo
                {
                    FileName = "nitro-cli",
                    Arguments = $"get-attestation-document --enclave-id {enclaveId}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                process = new Process { StartInfo = startInfo };
                process.Start();

                output = await process.StandardOutput.ReadToEndAsync();
                var attestationDoc = JsonSerializer.Deserialize<JsonElement>(output);
                var document = attestationDoc.GetProperty("Document").GetString();

                return Convert.FromBase64String(document);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attestation document");
                throw new EnclaveException("Error getting attestation document", ex);
            }
        }

        /// <summary>
        /// Stops the enclave
        /// </summary>
        /// <returns>True if the enclave was stopped successfully, false otherwise</returns>
        public async Task<bool> StopEnclaveAsync()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "nitro-cli",
                    Arguments = "terminate-enclave --enclave-id $(nitro-cli describe-enclaves | jq -r '.[0].EnclaveID')",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = new Process { StartInfo = startInfo };
                process.Start();

                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();

                if (!string.IsNullOrEmpty(error))
                {
                    _logger.LogError("Error stopping enclave: {Error}", error);
                    return false;
                }

                _logger.LogInformation("Enclave stopped: {Output}", output);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping enclave");
                return false;
            }
        }
    }
}
