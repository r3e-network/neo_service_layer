# Secrets Service TEE Client

## Overview

The TEE Client for the Secrets Service provides a secure way for services running within Trusted Execution Environment (TEE) enclaves to access secrets. This client establishes authenticated secure channels with the Secrets Service enclave, ensuring that secrets are only accessible to properly attested enclaves.

## Security Features

- **Enclave Attestation**: Verifies the identity and integrity of both client and server enclaves.
- **Secure Channel Communication**: All traffic between enclaves is encrypted and authenticated.
- **Zero Trust Model**: No secrets are accessible without successful attestation and secure channel establishment.
- **Fine-grained Access Control**: Access to secrets is managed through the TEE identity model.

## Usage

### Initialization

To use the TEE client, you need to provide:
1. A logger instance
2. A TEE client instance (from the TEE service)
3. The Secrets Service URL
4. The client enclave ID
5. The Secrets Service enclave ID

```go
import (
    "github.com/r3e-network/neo_service_layer/internal/secretservice/client"
    "github.com/r3e-network/neo_service_layer/internal/teeservice"
)

// Initialize the TEE client
teeClient := teeservice.NewTEEClient(...)
secretsClient, err := client.NewTEESecretsClient(
    logger,
    teeClient,
    "grpc://secrets-service:8080",
    "client-enclave-id",
    "secrets-service-enclave-id",
)
if err != nil {
    // Handle error
}

// Initialize the client (performs attestation and establishes secure channel)
err = secretsClient.Initialize(ctx)
if err != nil {
    // Handle error
}
```

### Secret Operations

#### Get a Secret

```go
secret, err := secretsClient.GetSecret(ctx, "path/to/secret")
if err != nil {
    // Handle error (not found, permission denied, etc.)
}

// Use the secret value
fmt.Println(string(secret.Value))
```

#### Create a Secret

```go
request := model.CreateSecretRequest{
    Path:  "path/to/new/secret",
    Value: []byte("secret-value"),
    Metadata: map[string]string{
        "owner": "service-name",
        "environment": "production",
    },
    RotationPolicy: &model.RotationPolicy{
        Schedule:      "@every 30d",
        Strategy:      model.RotationStrategyReplace,
        VersionsToKeep: 2,
    },
}

secretID, err := secretsClient.CreateSecret(ctx, request)
if err != nil {
    // Handle error
}
```

#### List Secrets

```go
// List secrets with a given prefix, optionally recursively
secrets, err := secretsClient.ListSecrets(ctx, "path/prefix", true)
if err != nil {
    // Handle error
}

for _, secretMeta := range secrets {
    fmt.Printf("Path: %s, Version: %d\n", secretMeta.Path, secretMeta.Version)
}
```

#### Update a Secret

```go
err := secretsClient.UpdateSecret(ctx, "path/to/secret", []byte("new-value"), map[string]string{
    "updated": "true",
})
if err != nil {
    // Handle error
}
```

#### Delete a Secret

```go
err := secretsClient.DeleteSecret(ctx, "path/to/secret")
if err != nil {
    // Handle error
}
```

## Error Handling

The client provides several error types to handle specific failure scenarios:

- `ErrSecretNotFound`: The requested secret path doesn't exist
- `ErrPermissionDenied`: The enclave is not authorized to access the requested secret
- `ErrAttestationFailed`: Failed to perform attestation with the Secrets Service
- `ErrSecureChannelFailed`: Failed to establish a secure channel
- `ErrInvalidResponse`: Received an unexpected response format
- `ErrTEENotInitialized`: The TEE client is not properly initialized

Example error handling:

```go
secret, err := secretsClient.GetSecret(ctx, "path/to/secret")
if errors.Is(err, client.ErrSecretNotFound) {
    // Handle not found case
} else if errors.Is(err, client.ErrPermissionDenied) {
    // Handle permission denied case
} else if err != nil {
    // Handle other errors
}
```

## Security Considerations

1. **Enclave Security**: Ensure your TEE enclave has appropriate security settings and minimal attack surface.
2. **Key Management**: The secure channel keys are managed by the TEE service but should be rotated periodically.
3. **Attestation Policies**: Configure appropriate attestation policies to ensure only trusted enclaves can access secrets.

## Integration with Non-TEE Services

For services that don't run within a TEE, use the standard gRPC client with appropriate authentication mechanisms. The TEE client is specifically designed for secure enclave-to-enclave communication. 