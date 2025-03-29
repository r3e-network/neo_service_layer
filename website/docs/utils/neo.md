# Neo N3 Utilities

## Overview
The Neo N3 utilities provide essential functionality for interacting with the Neo N3 blockchain. These utilities handle address formatting, validation, and conversion operations commonly needed when working with Neo N3.

## Features
- Address formatting
- Contract hash validation
- Script hash conversion
- Address validation

## API Reference

### `formatNeoAddress(address: string)`
Formats a Neo N3 address for display.

```typescript
function formatNeoAddress(address: string): string
```

**Parameters:**
- `address`: Full Neo N3 address

**Returns:**
- Formatted address (e.g., "NXv2...Hd")

**Example:**
```typescript
const address = "NXv29LiogxZk6KvXnThwMgJxKanYAKMzHd";
const formatted = formatNeoAddress(address); // Returns "NXv2...Hd"
```

### `validateContractHash(hash: string)`
Validates a Neo N3 contract hash.

```typescript
async function validateContractHash(hash: string): Promise<boolean>
```

**Parameters:**
- `hash`: Contract hash to validate

**Returns:**
- `true` if valid, `false` otherwise

**Example:**
```typescript
const hash = "0x1234567890abcdef1234567890abcdef12345678";
const isValid = await validateContractHash(hash);
```

### `getNeoScriptHash(address: string)`
Converts a Neo N3 address to script hash.

```typescript
function getNeoScriptHash(address: string): string
```

**Parameters:**
- `address`: Neo N3 address

**Returns:**
- Script hash for the address

**Example:**
```typescript
const address = "NXv29LiogxZk6KvXnThwMgJxKanYAKMzHd";
const scriptHash = getNeoScriptHash(address);
```

## Address Format
Neo N3 addresses follow this format:
- Base58 encoded
- 34 characters long
- Starts with 'N'
- Includes checksum

Example: `NXv29LiogxZk6KvXnThwMgJxKanYAKMzHd`

## Contract Hash Format
Neo N3 contract hashes follow this format:
- Starts with '0x'
- 40 hexadecimal characters
- Case insensitive

Example: `0x1234567890abcdef1234567890abcdef12345678`

## Script Hash Format
Script hashes in Neo N3:
- 40 hexadecimal characters
- Represents contract or account
- Used in smart contracts

Example: `1234567890abcdef1234567890abcdef12345678`

## Error Handling

### Common Errors
1. Invalid address format
2. Invalid contract hash
3. Conversion errors

### Error Handling Example
```typescript
try {
  const scriptHash = getNeoScriptHash(address);
  if (!scriptHash) {
    // Handle invalid address
  }
} catch (error) {
  console.error('Error getting script hash:', error);
  // Handle error appropriately
}
```

## Best Practices

### Address Handling
1. Always validate addresses before processing
2. Use proper error handling
3. Format addresses for display
4. Maintain original address for operations

### Contract Hash Handling
1. Validate hash format
2. Convert to consistent case
3. Verify on blockchain when needed
4. Cache validation results

### Script Hash Operations
1. Verify input address
2. Handle conversion errors
3. Validate output format
4. Cache common conversions

## Testing
- Unit tests for all functions
- Test with invalid inputs
- Test edge cases
- Performance testing

## Security Considerations
1. Input validation
2. Error handling
3. Address verification
4. Hash verification

## Performance
- Cache common conversions
- Batch operations when possible
- Minimize blockchain calls
- Use efficient algorithms

## Integration

### With Smart Contracts
```typescript
const scriptHash = getNeoScriptHash(address);
// Use script hash in contract calls
```

### With Wallet
```typescript
const address = "NXv29LiogxZk6KvXnThwMgJxKanYAKMzHd";
const isValid = await validateContractHash(contractHash);
if (isValid) {
  // Proceed with contract interaction
}
```

## Monitoring
- Track validation errors
- Monitor conversion performance
- Log invalid addresses
- Track usage patterns

## Dependencies
- @cityofzion/neon-js: Neo N3 SDK
- Proper TypeScript types
- Testing framework