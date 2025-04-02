# Neo N3 Service Layer WebSocket API Guide

## Overview

The Neo N3 Service Layer provides a WebSocket API for real-time updates and notifications. This guide explains how to connect to and use the WebSocket API effectively.

## Connection

Connect to the WebSocket API using the following URL:
```javascript
const ws = new WebSocket('wss://api.neo-service-layer.io/v1/ws');
```

### Authentication

WebSocket connections require JWT authentication. Send your authentication token immediately after connecting:

```javascript
ws.onopen = () => {
  ws.send(JSON.stringify({
    type: 'auth',
    token: 'your_jwt_token_here'
  }));
};
```

## Available Channels

### 1. Price Feed Channel

Subscribe to real-time price updates for trading pairs:

```javascript
// Subscribe to price updates
ws.send(JSON.stringify({
  type: 'subscribe',
  channel: 'prices',
  symbols: ['NEO/USD', 'GAS/USD']
}));

// Handle price updates
ws.onmessage = (event) => {
  const data = JSON.parse(event.data);
  if (data.channel === 'prices') {
    console.log('Price update:', data);
    // Example data:
    // {
    //   channel: 'prices',
    //   symbol: 'NEO/USD',
    //   price: '50.00',
    //   timestamp: '2024-03-27T00:00:00Z',
    //   sources: [{
    //     name: 'binance',
    //     price: '50.00',
    //     weight: 1.0
    //   }]
    // }
  }
};
```

### 2. Trigger Events Channel

Receive notifications about trigger executions:

```javascript
// Subscribe to trigger events
ws.send(JSON.stringify({
  type: 'subscribe',
  channel: 'triggers',
  triggerIds: ['trig_123', 'trig_456']  // Optional: specific triggers
}));

// Handle trigger events
ws.onmessage = (event) => {
  const data = JSON.parse(event.data);
  if (data.channel === 'triggers') {
    console.log('Trigger event:', data);
    // Example data:
    // {
    //   channel: 'triggers',
    //   triggerId: 'trig_123',
    //   event: 'execution_completed',
    //   status: 'success',
    //   result: 'Function executed successfully',
    //   timestamp: '2024-03-27T00:00:00Z'
    // }
  }
};
```

### 3. Gas Pool Updates Channel

Monitor gas pool status and allocation changes:

```javascript
// Subscribe to gas pool updates
ws.send(JSON.stringify({
  type: 'subscribe',
  channel: 'gas'
}));

// Handle gas pool updates
ws.onmessage = (event) => {
  const data = JSON.parse(event.data);
  if (data.channel === 'gas') {
    console.log('Gas pool update:', data);
    // Example data:
    // {
    //   channel: 'gas',
    //   event: 'allocation_updated',
    //   address: 'NXxx...xxxx',
    //   balance: '1000000',
    //   allocated: '500000',
    //   available: '500000',
    //   timestamp: '2024-03-27T00:00:00Z'
    // }
  }
};
```

### 4. Function Execution Events Channel

Monitor serverless function executions:

```javascript
// Subscribe to function execution events
ws.send(JSON.stringify({
  type: 'subscribe',
  channel: 'functions',
  functionIds: ['func_123']  // Optional: specific functions
}));

// Handle function execution events
ws.onmessage = (event) => {
  const data = JSON.parse(event.data);
  if (data.channel === 'functions') {
    console.log('Function execution:', data);
    // Example data:
    // {
    //   channel: 'functions',
    //   functionId: 'func_123',
    //   event: 'execution_started',
    //   timestamp: '2024-03-27T00:00:00Z',
    //   metadata: {
    //     triggerId: 'trig_123',
    //     runtime: 'javascript'
    //   }
    // }
  }
};
```

## Message Types

### 1. Subscribe Message

```javascript
{
  type: 'subscribe',
  channel: string,  // 'prices', 'triggers', 'gas', 'functions'
  // Optional channel-specific parameters:
  symbols?: string[],      // For prices channel
  triggerIds?: string[],   // For triggers channel
  functionIds?: string[]   // For functions channel
}
```

### 2. Unsubscribe Message

```javascript
{
  type: 'unsubscribe',
  channel: string,  // Channel to unsubscribe from
  // Optional channel-specific parameters (same as subscribe)
}
```

### 3. Heartbeat Message

The server sends heartbeat messages every 30 seconds:

```javascript
{
  type: 'heartbeat',
  timestamp: '2024-03-27T00:00:00Z'
}
```

Respond to heartbeats to maintain the connection:

```javascript
ws.onmessage = (event) => {
  const data = JSON.parse(event.data);
  if (data.type === 'heartbeat') {
    ws.send(JSON.stringify({
      type: 'heartbeat',
      timestamp: new Date().toISOString()
    }));
  }
};
```

## Error Handling

### 1. Connection Errors

