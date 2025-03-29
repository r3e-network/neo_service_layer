# WebSocket and Authentication Documentation

## WebSocket Server

The WebSocket server provides real-time updates for various services in the Neo Service Layer. It supports authenticated and unauthenticated connections, with certain channels requiring authentication.

### Connection

Connect to the WebSocket server with:
```javascript
const ws = new WebSocket('ws://your-server/ws?token=your-jwt-token');
```

### Channels

1. **Price Updates** (Public)
   - Channel: `price_updates`
   - Subscribe with symbol parameter: `{ type: 'subscribe', data: { channel: 'price_updates', params: { symbol: 'NEO_USDT' } } }`
   - Events:
     - `price_update`: Real-time price updates
     - `source_update`: Price source status updates

2. **Configuration Updates** (Authenticated)
   - Channel: `config_updates`
   - Subscribe: `{ type: 'subscribe', data: { channel: 'config_updates' } }`
   - Events:
     - `config_update`: Configuration changes

### Message Format

All messages follow this format:
```typescript
interface WebSocketMessage {
  type: string;
  data: any;
}
```

### Heartbeat

- Server sends ping every 30 seconds
- Client must respond with pong
- Connection times out after 35 seconds without response

## Authentication

The authentication system uses JWT tokens and Neo N3 wallet signatures for secure access.

### Authentication Flow

1. **Get Challenge**
   ```http
   POST /api/auth/challenge
   Content-Type: application/json
   
   {
     "address": "Neo3Address"
   }
   ```
   Response:
   ```json
   {
     "challenge": "randomString",
     "message": "Sign this message to authenticate: randomString"
   }
   ```

2. **Verify Signature**
   ```http
   POST /api/auth/verify
   Content-Type: application/json
   
   {
     "address": "Neo3Address",
     "signature": "SignedMessage",
     "networkMagic": 123456
   }
   ```
   Response:
   ```json
   {
     "token": "jwt_token",
     "address": "Neo3Address",
     "networkMagic": 123456
   }
   ```

3. **Refresh Token**
   ```http
   POST /api/auth/refresh
   Authorization: Bearer current_token
   ```
   Response:
   ```json
   {
     "token": "new_jwt_token",
     "address": "Neo3Address",
     "networkMagic": 123456
   }
   ```

### Protected API Routes

Use the authentication middleware to protect API routes:

```typescript
import { withAuth } from '../server/middleware/auth';

export default withAuth(async (req, res) => {
  // Access authenticated user info
  const { address, networkMagic } = req.user;
  // Handle request
});
```

For optional authentication:
```typescript
import { withOptionalAuth } from '../server/middleware/auth';

export default withOptionalAuth(async (req, res) => {
  // req.user may be undefined if not authenticated
  // Handle request
});
```

## Security Considerations

1. **JWT Token Security**
   - Tokens expire after 24 hours
   - Store JWT_SECRET in environment variables
   - Never expose JWT_SECRET in client-side code

2. **Challenge Security**
   - Challenges expire after 5 minutes
   - Each address can only have one active challenge
   - Challenges are automatically cleaned up

3. **WebSocket Security**
   - Authenticate using JWT tokens in connection URL
   - Channel-specific access control
   - Rate limiting on message handling
   - Automatic timeout for inactive connections

## Error Handling

All components include comprehensive error handling and logging:
- Invalid authentication attempts are logged
- WebSocket connection issues are tracked
- Metrics are collected for monitoring
- Detailed error messages for debugging

## Metrics

The following metrics are tracked:

1. **WebSocket Metrics**
   - `websocket_connections_total`
   - `websocket_connections_active`
   - `websocket_messages_sent_total`
   - `websocket_subscriptions_total`

2. **Authentication Metrics**
   - `auth_challenges_generated_total`
   - `auth_verification_success_total`
   - `auth_verification_failed_total`
   - `auth_token_refresh_success_total`
   - `auth_token_refresh_failed_total`

## Integration Example

```typescript
// Connect to WebSocket with authentication
const ws = new WebSocket(`ws://your-server/ws?token=${jwt_token}`);

// Subscribe to price updates
ws.send(JSON.stringify({
  type: 'subscribe',
  data: {
    channel: 'price_updates',
    params: { symbol: 'NEO_USDT' }
  }
}));

// Handle price updates
ws.onmessage = (event) => {
  const message = JSON.parse(event.data);
  if (message.type === 'price_update') {
    updateUI(message.data);
  }
};

// Handle connection issues
ws.onclose = () => {
  console.log('WebSocket connection closed');
  // Implement reconnection logic
};
```