# Secrets Service

The Secrets Service provides secure storage and management of sensitive information like API keys, credentials, and tokens for the Neo Service Layer.

## Architecture

The Secrets Service is designed with security as the primary concern:

1. **Encryption**: All secrets are encrypted at rest using AES-GCM.
2. **Access Control**: Secrets are tied to user accounts and can only be accessed by their owners.
3. **Expiration**: Optional automatic expiration of secrets to reduce security risks.
4. **Metadata**: Support for tags and attributes to organize and manage secrets.

## Features

- **Secret Management**: Store, retrieve, and delete sensitive information.
- **Encryption**: Strong encryption for all stored secrets.
- **Expiration**: Configurable time-to-live (TTL) for secrets.
- **Tagging**: Organize secrets with metadata and tags.
- **Read-Only Protection**: Prevent accidental modification of critical secrets.
- **Size Limits**: Configurable limits on secret size and count.

## Configuration

The service can be configured with the following options:

- `EncryptionKey`: Key used for encrypting secrets (required).
- `MaxSecretSize`: Maximum size of individual secrets (default: 10KB).
- `MaxSecretsPerUser`: Maximum number of secrets per user (default: 100).
- `SecretExpiryEnabled`: Whether secret expiration is enabled (default: true).
- `DefaultTTL`: Default time-to-live for secrets (default: 24 hours).

## Usage

### Storing a Secret

```go
err := secretservice.StoreSecret(
    ctx,
    userAddress,
    "api-key",
    "your-api-key-value",
    map[string]interface{}{
        "ttl":      24 * time.Hour,
        "tags":     []string{"api", "production"},
        "readOnly": true,
    },
)
```

### Retrieving a Secret

```go
value, err := secretservice.GetSecret(ctx, userAddress, "api-key")
```

### Listing Secrets

```go
keys, err := secretservice.ListSecrets(ctx, userAddress)
```

### Deleting a Secret

```go
err := secretservice.DeleteSecret(ctx, userAddress, "api-key")
```

## Security Considerations

- The encryption key should be stored securely and never hardcoded in source code.
- In production, consider using a key management system for the encryption key.
- Secrets are only encrypted at rest; application code must handle them securely in memory.
- Set appropriate TTLs to ensure secrets don't persist longer than needed.
- Use read-only flags for important secrets to prevent accidental deletion.
- Monitor and audit access to secrets for security compliance.