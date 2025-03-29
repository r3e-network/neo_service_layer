# Authentication Utilities

## Overview
The authentication utilities provide essential functionality for verifying Neo N3 signatures and addresses. These utilities ensure secure authentication without traditional login/registration, using blockchain-based identity instead.

## Features
- Signature verification
- Neo N3 address validation
- No traditional login required
- Blockchain-based identity

## API Reference

### `verifySignature(signature: string)`
Verifies a Neo N3 signature.

```typescript
async function verifySignature(signature: string): Promise<boolean>
```

**Parameters:**
- `signature`: Neo N3 signature to verify

**Returns:**
- `true` if signature is valid, `false` otherwise

**Example:**
```typescript
const signature = await wallet.signMessage('Hello Neo');
const isValid = await verifySignature(signature);
```

### `verifyNeoAddress(address: string)`
Validates a Neo N3 address format.

```typescript
function verifyNeoAddress(address: string): boolean
```

**Parameters:**
- `address`: Neo N3 address to validate

**Returns:**
- `true` if address format is valid, `false` otherwise

**Example:**
```typescript
const address = 'NXv29LiogxZk6KvXnThwMgJxKanYAKMzHd';
const isValid = verifyNeoAddress(address);
```

## Authentication Flow

### 1. Challenge Generation
```typescript
const challenge = generateChallenge();
// Send challenge to client
```

### 2. Signature Creation (Client Side)
```typescript
const signature = await wallet.signMessage(challenge);
// Send signature back to server
```

### 3. Verification (Server Side)
```typescript
const isValid = await verifySignature(signature);
if (isValid) {
  // Grant access
} else {
  // Deny access
}
```

## Security Considerations

### Signature Verification
- Verify signature format
- Check signature expiration
- Validate signing address
- Prevent replay attacks

### Address Validation
- Check address format
- Verify checksum
- Validate network prefix
- Check address length

### Best Practices
1. Always verify signatures
2. Validate addresses before use
3. Implement rate limiting
4. Log authentication attempts
5. Monitor for suspicious activity

## Error Handling

### Common Errors
1. Invalid signature format
2. Expired signature
3. Invalid address format
4. Network mismatch

### Error Handling Example
```typescript
try {
  const isValid = await verifySignature(signature);
  if (!isValid) {
    // Handle invalid signature
  }
} catch (error) {
  console.error('Error verifying signature:', error);
  // Handle error appropriately
}
```

## Testing

### Unit Tests
```typescript
describe('Authentication', () => {
  it('should verify valid signature', async () => {
    const signature = 'valid-signature';
    expect(await verifySignature(signature)).toBe(true);
  });

  it('should reject invalid signature', async () => {
    const signature = 'invalid-signature';
    expect(await verifySignature(signature)).toBe(false);
  });
});
```

## Performance
- Cache validation results
- Implement rate limiting
- Optimize signature verification
- Monitor response times

## Monitoring

### Metrics to Track
1. Authentication attempts
2. Success/failure rates
3. Response times
4. Error rates
5. Suspicious patterns

### Logging
```typescript
logger.info('Authentication attempt', {
  address,
  success: isValid,
  timestamp: new Date().toISOString()
});
```

## Integration

### With API Endpoints
```typescript
app.post('/api/authenticate', async (req, res) => {
  const { signature, address } = req.body;
  
  if (!verifyNeoAddress(address)) {
    return res.status(400).json({ error: 'Invalid address' });
  }
  
  const isValid = await verifySignature(signature);
  if (!isValid) {
    return res.status(401).json({ error: 'Invalid signature' });
  }
  
  // Grant access
});
```

### With Middleware
```typescript
const authMiddleware = async (req, res, next) => {
  const signature = req.headers['x-neo-signature'];
  const address = req.headers['x-neo-address'];
  
  if (!signature || !address) {
    return res.status(401).json({ error: 'Missing credentials' });
  }
  
  if (!verifyNeoAddress(address)) {
    return res.status(400).json({ error: 'Invalid address' });
  }
  
  const isValid = await verifySignature(signature);
  if (!isValid) {
    return res.status(401).json({ error: 'Invalid signature' });
  }
  
  req.neoAddress = address;
  next();
};
```

## Rate Limiting
```typescript
const rateLimiter = {
  windowMs: 15 * 60 * 1000, // 15 minutes
  max: 100 // limit each IP to 100 requests per windowMs
};
```

## Dependencies
- @cityofzion/neon-js: Neo N3 SDK
- TypeScript types
- Testing framework

## Configuration
```typescript
const config = {
  networkMagic: 860833102, // Neo N3 MainNet
  signatureTimeout: 5 * 60 * 1000, // 5 minutes
  maxRetries: 3
};
```

## Troubleshooting

### Common Issues
1. Invalid signature format
2. Network mismatch
3. Expired signature
4. Rate limit exceeded

### Solutions
1. Verify signature format
2. Check network configuration
3. Validate timestamp
4. Implement backoff strategy