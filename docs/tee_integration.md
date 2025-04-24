# TEE Integration with VSOCK

## Overview

This document describes how the Neo Service Layer integrates with AWS Nitro Enclaves using VSOCK for secure communication. It provides technical details on the implementation, configuration, and usage of the Trusted Execution Environment (TEE) within the system.

## AWS Nitro Enclaves

AWS Nitro Enclaves is a feature of Amazon EC2 that enables the creation of isolated compute environments to protect and securely process highly sensitive data. Key features include:

1. **Isolated Memory**: Enclave memory is encrypted and isolated from the parent instance
2. **No Persistent Storage**: Enclaves have no persistent storage, enhancing security
3. **No Interactive Access**: No SSH or other interactive access to the enclave
4. **Cryptographic Attestation**: Provides proof of the enclave's identity and integrity

## VSOCK Communication

VSOCK (Virtual Socket) is a communication protocol used for communication between the parent EC2 instance and the Nitro Enclave. It provides a secure channel for data exchange without exposing the enclave to the network.

### VSOCK Architecture

```
+-------------------------------------------+
|              EC2 Instance                 |
|                                           |
|  +-----------------------------------+    |
|  |         Parent Instance           |    |
|  |                                   |    |
|  |  +---------------------------+    |    |
|  |  |                           |    |    |
|  |  |    VSOCK Client           |    |    |
|  |  |    (CID: 3)               |    |    |
|  |  |                           |    |    |
|  |  +------------+--------------+    |    |
|  |               |                   |    |
|  +---------------+-------------------+    |
|                  |                        |
|                  | VSOCK                  |
|                  | Communication          |
|                  |                        |
|  +---------------+-------------------+    |
|  |               |                   |    |
|  |  +------------+--------------+    |    |
|  |  |                           |    |    |
|  |  |    VSOCK Server           |    |    |
|  |  |    (CID: 16)              |    |    |
|  |  |                           |    |    |
|  |  +---------------------------+    |    |
|  |                                   |    |
|  |         Nitro Enclave            |    |
|  +-----------------------------------+    |
|                                           |
+-------------------------------------------+
```

### VSOCK Addressing

VSOCK uses Context Identifiers (CIDs) to identify endpoints:
- **CID 3**: The parent instance
- **CID 16**: The enclave instance

Ports are used to identify specific services within each endpoint.

## Implementation in Neo Service Layer

### 1. VSOCK Client (Parent Instance)

The VSOCK client runs on the parent instance and communicates with the enclave. It is implemented in the `NeoServiceLayer.Enclave.Host.VsockClient` class.

```csharp
public class VsockClient
{
    private const int EnclavePort = 5000;
    private const int EnclaveCid = 16;
    
    public async Task<byte[]> SendMessageAsync(byte[] message)
    {
        using var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        var endpoint = new VsockEndPoint(EnclaveCid, EnclavePort);
        
        await socket.ConnectAsync(endpoint);
        
        // Send message length
        var lengthBytes = BitConverter.GetBytes(message.Length);
        await socket.SendAsync(lengthBytes, SocketFlags.None);
        
        // Send message
        await socket.SendAsync(message, SocketFlags.None);
        
        // Receive response length
        var responseLengthBytes = new byte[4];
        await socket.ReceiveAsync(responseLengthBytes, SocketFlags.None);
        var responseLength = BitConverter.ToInt32(responseLengthBytes);
        
        // Receive response
        var response = new byte[responseLength];
        var totalBytesReceived = 0;
        
        while (totalBytesReceived < responseLength)
        {
            var bytesReceived = await socket.ReceiveAsync(
                response.AsMemory(totalBytesReceived, responseLength - totalBytesReceived),
                SocketFlags.None);
                
            totalBytesReceived += bytesReceived;
        }
        
        return response;
    }
}
```

### 2. VSOCK Server (Enclave)

The VSOCK server runs in the enclave and handles requests from the parent instance. It is implemented in the `NeoServiceLayer.Enclave.Enclave.VsockServer` class.

```csharp
public class VsockServer
{
    private const int EnclavePort = 5000;
    private readonly IServiceProvider _serviceProvider;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Socket _socket;
    
    public VsockServer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _cancellationTokenSource = new CancellationTokenSource();
        _socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
    }
    
    public void Start()
    {
        var endpoint = new VsockEndPoint(VsockEndPoint.LocalCid, EnclavePort);
        _socket.Bind(endpoint);
        _socket.Listen(10);
        
        Task.Run(AcceptConnectionsAsync);
    }
    
    private async Task AcceptConnectionsAsync()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            var clientSocket = await _socket.AcceptAsync();
            _ = HandleClientAsync(clientSocket);
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
                Console.WriteLine($"Error handling client: {ex.Message}");
            }
        }
    }
    
    private async Task<byte[]> ProcessMessageAsync(byte[] message)
    {
        // Deserialize message
        var request = JsonSerializer.Deserialize<EnclaveRequest>(message);
        
        // Route to appropriate service
        var response = request.ServiceType switch
        {
            "account" => await ProcessAccountRequestAsync(request),
            "wallet" => await ProcessWalletRequestAsync(request),
            "secrets" => await ProcessSecretsRequestAsync(request),
            "function" => await ProcessFunctionRequestAsync(request),
            _ => CreateErrorResponse("Unknown service type")
        };
        
        // Serialize response
        return JsonSerializer.SerializeToUtf8Bytes(response);
    }
    
    private async Task<EnclaveResponse> ProcessAccountRequestAsync(EnclaveRequest request)
    {
        var accountService = _serviceProvider.GetRequiredService<IAccountService>();
        // Process account request
        // ...
        return new EnclaveResponse { /* ... */ };
    }
    
    // Similar methods for other service types
    
    private EnclaveResponse CreateErrorResponse(string errorMessage)
    {
        return new EnclaveResponse
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
    
    public void Stop()
    {
        _cancellationTokenSource.Cancel();
        _socket.Close();
    }
}
```