```javascript
ws.onerror = (error) => {
  console.error('WebSocket error:', error);
};

ws.onclose = (event) => {
  console.log('WebSocket closed:', event.code, event.reason);
  // Implement reconnection logic
  setTimeout(connectWebSocket, 5000);
};
```

### 2. Error Messages

The server may send error messages:

```javascript
{
  type: 'error',
  code: 'SUBSCRIPTION_FAILED',
  message: 'Invalid subscription parameters',
  details: {
    field: 'symbols',
    reason: 'Invalid trading pair format'
  }
}
```

Handle error messages appropriately:

```javascript
ws.onmessage = (event) => {
  const data = JSON.parse(event.data);
  if (data.type === 'error') {
    console.error('Server error:', data);
    // Implement error-specific handling
  }
};
```

## Best Practices

1. **Connection Management**
   - Implement automatic reconnection
   - Handle connection errors gracefully
   - Respond to heartbeat messages

2. **Subscription Management**
   - Subscribe only to needed channels
   - Unsubscribe when data is no longer needed
   - Handle subscription errors

3. **Message Processing**
   - Validate message format
   - Handle all message types
   - Process messages asynchronously if needed

4. **Error Handling**
   - Implement exponential backoff for reconnection
   - Log errors appropriately
   - Notify users of connection issues

## Example Implementation

Here's a complete example of a WebSocket client:

```javascript
class NeoServiceWebSocket {
  constructor(url, token) {
    this.url = url;
    this.token = token;
    this.subscriptions = new Set();
    this.reconnectAttempts = 0;
    this.maxReconnectAttempts = 5;
    this.connect();
  }

  connect() {
    this.ws = new WebSocket(this.url);
    this.ws.onopen = this.onOpen.bind(this);
    this.ws.onclose = this.onClose.bind(this);
    this.ws.onerror = this.onError.bind(this);
    this.ws.onmessage = this.onMessage.bind(this);
  }

  onOpen() {
    console.log('Connected to WebSocket');
    this.reconnectAttempts = 0;
    
    // Authenticate
    this.ws.send(JSON.stringify({
      type: 'auth',
      token: this.token
    }));

    // Resubscribe to previous channels
    for (const subscription of this.subscriptions) {
      this.ws.send(JSON.stringify(subscription));
    }
  }

  onClose(event) {
    console.log('WebSocket closed:', event.code, event.reason);
    this.reconnect();
  }

  onError(error) {
    console.error('WebSocket error:', error);
  }

  onMessage(event) {
    const data = JSON.parse(event.data);
    
    switch (data.type) {
      case 'heartbeat':
        this.handleHeartbeat(data);
        break;
      case 'error':
        this.handleError(data);
        break;
      default:
        this.handleChannelMessage(data);
    }
  }

  handleHeartbeat(data) {
    this.ws.send(JSON.stringify({
      type: 'heartbeat',
      timestamp: new Date().toISOString()
    }));
  }

  handleError(data) {
    console.error('Server error:', data);
    // Implement error-specific handling
  }

  handleChannelMessage(data) {
    // Emit event for channel-specific handlers
    const event = new CustomEvent(data.channel, { detail: data });
    window.dispatchEvent(event);
  }

  reconnect() {
    if (this.reconnectAttempts >= this.maxReconnectAttempts) {
      console.error('Max reconnection attempts reached');
      return;
    }

    const delay = Math.min(1000 * Math.pow(2, this.reconnectAttempts), 30000);
    this.reconnectAttempts++;

    console.log(`Reconnecting in ${delay}ms...`);
    setTimeout(() => this.connect(), delay);
  }

  subscribe(channel, params = {}) {
    const subscription = {
      type: 'subscribe',
      channel,
      ...params
    };
    
    this.subscriptions.add(subscription);
    if (this.ws.readyState === WebSocket.OPEN) {
      this.ws.send(JSON.stringify(subscription));
    }
  }

  unsubscribe(channel, params = {}) {
    const subscription = {
      type: 'subscribe',
      channel,
      ...params
    };
    
    this.subscriptions.delete(subscription);
    if (this.ws.readyState === WebSocket.OPEN) {
      this.ws.send(JSON.stringify({
        type: 'unsubscribe',
        channel,
        ...params
      }));
    }
  }
}

// Usage example:
const ws = new NeoServiceWebSocket(
  'wss://api.neo-service-layer.io/v1/ws',
  'your_jwt_token_here'
);

// Subscribe to channels
ws.subscribe('prices', { symbols: ['NEO/USD'] });
ws.subscribe('triggers', { triggerIds: ['trig_123'] });

// Handle channel messages
window.addEventListener('prices', (event) => {
  console.log('Price update:', event.detail);
});

window.addEventListener('triggers', (event) => {
  console.log('Trigger event:', event.detail);
});
```

## Support

For WebSocket API support:
- Email: api@neo-service-layer.io
- Discord: [Neo Service Layer Community](https://discord.gg/neo-service-layer)
- GitHub Issues: [Report a bug](https://github.com/r3e-network/neo_service_layer/issues) 