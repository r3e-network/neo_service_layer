# Secure Vault Management

## Overview
The Secure Vault provides a robust secret management system for the Neo Service Layer. It handles secret storage, access control, logging, and key rotation within a Trusted Execution Environment (TEE).

## Features
- Secure secret storage
- Access logging
- Key rotation
- Metrics tracking
- Permission management
- TEE integration

## API Reference

### `SecretVault`

#### Constructor
```typescript
constructor(config: VaultConfig)
```

**Parameters:**
- `config`: Configuration object
  - `teeEnabled`: Whether TEE is enabled
  - `backupEnabled`: Whether backup is enabled
  - `rotationPeriod`: Key rotation period in milliseconds

#### `listSecrets(neoAddress: string)`
Lists secrets accessible to a Neo address.

```typescript
async listSecrets(neoAddress: string): Promise<any[]>
```

**Parameters:**
- `neoAddress`: Neo N3 address

**Returns:**
- Array of accessible secrets

#### `getSecret(id: string)`
Retrieves a specific secret.

```typescript
async getSecret(id: string): Promise<any>
```

**Parameters:**
- `id`: Secret identifier

**Returns:**
- Secret object if found

#### `createSecret(secret: any)`
Creates a new secret.

```typescript
async createSecret(secret: any): Promise<void>
```

**Parameters:**
- `secret`: Secret object with:
  - `id`: Unique identifier
  - `name`: Secret name
  - `value`: Secret value
  - `neoAddress`: Owner's Neo address
  - `permissions`: Access permissions
  - `teeConfig`: TEE configuration

#### `updateSecret(secret: any)`
Updates an existing secret.

```typescript
async updateSecret(secret: any): Promise<void>
```

**Parameters:**
- `secret`: Updated secret object

#### `deleteSecret(id: string)`
Deletes a secret.

```typescript
async deleteSecret(id: string): Promise<void>
```

**Parameters:**
- `id`: Secret identifier

#### `logAccess(accessLog: any)`
Logs secret access.

```typescript
async logAccess(accessLog: any): Promise<void>
```

**Parameters:**
- `accessLog`: Access log entry

#### `updateAccessMetrics(secretId: string)`
Updates access metrics for a secret.

```typescript
async updateAccessMetrics(secretId: string): Promise<void>
```

**Parameters:**
- `secretId`: Secret identifier

#### `rotateSecrets()`
Rotates keys for secrets.

```typescript
async rotateSecrets(): Promise<void>
```

## Secret Structure
```typescript
interface Secret {
  id: string;
  name: string;
  value: string;
  neoAddress: string;
  createdAt: string;
  lastAccessed: string;
  accessCount: number;
  permissions: {
    functionIds: string[];
    roles: string[];
  };
  teeConfig: {
    encryptionKeyId: string;
    attestationToken: string;
    mrEnclave: string;
  };
}
```

## Access Log Structure
```typescript
interface AccessLog {
  id: string;
  secretId: string;
  timestamp: string;
  functionId: string;
  neoAddress: string;
  status: 'granted' | 'denied';
  teeVerification: {
    attestationValid: boolean;
    mrEnclaveMatch: boolean;
  };
}
```

## Usage Examples

### Creating a Secret
```typescript
const vault = new SecretVault({
  teeEnabled: true,
  backupEnabled: true,
  rotationPeriod: 24 * 60 * 60 * 1000 // 24 hours
});

await vault.createSecret({
  id: 'secret-1',
  name: 'API Key',
  value: 'my-api-key',
  neoAddress: 'NXv29LiogxZk6KvXnThwMgJxKanYAKMzHd',
  permissions: {
    functionIds: ['func-1'],
    roles: ['admin']
  }
});
```

### Accessing a Secret
```typescript
const secret = await vault.getSecret('secret-1');
await vault.logAccess({
  secretId: 'secret-1',
  functionId: 'func-1',
  neoAddress: 'NXv29LiogxZk6KvXnThwMgJxKanYAKMzHd'
});
```

## Security Considerations

### Access Control
- Neo address-based ownership
- Function-level permissions
- Role-based access
- TEE verification

### Key Management
- Regular key rotation
- Secure key storage in TEE
- Key backup (optional)
- Key destruction

### Monitoring
- Access logging
- Usage metrics
- Error tracking
- Suspicious activity detection

## Error Handling

### Common Errors
1. `SecretNotFoundError`
2. `PermissionDeniedError`
3. `TEEVerificationError`
4. `RotationError`

### Error Handling Example
```typescript
try {
  await vault.getSecret('secret-1');
} catch (error) {
  if (error instanceof SecretNotFoundError) {
    // Handle missing secret
  } else if (error instanceof PermissionDeniedError) {
    // Handle permission issue
  }
  // Log error and take appropriate action
}
```

## Best Practices
1. Regular key rotation
2. Comprehensive access logging
3. Proper error handling
4. Regular security audits
5. Monitoring and alerting
6. Backup strategy

## Performance
- Caching where appropriate
- Batch operations
- Efficient key rotation
- Optimized access patterns

## Testing
- Unit tests
- Integration tests
- Security tests
- Performance tests
- Backup/restore tests

## Monitoring
- Access patterns
- Error rates
- Key rotation status
- TEE status
- Performance metrics

## Backup and Recovery
- Optional backup support
- Secure backup storage
- Recovery procedures
- Backup verification

## Compliance
- Audit logging
- Access control
- Key management
- Data protection
- Monitoring requirements