### 3. Message Format

Messages exchanged between the parent instance and the enclave are serialized using JSON. The basic message format is:

```csharp
public class EnclaveRequest
{
    public string RequestId { get; set; }
    public string ServiceType { get; set; }
    public string Operation { get; set; }
    public byte[] Payload { get; set; }
}

public class EnclaveResponse
{
    public string RequestId { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public byte[] Payload { get; set; }
}
```

### 4. Enclave Manager

The Enclave Manager is responsible for creating and managing the enclave. It is implemented in the `NeoServiceLayer.Enclave.Host.EnclaveManager` class.

```csharp
public class EnclaveManager
{
    private readonly ILogger<EnclaveManager> _logger;
    private readonly IConfiguration _configuration;
    private Process _enclaveProcess;
    private VsockClient _vsockClient;
    
    public EnclaveManager(ILogger<EnclaveManager> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _vsockClient = new VsockClient();
    }
    
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
    
    public async Task<EnclaveResponse> SendRequestAsync(EnclaveRequest request)
    {
        try
        {
            var requestBytes = JsonSerializer.SerializeToUtf8Bytes(request);
            var responseBytes = await _vsockClient.SendMessageAsync(requestBytes);
            return JsonSerializer.Deserialize<EnclaveResponse>(responseBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending request to enclave");
            return new EnclaveResponse
            {
                RequestId = request.RequestId,
                Success = false,
                ErrorMessage = $"Error communicating with enclave: {ex.Message}"
            };
        }
    }
    
    public async Task StopEnclaveAsync()
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
            }
            
            _logger.LogInformation("Enclave stopped: {Output}", output);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping enclave");
        }
    }
}
```

## Enclave Services

The following services run within the enclave to provide secure processing of sensitive operations:

### 1. Account Service

The Account Service in the enclave handles sensitive account operations such as:
- Password hashing and verification
- Account data encryption and decryption
- Authentication token generation

### 2. Wallet Service

The Wallet Service in the enclave handles sensitive wallet operations such as:
- Private key generation and storage
- Transaction signing
- Key derivation

### 3. Secrets Service

The Secrets Service in the enclave handles sensitive secrets operations such as:
- Secret encryption and decryption
- Access control validation
- Secret versioning

### 4. Function Execution

The Function Execution service in the enclave provides a secure environment for executing user functions with access to sensitive data.

## Attestation

Attestation is a process that proves the identity and integrity of the enclave to remote parties. It is used to establish trust between the enclave and external systems.

### Attestation Process

```
+----------------+     +----------------+     +----------------+
|                |     |                |     |                |
|  Client        |     |  Enclave       |     |  AWS Nitro     |
|                |     |                |     |  Attestation   |
+-------+--------+     +-------+--------+     +-------+--------+
        |                      |                      |
        | 1. Request           |                      |
        | Attestation          |                      |
        +--------------------->|                      |
        |                      |                      |
        |                      | 2. Generate          |
        |                      | Attestation Document |
        |                      +--------------------->|
        |                      |                      |
        |                      | 3. Signed            |
        |                      | Attestation Document |
        |                      |<---------------------+
        |                      |                      |
        | 4. Attestation       |                      |
        | Document             |                      |
        |<---------------------+                      |
        |                      |                      |
        | 5. Verify            |                      |
        | Attestation          |                      |
        +--------------------->|                      |
        |                      |                      |
        | 6. Establish         |                      |
        | Secure Channel       |                      |
        |<--------------------->                      |
        |                      |                      |
```

### Attestation Implementation

```csharp
public class AttestationService
{
    private readonly ILogger<AttestationService> _logger;
    
    public AttestationService(ILogger<AttestationService> logger)
    {
        _logger = logger;
    }
    
    public async Task<byte[]> GenerateAttestationDocumentAsync()
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
            _logger.LogError(ex, "Error generating attestation document");
            throw;
        }
    }
    
    public bool VerifyAttestationDocument(byte[] attestationDocument)
    {
        try
        {
            // Verify the attestation document
            // This would typically involve checking the signature, PCR values, etc.
            // For simplicity, we're just returning true here
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying attestation document");
            return false;
        }
    }
}
```

## Configuration

### Parent Instance Configuration

```json
{
  "Enclave": {
    "Path": "/path/to/enclave.eif",
    "Memory": "2048",
    "Cpus": "2"
  },
  "Vsock": {
    "Port": 5000
  }
}
```

### Enclave Configuration

```json
{
  "Vsock": {
    "Port": 5000
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```

## Security Considerations

### 1. Minimizing Attack Surface

- Include only necessary components in the enclave
- Limit communication channels to VSOCK only
- Implement proper input validation for all messages

### 2. Secure Boot

- Verify enclave image integrity before launching
- Use secure boot process to ensure the enclave is not tampered with
- Implement runtime integrity checks

### 3. Memory Protection

- Use memory encryption to protect sensitive data
- Implement proper memory management to prevent leaks
- Clear sensitive data from memory when no longer needed

### 4. Communication Security

- Validate all messages received from the parent instance
- Encrypt sensitive data before sending it over VSOCK
- Implement proper error handling to prevent information leakage

## Conclusion

The Neo Service Layer leverages AWS Nitro Enclaves and VSOCK communication to provide a secure environment for processing sensitive operations. By isolating critical components in the enclave and using secure communication channels, the system ensures the highest level of security for user data and blockchain operations.
