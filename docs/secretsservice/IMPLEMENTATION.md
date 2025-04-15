# Secrets Service Implementation

*Last Updated: 2025-04-14*

## Overview

This document provides technical details on the implementation of the Secrets Service, including technologies used, code organization, and key implementation patterns.

## Technology Stack

The Secrets Service is built using the following technologies:

- **Programming Language**: Go 1.21+
- **Frameworks and Libraries**:
  - gRPC & Protocol Buffers for service communication
  - HashiCorp Vault (as optional backend)
  - PKCS#11 for HSM integration
  - OpenTelemetry for observability
  - Redis for distributed locking and caching
  - PostgreSQL for metadata storage
  - NATS for event messaging

## Code Organization

The Secrets Service codebase follows a clean architecture pattern with the following structure:

```
internal/secretservice/
├── api/                    # API layer (gRPC, HTTP)
│   ├── grpc/               # gRPC server implementations
│   ├── http/               # HTTP handlers
│   └── middleware/         # Common API middleware
│
├── core/                   # Core business logic
│   ├── service/            # Service implementations
│   ├── domain/             # Domain models and business rules
│   └── port/               # Interface definitions (ports)
│
├── adapters/               # External adapters (implementations of ports)
│   ├── store/              # Storage implementations
│   │   ├── vault/          # HashiCorp Vault adapter
│   │   ├── postgres/       # PostgreSQL adapter
│   │   ├── hsm/            # HSM adapter
│   │   └── memory/         # In-memory adapter (for testing)
│   │
│   ├── crypto/             # Cryptographic implementations
│   │   ├── software/       # Software-based crypto
│   │   ├── hardware/       # Hardware-based crypto
│   │   └── tee/            # TEE-based crypto
│   │
│   └── messaging/          # Messaging implementations
│       ├── nats/           # NATS implementation
│       └── memory/         # In-memory implementation (for testing)
│
├── config/                 # Configuration management
│
└── util/                   # Common utilities
```

## Key Interfaces

The Secrets Service uses a port-adapter pattern. Core interfaces include:

### Secret Storage Interface

```go
type ISecretStore interface {
    Create(ctx context.Context, secret domain.Secret) (string, error)
    Get(ctx context.Context, id string) (domain.Secret, error)
    GetValue(ctx context.Context, id string, version int) (string, error)
    Update(ctx context.Context, secret domain.Secret) error
    Delete(ctx context.Context, id string) error
    List(ctx context.Context, namespace string, options ListOptions) ([]domain.Secret, error)
    Rotate(ctx context.Context, id string) (int, error)
}
```

### Cryptographic Interface

```go
type ICryptoProvider interface {
    Sign(ctx context.Context, keyID string, data []byte, algorithm string) ([]byte, error)
    Verify(ctx context.Context, keyID string, data []byte, signature []byte, algorithm string) (bool, error)
    Encrypt(ctx context.Context, keyID string, plaintext []byte, options EncryptOptions) (EncryptResult, error)
    Decrypt(ctx context.Context, keyID string, ciphertext []byte, options DecryptOptions) ([]byte, error)
    GenerateKey(ctx context.Context, keyType string, options KeyOptions) (domain.Key, error)
}
```

### Access Control Interface

```go
type IAccessControl interface {
    CanAccess(ctx context.Context, principal domain.Principal, resource domain.Resource, operation string) (bool, error)
    CreatePolicy(ctx context.Context, policy domain.AccessPolicy) (string, error)
    GetPolicy(ctx context.Context, id string) (domain.AccessPolicy, error)
    UpdatePolicy(ctx context.Context, policy domain.AccessPolicy) error
    DeletePolicy(ctx context.Context, id string) error
    ListPolicies(ctx context.Context, options ListOptions) ([]domain.AccessPolicy, error)
}
```

## Domain Models

### Secret

