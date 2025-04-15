# Neo Service Layer: Service-to-Service Integration

*Last Updated: 2023-12-20*

## Scope

This document details the patterns, protocols, and security measures for service-to-service integration within the Neo Service Layer. It includes:

- Communication protocols and patterns between services
- Authentication and authorization mechanisms
- Service discovery implementation
- TEE attestation in service communication flow
- Error handling between services
- Context propagation and distributed tracing
- Secure channel establishment methods

This document does NOT cover:

- General system architecture (see ARCHITECTURE_OVERVIEW.md)
- Specific service APIs (see each service's API_REFERENCE.md)
- Detailed deployment configurations (see DEPLOYMENT.md)
- Service-specific implementation details (see service IMPLEMENTATION.md files)

## Overview

This document details the patterns, protocols, and security measures used for service-to-service integration within the Neo Service Layer. All inter-service communication is designed with security-first principles, leveraging Trusted Execution Environments (TEEs), mutual authentication, and encrypted transport.

## Core Integration Principles

1. **Zero Trust Model**: All service-to-service calls require full authentication and authorization, regardless of network location.

2. **Defense in Depth**: Multiple layers of security controls protect service communications.

3. **Secure by Design**: Security is built into the communication layer, not added as an afterthought.

4. **Observability**: All service interactions are logged, monitored, and traceable.

5. **Resilience**: Communication patterns are designed to handle failures gracefully.

## Communication Protocols

### gRPC

gRPC is the primary protocol for service-to-service communication:

- **Protocol Buffers**: Type-safe, efficient message serialization
- **Bidirectional Streaming**: Support for request/response and streaming patterns
- **Metadata**: Headers for authentication, tracing, and context propagation
- **Deadlines**: Enforced timeouts to prevent cascading failures

Example service definition:

```protobuf
syntax = "proto3";

package functionservice;

service FunctionService {
  // Create a new function
  rpc CreateFunction(CreateFunctionRequest) returns (CreateFunctionResponse);
  
  // Get function details
  rpc GetFunction(GetFunctionRequest) returns (GetFunctionResponse);
  
  // Execute a function in a TEE
  rpc ExecuteFunction(ExecuteFunctionRequest) returns (ExecuteFunctionResponse);
  
  // Stream function execution logs
  rpc StreamFunctionLogs(StreamFunctionLogsRequest) returns (stream LogEntry);
}
```

### HTTP/REST (Internal API Gateway)

For services that require REST interfaces:

- Used primarily for the API Gateway Service
- Converts between external REST and internal gRPC
- Follows standard REST patterns for resource access

## Authentication & Authorization

### Mutual TLS (mTLS)

All service-to-service communication uses mutual TLS:

1. **Certificate Authority (CA)**: Central CA issues and manages service certificates
2. **Service Identity**: Each service has a unique certificate that identifies it
3. **Certificate Rotation**: Automated certificate rotation with short validity periods (24-48 hours)
4. **Revocation**: Support for certificate revocation in case of compromise

Certificate issuance process:

```
┌────────────┐                   ┌────────────┐
│            │ 1. CSR with       │            │
│ Service    ├───────────────────► Certificate│
│            │   Service Identity │ Authority  │
│            │                   │            │
│            │ 2. Signed Cert    │            │
│            │◄───────────────────┤            │
└────────────┘                   └────────────┘
```

### TEE Attestation

Services running in TEEs provide attestation evidence:

1. **Remote Attestation**: TEE produces cryptographic evidence of its state
2. **Evidence Verification**: Evidence verified against expected measurements
3. **Service Binding**: TEE identity bound to service identity
4. **Data Protection**: Sensitive data only sent to verified TEEs

Attestation flow:

```
┌────────────┐                   ┌────────────┐
│ Requesting │ 1. Request with   │ TEE-based  │
│ Service    ├───────────────────► Service    │
│            │   Attestation Nonce            │
│            │                   │            │
│            │ 2. TEE Evidence   │            │
│            │◄───────────────────┤            │
│            │                   │            │
│            │ 3. Verify         │            │
│            │    Evidence       │            │
│            │                   │            │
│            │ 4. Proceed if     │            │
│            │    Valid          │            │
└────────────┘                   └────────────┘
```

### Authorization Policy

After authentication, authorization is enforced:

1. **Service Roles**: Each service has defined roles/capabilities
2. **Access Control Lists**: Explicit permissions for service-to-service calls
3. **Context-Aware**: Authorization decisions consider request context
4. **Least Privilege**: Services have only the minimum permissions needed

Example authorization policy:

```yaml
service: "automation-service"
allowed_calls:
  - target_service: "function-service"
    operations: ["ExecuteFunction", "GetFunction"]
  - target_service: "gas-bank-service"
    operations: ["RequestGas", "CheckBalance"]
  - target_service: "key-service"
    operations: ["Sign"]
  - target_service: "price-feed-service"
    operations: ["GetPrice", "SubscribePriceUpdates"]
```

## Service Discovery

Services discover each other through a secure service registry:

1. **Registration**: Services register their endpoints with health status
2. **Discovery**: Services query registry to find other services
3. **Health Checking**: Registry monitors service health
4. **Metadata**: Service capabilities and versions advertised

Discovery flow:

```
┌────────────┐                   ┌────────────┐
│            │ 1. Register       │            │
│ Service A  ├───────────────────► Service    │
│            │   (name, endpoints)│ Registry   │
│            │                   │            │
│            │ 2. Confirmation   │            │
│            │◄───────────────────┤            │
└────────────┘                   └───┬────────┘
                                     │
┌────────────┐                       │
│            │ 3. Discover Service A │
│ Service B  ├───────────────────────┤
│            │                       │
│            │ 4. Service A Endpoints│
│            │◄───────────────────────┘
└────────────┘
```

## Request Flow Patterns

### Request-Response

Standard synchronous communication pattern:

1. Client service makes request to server service
2. Server processes request and returns response
3. Client waits for response or timeout

Used for:

- Function management (create, get, update)
- Job management
- Key operations

### Streaming

For continuous data or long-running operations:

1. Client service establishes stream with server service
2. Server sends multiple responses over time
3. Either party can terminate the stream

Used for:

- Function execution logs
- Price feed updates
- Event monitoring

### Asynchronous with Callbacks

For operations that may take significant time:

1. Client service initiates request
2. Server acknowledges receipt with operation ID
3. Client can poll for status or register callback
4. Server calls back when operation completes

Used for:

- Long-running function executions
- Blockchain transactions
- Background job processing

## Error Handling

Standardized error handling across all services:

1. **Error Codes**: Well-defined error codes with standardized meanings
2. **Error Details**: Structured error details for programmatic handling
3. **Retry Logic**: Client-side retry with exponential backoff
4. **Circuit Breaking**: Fail fast when downstream services are degraded

Example error response:

```json
{
  "error": {
    "code": "PERMISSION_DENIED",
    "message": "Caller does not have permission to execute function",
    "details": [
      {
        "type": "AuthorizationError",
        "service": "function-service",
        "required_permission": "execute",
        "resource": "function/12345"
      }
    ]
  }
}
```

## Context Propagation

Request context is propagated across service boundaries:

1. **Trace Context**: Distributed tracing IDs
2. **Authentication Context**: Caller identity information
3. **Request Metadata**: Deadline, priority, etc.
4. **Business Context**: Request-specific contextual information

Example context propagation:

```go
// Adding context to outgoing request
ctx := context.Background()
ctx = metadata.AppendToOutgoingContext(ctx, 
    "x-trace-id", traceID,
    "x-caller-id", callerID,
    "x-deadline-ms", deadline.String())

// Reading context in receiving service
md, ok := metadata.FromIncomingContext(ctx)
if ok {
    traceID := md.Get("x-trace-id")[0]
    callerID := md.Get("x-caller-id")[0]
    // Use context information
}
```

## Secure Channel Establishment

The full process for establishing a secure channel between services:

```
┌────────────┐                                  ┌────────────┐
│ Service A  │                                  │ Service B  │
└──────┬─────┘                                  └──────┬─────┘
       │                                               │
       │  1. TLS Handshake with Client Certificate    │
       ├───────────────────────────────────────────────►
       │                                               │
       │  2. Server Verifies Client Certificate       │
       │                                               │
       │  3. Server Presents Certificate              │
       │◄───────────────────────────────────────────────┤
       │                                               │
       │  4. Client Verifies Server Certificate       │
       │                                               │
       │  5. Secure TLS Channel Established           │
       ├───────────────────────────────────────────────►
       │                                               │
       │  6. Client Sends Request with TEE Nonce      │
       ├───────────────────────────────────────────────►
       │                                               │
       │  7. Server Generates TEE Attestation         │
       │                                               │
       │  8. Server Returns Attestation Evidence      │
       │◄───────────────────────────────────────────────┤
       │                                               │
       │  9. Client Verifies Attestation Evidence     │
       │                                               │
       │  10. Higher-Level Protocol Messages          │
       ├───────────────────────────────────────────────►
       │                                               │
       │  11. Service-Specific Processing             │
       │                                               │
       │  12. Response Return                         │
       │◄───────────────────────────────────────────────┤
       │                                               │
```

## Rate Limiting and Backpressure

To prevent service overload:

1. **Server-Side Rate Limiting**: Services limit incoming requests
2. **Client-Side Throttling**: Clients respect service limits
3. **Backpressure Signals**: Services signal when approaching capacity
4. **Graceful Degradation**: Non-critical operations deprioritized under load

Example rate limit configuration:

```yaml
rate_limits:
  global:
    requests_per_second: 1000
    burst: 100
  per_caller:
    automation-service:
      requests_per_second: 500
      burst: 50
    api-service:
      requests_per_second: 300
      burst: 30
  per_method:
    ExecuteFunction:
      requests_per_second: 100
      burst: 20
```

## Security Considerations

### Data Protection

1. **Encryption in Transit**: All data encrypted using TLS 1.3
2. **Sensitive Data Handling**: Sensitive data only sent to attested TEEs
3. **Data Minimization**: Only necessary data included in requests

### Authentication Challenges

1. **Certificate Compromise**: Mitigated through short-lived certificates
2. **TEE Compromise**: Mitigated through measurement verification
3. **Insider Threats**: Mitigated through least privilege and audit logging

### Network Security

1. **Network Segmentation**: Services in separate security groups/subnets
2. **Firewall Rules**: Explicit rules for service-to-service communication
3. **Denial of Service Protection**: Rate limiting and traffic filtering

## Implementation Example

Example Go code for secure service-to-service communication:

```go
// Service client with security configuration
func NewSecureServiceClient(target string) (*grpc.ClientConn, error) {
    // Load TLS credentials
    creds, err := credentials.NewClientTLSFromFile(
        "certs/ca.crt", 
        "service-name")
    if err != nil {
        return nil, err
    }
    
    // Configure connection with security options
    conn, err := grpc.Dial(
        target,
        grpc.WithTransportCredentials(creds),
        grpc.WithUnaryInterceptor(attestationInterceptor),
        grpc.WithStreamInterceptor(tracingStreamInterceptor),
        grpc.WithDefaultCallOptions(grpc.MaxCallRecvMsgSize(maxMsgSize)),
        grpc.WithDefaultServiceConfig(`{
            "loadBalancingPolicy": "round_robin",
            "retryPolicy": {
                "maxAttempts": 3,
                "initialBackoff": "0.1s",
                "maxBackoff": "1s",
                "backoffMultiplier": 2.0,
                "retryableStatusCodes": ["UNAVAILABLE"]
            }
        }`),
    )
    
    return conn, err
}

// Attestation interceptor for verifying TEE evidence
func attestationInterceptor(ctx context.Context, method string, 
                            req, reply interface{}, cc *grpc.ClientConn, 
                            invoker grpc.UnaryInvoker, opts ...grpc.CallOption) error {
    
    // Generate random nonce for attestation
    nonce := generateNonce()
    
    // Add nonce to outgoing context
    ctx = metadata.AppendToOutgoingContext(ctx, "x-attestation-nonce", nonce)
    
    // Process response and verify attestation evidence
    var header metadata.MD
    err := invoker(ctx, method, req, reply, cc, append(opts, grpc.Header(&header))...)
    if err != nil {
        return err
    }
    
    // Extract attestation evidence from response headers
    evidence := header.Get("x-attestation-evidence")
    if len(evidence) == 0 {
        return errors.New("missing attestation evidence")
    }
    
    // Verify the attestation evidence
    if !verifyAttestation(evidence[0], nonce) {
        return errors.New("invalid attestation evidence")
    }
    
    return nil
}

// Service implementation with security checks
func (s *serviceServer) ProcessRequest(ctx context.Context, req *pb.Request) (*pb.Response, error) {
    // Extract caller identity from context
    md, ok := metadata.FromIncomingContext(ctx)
    if !ok {
        return nil, status.Error(codes.Unauthenticated, "missing metadata")
    }
    
    // Get caller identity from TLS common name
    p, ok := peer.FromContext(ctx)
    if !ok {
        return nil, status.Error(codes.Unauthenticated, "no peer info")
    }
    
    tlsInfo, ok := p.AuthInfo.(credentials.TLSInfo)
    if !ok {
        return nil, status.Error(codes.Unauthenticated, "no TLS auth info")
    }
    
    callerID := tlsInfo.State.PeerCertificates[0].Subject.CommonName
    
    // Authorize the request
    if !s.authorizer.CanAccess(callerID, "ProcessRequest") {
        return nil, status.Error(codes.PermissionDenied, "not authorized")
    }
    
    // Add attestation evidence to response headers
    header := metadata.Pairs(
        "x-attestation-evidence", generateAttestationEvidence(ctx),
    )
    grpc.SetHeader(ctx, header)
    
    // Process the request
    // ...
    
    return &pb.Response{}, nil
}
```

## Integration Testing

Testing strategies for service integration:

1. **Contract Testing**: Verify service interfaces conform to contracts
2. **Integration Testing**: End-to-end testing of service interactions
3. **Security Testing**: Verify authentication and authorization
4. **Chaos Testing**: Test resilience to network failures and delays

## Best Practices

1. **Keep Services Independent**: Minimize direct dependencies
2. **Design for Failure**: Assume services may be unavailable
3. **Versioned APIs**: Support backward compatibility
4. **Log Integration Points**: Monitor and alert on integration issues
5. **Security Reviews**: Regular reviews of integration security
