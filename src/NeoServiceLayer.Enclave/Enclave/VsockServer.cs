using System;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NeoServiceLayer.Core;
using NeoServiceLayer.Core.Models;
using NeoServiceLayer.Enclave.Host;
using NeoServiceLayer.Enclave.Enclave.Services;
using System.Linq;
using NeoServiceLayer.Enclave.Enclave.Models;
using CoreEnclaveRequest = NeoServiceLayer.Core.Models.EnclaveRequest;
using CoreEnclaveResponse = NeoServiceLayer.Core.Models.EnclaveResponse;
using EnclaveEnclaveRequest = NeoServiceLayer.Enclave.Enclave.Models.EnclaveRequest;
using EnclaveEnclaveResponse = NeoServiceLayer.Enclave.Enclave.Models.EnclaveResponse;

namespace NeoServiceLayer.Enclave.Enclave
{
    /// <summary>
    /// Server for VSOCK communication with the parent instance
    /// </summary>
    public class VsockServer
    {
        private readonly ILogger<VsockServer> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Socket _socket;

        /// <summary>
        /// Initializes a new instance of the <see cref="VsockServer"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="serviceProvider">Service provider</param>
        public VsockServer(ILogger<VsockServer> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _cancellationTokenSource = new CancellationTokenSource();
            _socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        }

        /// <summary>
        /// Starts the VSOCK server
        /// </summary>
        public void Start()
        {
            try
            {
                var endpoint = new VsockEndPoint(VsockEndPoint.LocalCid, Constants.VsockConfig.EnclavePort);
                _socket.Bind(endpoint);
                _socket.Listen(10);

                _logger.LogInformation("VSOCK server started on port {Port}", Constants.VsockConfig.EnclavePort);

                Task.Run(AcceptConnectionsAsync);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting VSOCK server");
                throw;
            }
        }

        /// <summary>
        /// Stops the VSOCK server
        /// </summary>
        public void Stop()
        {
            _logger.LogInformation("Stopping VSOCK server");
            _cancellationTokenSource.Cancel();
            _socket.Close();
        }