```go
type Secret struct {
    ID          string
    Name        string
    Namespace   string
    Type        SecretType
    Description string
    Version     int
    Value       string // Only populated when explicitly requested
    Metadata    map[string]string
    CreatedAt   time.Time
    UpdatedAt   time.Time
    CreatedBy   string
    UpdatedBy   string
    
    RotationPolicy *RotationPolicy
    Versions       []SecretVersion
}

type SecretType string

const (
    SecretTypeKey        SecretType = "key"
    SecretTypeCredential SecretType = "credential"
    SecretTypeToken      SecretType = "token"
)

type SecretVersion struct {
    Version   int
    CreatedAt time.Time
    CreatedBy string
}

type RotationPolicy struct {
    Enabled      bool
    IntervalDays int
    LastRotated  time.Time
    NextRotation time.Time
}
```

### Access Policy

```go
type AccessPolicy struct {
    ID          string
    Name        string
    Description string
    SecretID    string
    ServiceIDs  []string
    Operations  []string
    Conditions  PolicyConditions
    CreatedAt   time.Time
    UpdatedAt   time.Time
    CreatedBy   string
    UpdatedBy   string
}

type PolicyConditions struct {
    IPRange      []string
    TimeWindow   *TimeWindow
    RequireTEE   bool
    TEEMeasurement string
}

type TimeWindow struct {
    StartTime string
    EndTime   string
    TimeZone  string
}
```

## Key Implementation Patterns

### Secret Encryption

Secrets are encrypted using a layered approach:

1. **Data Encryption Key (DEK)**: Each secret is encrypted with a unique DEK
2. **Key Encryption Key (KEK)**: DEKs are encrypted with a KEK
3. **Master Key**: KEKs are encrypted with a master key (stored in HSM if available)

This allows for efficient key rotation without re-encrypting all secrets.

```go
func (s *secretService) encryptSecret(plaintext string) (string, error) {
    // Generate a new DEK for this secret
    dek, err := crypto.GenerateRandomBytes(32)
    if err != nil {
        return "", err
    }
    
    // Encrypt the plaintext with the DEK
    ciphertext, err := crypto.Encrypt(plaintext, dek)
    if err != nil {
        return "", err
    }
    
    // Encrypt the DEK with the KEK
    encryptedDEK, err := s.keyEncryptionKey.Encrypt(dek)
    if err != nil {
        return "", err
    }
    
    // Combine the encrypted DEK and ciphertext
    return encodeSecret(encryptedDEK, ciphertext), nil
}
```

### Authentication Flow

Service-to-service authentication uses mTLS with certificate validation:

```go
func (m *authMiddleware) Authenticate(ctx context.Context) (context.Context, error) {
    // Extract TLS info from context
    p, ok := peer.FromContext(ctx)
    if !ok {
        return nil, status.Error(codes.Unauthenticated, "no peer found")
    }
    
    tlsInfo, ok := p.AuthInfo.(credentials.TLSInfo)
    if !ok {
        return nil, status.Error(codes.Unauthenticated, "no TLS credentials")
    }
    
    // Validate client certificate
    if len(tlsInfo.State.PeerCertificates) == 0 {
        return nil, status.Error(codes.Unauthenticated, "no client certificate")
    }
    
    cert := tlsInfo.State.PeerCertificates[0]
    
    // Verify certificate is valid and trusted
    if err := m.certVerifier.Verify(cert); err != nil {
        return nil, status.Errorf(codes.Unauthenticated, "invalid certificate: %v", err)
    }
    
    // Extract service identity from certificate
    serviceID := cert.Subject.CommonName
    
    // Store authenticated identity in context
    return context.WithValue(ctx, principalKey, domain.Principal{
        ID:   serviceID,
        Type: domain.PrincipalTypeService,
    }), nil
}
```

### Access Control Enforcement

The Secrets Service uses a policy-based access control system:

