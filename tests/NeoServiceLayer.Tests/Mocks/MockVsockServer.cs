using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Enclave.Enclave;

namespace NeoServiceLayer.Tests.Mocks
{
    public class MockVsockServer : VsockServer
    {
        public MockVsockServer(ILogger<VsockServer> logger, IServiceProvider serviceProvider)
            : base(logger, serviceProvider)
        {
        }

        public async Task<byte[]> ProcessMessageAsync(byte[] message)
        {
            try
            {
                // Try to deserialize as EnclaveRequest
                try
                {
                    var request = JsonSerializer.Deserialize<EnclaveRequest>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    if (request != null)
                    {
                        if (request.ServiceType == "ping")
                        {
                            var response = new EnclaveResponse
                            {
                                RequestId = request.RequestId,
                                Success = true,
                                Payload = JsonSerializer.SerializeToUtf8Bytes(new { Status = "OK" })
                            };
                            return JsonSerializer.SerializeToUtf8Bytes(response);
                        }
                        else if (request.ServiceType == "metrics")
                        {
                            var metrics = new
                            {
                                CpuUsage = 10.5,
                                MemoryUsage = 1024 * 1024 * 100,
                                Uptime = TimeSpan.FromMinutes(30),
                                RequestCount = 42,
                                Timestamp = DateTime.UtcNow
                            };
                            
                            var response = new EnclaveResponse
                            {
                                RequestId = request.RequestId,
                                Success = true,
                                Payload = JsonSerializer.SerializeToUtf8Bytes(metrics)
                            };
                            return JsonSerializer.SerializeToUtf8Bytes(response);
                        }
                    }
                }
                catch
                {
                    // Invalid JSON, fall through to error response
                }
                
                // Default error response
                var errorResponse = new EnclaveResponse
                {
                    RequestId = "unknown",
                    Success = false,
                    ErrorMessage = "Error processing message: Invalid request format"
                };
                return JsonSerializer.SerializeToUtf8Bytes(errorResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new EnclaveResponse
                {
                    RequestId = "unknown",
                    Success = false,
                    ErrorMessage = $"Error processing message: {ex.Message}"
                };
                return JsonSerializer.SerializeToUtf8Bytes(errorResponse);
            }
        }
    }
}