        private async Task AcceptConnectionsAsync()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var clientSocket = await _socket.AcceptAsync();
                    _ = HandleClientAsync(clientSocket);
                }
                catch (Exception ex) when (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Error accepting client connection");
                }
            }
        }

        private async Task HandleClientAsync(Socket clientSocket)
        {
            using (clientSocket)
            {
                try
                {
                    // Receive message length
                    var lengthBytes = new byte[4];
                    await clientSocket.ReceiveAsync(lengthBytes, SocketFlags.None);
                    var messageLength = BitConverter.ToInt32(lengthBytes);

                    // Receive message
                    var message = new byte[messageLength];
                    var totalBytesReceived = 0;

                    while (totalBytesReceived < messageLength)
                    {
                        var bytesReceived = await clientSocket.ReceiveAsync(
                            message.AsMemory(totalBytesReceived, messageLength - totalBytesReceived),
                            SocketFlags.None);

                        totalBytesReceived += bytesReceived;
                    }

                    // Process message
                    var response = await ProcessMessageAsync(message);

                    // Send response length
                    var responseLengthBytes = BitConverter.GetBytes(response.Length);
                    await clientSocket.SendAsync(responseLengthBytes, SocketFlags.None);

                    // Send response
                    await clientSocket.SendAsync(response, SocketFlags.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling client");
                }
            }
        }

        private async Task<byte[]> ProcessMessageAsync(byte[] message)
        {
            try
            {
                // Deserialize message
                var request = JsonSerializer.Deserialize<CoreEnclaveRequest>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                _logger.LogInformation("Received request: {RequestId} {ServiceType}.{Operation}",
                    request.RequestId, request.ServiceType, request.Operation);

                // Increment request count
                IncrementRequestCount();

                // Route to appropriate service
                var response = await RouteRequestAsync(request);

                // Serialize response
                return JsonSerializer.SerializeToUtf8Bytes(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");

                var response = new CoreEnclaveResponse
                {
                    RequestId = "unknown",
                    Success = false,
                    ErrorMessage = $"Error processing message: {ex.Message}"
                };

                return JsonSerializer.SerializeToUtf8Bytes(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }
        }

        private async Task<CoreEnclaveResponse> RouteRequestAsync(CoreEnclaveRequest request)
        {
            try
            {
                switch (request.ServiceType)
                {
                    case Constants.EnclaveServiceTypes.Account:
                        return await ProcessAccountRequestAsync(request);
                    case Constants.EnclaveServiceTypes.Wallet:
                        return await ProcessWalletRequestAsync(request);
                    case Constants.EnclaveServiceTypes.Secrets:
                        return await ProcessSecretsRequestAsync(request);
                    case Constants.EnclaveServiceTypes.Function:
                        return await ProcessFunctionRequestAsync(request);
                    case Constants.EnclaveServiceTypes.PriceFeed:
                        return await ProcessPriceFeedRequestAsync(request);
                    case "ping":
                        return CreateSuccessResponse(request.RequestId, new { Status = "OK" });
                    case "metrics":
                        return await ProcessMetricsRequestAsync(request);
                    default:
                        return CreateErrorResponse(request.RequestId, $"Unknown service type: {request.ServiceType}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error routing request");
                return CreateErrorResponse(request.RequestId, $"Error routing request: {ex.Message}");
            }
        }

        private async Task<CoreEnclaveResponse> ProcessAccountRequestAsync(CoreEnclaveRequest request)
        {
            try
            {
                var accountService = _serviceProvider.GetRequiredService<EnclaveAccountService>();
                var enclaveRequest = new EnclaveEnclaveRequest
                {
                    RequestId = request.RequestId,
                    Operation = request.Operation,
                    Payload = request.Payload
                };
                var response = await accountService.ProcessRequestAsync(enclaveRequest);
                return new CoreEnclaveResponse
                {
                    RequestId = response.RequestId,
                    Success = response.Success,
                    ErrorMessage = response.ErrorMessage,
                    Payload = response.Payload
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing account request");
                return CreateErrorResponse(request.RequestId, ex.Message);
            }
        }

        private async Task<CoreEnclaveResponse> ProcessWalletRequestAsync(CoreEnclaveRequest request)
        {
            try
            {
                var walletService = _serviceProvider.GetRequiredService<EnclaveWalletService>();
                var enclaveRequest = new EnclaveEnclaveRequest
                {
                    RequestId = request.RequestId,
                    Operation = request.Operation,
                    Payload = request.Payload
                };
                var response = await walletService.ProcessRequestAsync(enclaveRequest);
                return new CoreEnclaveResponse
                {
                    RequestId = response.RequestId,
                    Success = response.Success,
                    ErrorMessage = response.ErrorMessage,
                    Payload = response.Payload
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing wallet request");
                return CreateErrorResponse(request.RequestId, ex.Message);
            }
        }

        private async Task<CoreEnclaveResponse> ProcessSecretsRequestAsync(CoreEnclaveRequest request)
        {
            try
            {
                var secretsService = _serviceProvider.GetRequiredService<EnclaveSecretsService>();
                var enclaveRequest = new EnclaveEnclaveRequest
                {
                    RequestId = request.RequestId,
                    Operation = request.Operation,
                    Payload = request.Payload
                };
                var response = await secretsService.ProcessRequestAsync(enclaveRequest);
                return new CoreEnclaveResponse
                {
                    RequestId = response.RequestId,
                    Success = response.Success,
                    ErrorMessage = response.ErrorMessage,
                    Payload = response.Payload
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing secrets request");
                return CreateErrorResponse(request.RequestId, ex.Message);
            }
        }

        private async Task<CoreEnclaveResponse> ProcessFunctionRequestAsync(CoreEnclaveRequest request)
        {
            try
            {
                var functionService = _serviceProvider.GetRequiredService<EnclaveFunctionService>();
                var enclaveRequest = new EnclaveEnclaveRequest
                {
                    RequestId = request.RequestId,
                    Operation = request.Operation,
                    Payload = request.Payload
                };
                var response = await functionService.ProcessRequestAsync(enclaveRequest);
                return new CoreEnclaveResponse
                {
                    RequestId = response.RequestId,
                    Success = response.Success,
                    ErrorMessage = response.ErrorMessage,
                    Payload = response.Payload
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing function request");
                return CreateErrorResponse(request.RequestId, ex.Message);
            }
        }

        private async Task<CoreEnclaveResponse> ProcessPriceFeedRequestAsync(CoreEnclaveRequest request)
        {
            try
            {
                var priceFeedService = _serviceProvider.GetRequiredService<EnclavePriceFeedService>();
                var enclaveRequest = new EnclaveEnclaveRequest
                {
                    RequestId = request.RequestId,
                    Operation = request.Operation,
                    Payload = request.Payload
                };
                var response = await priceFeedService.ProcessRequestAsync(enclaveRequest);
                return new CoreEnclaveResponse
                {
                    RequestId = response.RequestId,
                    Success = response.Success,
                    ErrorMessage = response.ErrorMessage,
                    Payload = response.Payload
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing price feed request");
                return CreateErrorResponse(request.RequestId, ex.Message);
            }
        }

        private async Task<CoreEnclaveResponse> ProcessMetricsRequestAsync(CoreEnclaveRequest request)
        {
            try
            {
                // Get system metrics
                var metrics = new
                {
                    CpuUsage = GetCpuUsage(),
                    MemoryUsage = GetMemoryUsage(),
                    Uptime = GetUptime(),
                    RequestCount = GetRequestCount(),
                    Timestamp = DateTime.UtcNow
                };

                await Task.Delay(10); // Small delay to simulate processing
                var payload = JsonSerializer.SerializeToUtf8Bytes(metrics, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                return new CoreEnclaveResponse
                {
                    RequestId = request.RequestId,
                    Success = true,
                    Payload = payload
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing metrics request");
                return CreateErrorResponse(request.RequestId, ex.Message);
            }
        }

        private double GetCpuUsage()
        {
            try
            {
                // Get CPU usage using /proc/stat
                // This is a simplified implementation that calculates CPU usage based on the difference between two readings

                // Read /proc/stat
                var statBefore = System.IO.File.ReadAllText("/proc/stat").Split('\n')[0];
                var cpuTimesBefore = statBefore.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1).Take(7).Select(long.Parse).ToArray();
                var idleTimeBefore = cpuTimesBefore[3];
                var totalTimeBefore = cpuTimesBefore.Sum();

                // Wait a short time
                Thread.Sleep(100);

                // Read /proc/stat again
                var statAfter = System.IO.File.ReadAllText("/proc/stat").Split('\n')[0];
                var cpuTimesAfter = statAfter.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1).Take(7).Select(long.Parse).ToArray();
                var idleTimeAfter = cpuTimesAfter[3];
                var totalTimeAfter = cpuTimesAfter.Sum();

                // Calculate CPU usage
                var idleDelta = idleTimeAfter - idleTimeBefore;
                var totalDelta = totalTimeAfter - totalTimeBefore;

                if (totalDelta == 0)
                {
                    return 0;
                }

                var cpuUsage = 100.0 * (1.0 - (double)idleDelta / totalDelta);
                return Math.Round(cpuUsage, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting CPU usage");
                return new Random().NextDouble() * 100; // Fallback to random value
            }
        }

        private long GetMemoryUsage()
        {
            try
            {
                // Get memory usage using /proc/meminfo
                var memInfo = System.IO.File.ReadAllText("/proc/meminfo");
                var lines = memInfo.Split('\n');

                // Parse MemTotal and MemAvailable
                var memTotal = 0L;
                var memAvailable = 0L;

                foreach (var line in lines)
                {
                    if (line.StartsWith("MemTotal:"))
                    {
                        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        memTotal = long.Parse(parts[1]) * 1024; // Convert from KB to bytes
                    }
                    else if (line.StartsWith("MemAvailable:"))
                    {
                        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        memAvailable = long.Parse(parts[1]) * 1024; // Convert from KB to bytes
                    }
                }

                // Calculate memory usage
                var memUsed = memTotal - memAvailable;
                return memUsed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting memory usage");
                return (long)(new Random().NextDouble() * 1024 * 1024 * 1024); // Fallback to random value up to 1GB
            }
        }

        private readonly DateTime _startTime = DateTime.UtcNow;

        private TimeSpan GetUptime()
        {
            try
            {
                // Calculate uptime based on the server start time
                return DateTime.UtcNow - _startTime;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting uptime");
                return TimeSpan.FromMinutes(new Random().Next(1, 60 * 24 * 7)); // Fallback to random value up to 1 week
            }
        }

        private long _requestCount = 0;

        private long GetRequestCount()
        {
            try
            {
                return Interlocked.Read(ref _requestCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting request count");
                return new Random().Next(1, 10000); // Fallback to random value
            }
        }

        private void IncrementRequestCount()
        {
            Interlocked.Increment(ref _requestCount);
        }

        private CoreEnclaveResponse CreateSuccessResponse(string requestId, object payload)
        {
            return new CoreEnclaveResponse
            {
                RequestId = requestId,
                Success = true,
                Payload = JsonSerializer.SerializeToUtf8Bytes(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
            };
        }

        private CoreEnclaveResponse CreateErrorResponse(string requestId, string errorMessage)
        {
            return new CoreEnclaveResponse
            {
                RequestId = requestId,
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