```go
func (a *accessControl) CanAccess(ctx context.Context, principal domain.Principal, resource domain.Resource, operation string) (bool, error) {
    // Get policies for this resource
    policies, err := a.policyStore.GetPoliciesForResource(ctx, resource.ID)
    if err != nil {
        return false, err
    }
    
    // Check each policy
    for _, policy := range policies {
        // Check if principal is allowed by this policy
        if !containsString(policy.ServiceIDs, principal.ID) {
            continue
        }
        
        // Check if operation is allowed
        if !containsString(policy.Operations, operation) {
            continue
        }
        
        // Check conditions
        if !a.evaluateConditions(ctx, policy.Conditions) {
            continue
        }
        
        // Access granted
        return true, nil
    }
    
    // No matching policy found
    return false, nil
}
```

### Audit Logging

All sensitive operations are logged with detailed context:

```go
func (s *secretService) auditLog(ctx context.Context, action string, resource domain.Resource, result string, err error) {
    principal := getPrincipalFromContext(ctx)
    
    entry := domain.AuditEntry{
        Timestamp: time.Now(),
        Action:    action,
        Principal: principal,
        Resource:  resource,
        Result:    result,
        Error:     err,
        RequestID: getRequestIDFromContext(ctx),
        ClientIP:  getClientIPFromContext(ctx),
    }
    
    // Log to storage backend asynchronously
    go func() {
        if logErr := s.auditLogger.Log(ctx, entry); logErr != nil {
            s.logger.Error("Failed to write audit log", zap.Error(logErr))
        }
    }()
    
    // Always log to local logger as well
    logFields := []zap.Field{
        zap.String("action", action),
        zap.String("principal", principal.ID),
        zap.String("principal_type", string(principal.Type)),
        zap.String("resource", resource.ID),
        zap.String("resource_type", string(resource.Type)),
        zap.String("result", result),
        zap.Error(err),
        zap.String("request_id", entry.RequestID),
        zap.String("client_ip", entry.ClientIP),
    }
    
    if err != nil {
        s.logger.Error("Audit", logFields...)
    } else {
        s.logger.Info("Audit", logFields...)
    }
}
```

## Error Handling

The service follows a standardized error handling pattern:

```go
// Define domain error types
type ErrorCode string

const (
    ErrorCodeNotFound           ErrorCode = "NOT_FOUND"
    ErrorCodeAlreadyExists      ErrorCode = "ALREADY_EXISTS"
    ErrorCodeInvalidArgument    ErrorCode = "INVALID_ARGUMENT"
    ErrorCodePermissionDenied   ErrorCode = "PERMISSION_DENIED"
    ErrorCodeUnauthenticated    ErrorCode = "UNAUTHENTICATED"
    ErrorCodeInternal           ErrorCode = "INTERNAL"
)

type Error struct {
    Code    ErrorCode
    Message string
    Details map[string]interface{}
    cause   error
}

// Map domain errors to gRPC status codes
func toGRPCError(err error) error {
    var domainErr *Error
    if !errors.As(err, &domainErr) {
        return status.Error(codes.Internal, "internal error")
    }
    
    var code codes.Code
    switch domainErr.Code {
    case ErrorCodeNotFound:
        code = codes.NotFound
    case ErrorCodeAlreadyExists:
        code = codes.AlreadyExists
    case ErrorCodeInvalidArgument:
        code = codes.InvalidArgument
    case ErrorCodePermissionDenied:
        code = codes.PermissionDenied
    case ErrorCodeUnauthenticated:
        code = codes.Unauthenticated
    default:
        code = codes.Internal
    }
    
    st := status.New(code, domainErr.Message)
    
    if len(domainErr.Details) > 0 {
        detailsProto := &errdetails.ErrorInfo{
            Reason:   string(domainErr.Code),
            Metadata: stringMapToProtoMap(domainErr.Details),
        }
        
        st, _ = st.WithDetails(detailsProto)
    }
    
    return st.Err()
}
```

## Scaling and Performance

The Secrets Service is designed for high throughput and low latency:

- **Connection Pooling**: Database and Vault client connections are pooled
- **Caching**: Frequently accessed secrets and policies are cached with Redis
- **Read Replicas**: Read operations are distributed across replicas
- **Horizontal Scaling**: Multiple service instances can be deployed behind a load balancer
- **Rate Limiting**: Prevents abuse and ensures fair resource allocation

