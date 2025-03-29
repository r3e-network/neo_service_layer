# useWallet Hook

## Overview
The `useWallet` hook provides Neo N3 wallet integration for React components. It handles wallet connection, disconnection, message signing, and state management.

## Features
- Wallet connection management
- Message signing
- Connection state tracking
- Error handling
- TypeScript support

## API Reference

### Hook Signature
```typescript
function useWallet(): {
  isConnected: boolean;
  neoAddress: string;
  provider: any;
  connect: () => Promise<any>;
  disconnect: () => Promise<void>;
  signMessage: (message: string) => Promise<string>;
}
```

### Return Values

#### State
- `isConnected`: Whether wallet is connected
- `neoAddress`: Connected Neo N3 address
- `provider`: Wallet provider instance

#### Methods
- `connect()`: Connect to Neo N3 wallet
- `disconnect()`: Disconnect from wallet
- `signMessage(message: string)`: Sign a message

## Usage Examples

### Basic Usage
```typescript
import { useWallet } from '../src/app/hooks/useWallet';

function WalletComponent() {
  const { 
    isConnected, 
    neoAddress, 
    connect, 
    disconnect 
  } = useWallet();

  return (
    <div>
      {isConnected ? (
        <>
          <p>Connected: {neoAddress}</p>
          <button onClick={disconnect}>Disconnect</button>
        </>
      ) : (
        <button onClick={connect}>Connect Wallet</button>
      )}
    </div>
  );
}
```

### Message Signing
```typescript
function SignMessageComponent() {
  const { signMessage, isConnected } = useWallet();
  const [signature, setSignature] = useState('');

  const handleSign = async () => {
    try {
      const sig = await signMessage('Hello Neo');
      setSignature(sig);
    } catch (error) {
      console.error('Signing failed:', error);
    }
  };

  return (
    <div>
      {isConnected && (
        <button onClick={handleSign}>Sign Message</button>
      )}
      {signature && (
        <p>Signature: {signature}</p>
      )}
    </div>
  );
}
```

### With Error Handling
```typescript
function WalletWithErrors() {
  const { connect } = useWallet();
  const [error, setError] = useState('');

  const handleConnect = async () => {
    try {
      await connect();
    } catch (error) {
      setError(error.message);
    }
  };

  return (
    <div>
      <button onClick={handleConnect}>Connect</button>
      {error && (
        <p className="error">{error}</p>
      )}
    </div>
  );
}
```

## State Management

### Initial State
```typescript
const initialState = {
  isConnected: false,
  neoAddress: '',
  provider: null,
};
```

### State Updates
```typescript
// After successful connection
setState({
  isConnected: true,
  neoAddress: provider.address,
  provider: provider,
});

// After disconnection
setState({
  isConnected: false,
  neoAddress: '',
  provider: null,
});
```

## Error Handling

### Common Errors
1. Connection failed
2. User rejected
3. Network error
4. Signing failed

### Error Handling Example
```typescript
const connect = async () => {
  try {
    const provider = await connectToWallet();
    // Update state
  } catch (error) {
    console.error('Connection failed:', error);
    throw error;
  }
};
```

## Testing

### Unit Tests
```typescript
describe('useWallet', () => {
  it('should initialize disconnected', () => {
    const { result } = renderHook(() => useWallet());
    expect(result.current.isConnected).toBe(false);
  });

  it('should connect successfully', async () => {
    const { result } = renderHook(() => useWallet());
    await act(async () => {
      await result.current.connect();
    });
    expect(result.current.isConnected).toBe(true);
  });
});
```

## Performance

### Optimization
1. Memoize callbacks
2. Minimize state updates
3. Clean up listeners
4. Handle unmounting

### Example
```typescript
const connect = useCallback(async () => {
  // Implementation
}, []);

useEffect(() => {
  // Setup listeners
  return () => {
    // Cleanup listeners
  };
}, []);
```

## Security Considerations

### Best Practices
1. Validate signatures
2. Verify network
3. Handle timeouts
4. Secure storage
5. Clear on disconnect

### Implementation
```typescript
const signMessage = async (message: string) => {
  if (!isConnected) {
    throw new Error('Wallet not connected');
  }
  
  try {
    const signature = await provider.signMessage(message);
    // Verify signature
    return signature;
  } catch (error) {
    console.error('Signing failed:', error);
    throw error;
  }
};
```