```go
// Example caching implementation
func (s *cachedSecretStore) Get(ctx context.Context, id string) (domain.Secret, error) {
    // Try cache first
    cacheKey := fmt.Sprintf("secret:%s:metadata", id)
    if cachedSecret, found := s.cache.Get(ctx, cacheKey); found {
        return cachedSecret.(domain.Secret), nil
    }
    
    // Cache miss, get from primary store
    secret, err := s.primaryStore.Get(ctx, id)
    if err != nil {
        return domain.Secret{}, err
    }
    
    // Store in cache
    s.cache.Set(ctx, cacheKey, secret, s.cacheTTL)
    
    return secret, nil
}
```

## Testing Strategy

The Secrets Service uses a comprehensive testing approach:

- **Unit Tests**: Test individual components with mocked dependencies
- **Integration Tests**: Test interactions between components
- **End-to-End Tests**: Test the full service with real dependencies (in a test environment)
- **Performance Tests**: Ensure the service meets performance requirements

Example test pattern:

```go
func TestSecretCreation(t *testing.T) {
    // Set up
    mockStore := mock.NewMockSecretStore()
    mockACL := mock.NewMockAccessControl()
    mockCrypto := mock.NewMockCryptoProvider()
    mockAudit := mock.NewMockAuditLogger()
    
    service := New(mockStore, mockACL, mockCrypto, mockAudit)
    
    // Set expectations
    mockACL.EXPECT().CanAccess(
        gomock.Any(),
        gomock.Any(),
        domain.Resource{Type: domain.ResourceTypeNamespace, ID: "test"},
        "create",
    ).Return(true, nil)
    
    mockCrypto.EXPECT().
        Encrypt(gomock.Any(), gomock.Any(), gomock.Any()).
        Return([]byte("encrypted"), nil)
    
    mockStore.EXPECT().
        Create(gomock.Any(), gomock.Any()).
        Return("new-id", nil)
    
    mockAudit.EXPECT().
        Log(gomock.Any(), gomock.Any()).
        Return(nil)
    
    // Execute
    secret := domain.Secret{
        Name:      "test-secret",
        Namespace: "test",
        Type:      domain.SecretTypeCredential,
        Value:     "test-value",
    }
    
    ctx := contextWithPrincipal(domain.Principal{
        ID:   "test-service",
        Type: domain.PrincipalTypeService,
    })
    
    id, err := service.CreateSecret(ctx, secret)
    
    // Assert
    assert.NoError(t, err)
    assert.Equal(t, "new-id", id)
}
```

## Deployment Considerations

The Secrets Service deployment requires special considerations:

- **Secure Initialization**: Bootstrap encryption keys and access policies
- **Secret Zero Problem**: Initial authentication credentials must be securely provided
- **HSM Integration**: Hardware Security Modules should be provisioned and configured
- **Network Security**: Network policies should restrict access to the service
- **Monitoring**: Comprehensive monitoring for security events and performance

## Development Workflows

When working on the Secrets Service, follow these guidelines:

1. Run the service with in-memory backends for local development:
   ```bash
   go run cmd/secrets_service/main.go --config config/dev.yaml
   ```

2. Generate protocol buffer files when API changes:
   ```bash
   make proto
   ```

3. Run tests before submitting changes:
   ```bash
   make test
   ```

4. Run linters to ensure code quality:
   ```bash
   make lint
   ```

5. Update documentation when adding or changing functionality.

## Tools and Scripts

Useful scripts for development:

- `scripts/gen_proto.sh`: Generates protocol buffer code
- `scripts/setup_dev.sh`: Sets up a development environment
- `scripts/vault_dev.sh`: Starts a development Vault server
- `scripts/run_integration_tests.sh`: Runs integration tests

> **Note:** This service was previously also documented as the Key Service. Both refer to the same service functionality.
