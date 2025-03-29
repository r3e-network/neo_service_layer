# Neo N3 Service Layer SDK Guide

## Overview

The Neo N3 Service Layer provides official SDKs for multiple programming languages to help developers integrate with the service more easily. This guide covers installation, configuration, and usage of the SDKs.

## Available SDKs

- [JavaScript/TypeScript SDK](https://github.com/will/neo-service-sdk-js)
- [Python SDK](https://github.com/will/neo-service-sdk-python)
- [Go SDK](https://github.com/will/neo-service-sdk-go)

## JavaScript/TypeScript SDK

### Installation

```bash
# Using npm
npm install @neo-service/sdk

# Using yarn
yarn add @neo-service/sdk

# Using pnpm
pnpm add @neo-service/sdk
```

### Basic Usage

```typescript
import { NeoServiceClient } from '@neo-service/sdk';

// Initialize the client
const client = new NeoServiceClient({
  baseUrl: 'https://api.neo-service-layer.io/v1',
  jwt: 'your_jwt_token_here'
});

// Example: Create a function
async function createPriceAlertFunction() {
  const func = await client.functions.create({
    name: 'price_alert',
    description: 'NEO price alert function',
    runtime: 'javascript',
    code: `
      async function handler(context, event) {
        const price = await context.priceFeed.getPrice('NEO/USD');
        if (price > 100) {
          await context.notify('NEO price alert', \`Price is \${price}\`);
        }
      }
    `,
    environment: {
      ALERT_THRESHOLD: '100'
    }
  });
  
  console.log('Created function:', func);
}

// Example: Create a trigger
async function createDailyTrigger() {
  const trigger = await client.triggers.create({
    name: 'daily_price_check',
    functionId: 'func_123',
    type: 'schedule',
    schedule: '0 0 * * *'
  });
  
  console.log('Created trigger:', trigger);
}

// Example: Monitor gas balance
async function monitorGasBalance() {
  const balance = await client.gas.getBalance();
  console.log('Gas balance:', balance);
  
  // Request more gas if needed
  if (balance.available < 100000) {
    await client.gas.request({
      amount: '100000'
    });
  }
}

// Example: WebSocket integration
const ws = client.createWebSocket();

ws.subscribe('prices', { symbols: ['NEO/USD'] });
ws.on('prices', (data) => {
  console.log('Price update:', data);
});
```

### Advanced Features

#### 1. Batch Operations

```typescript
// Create multiple functions
const functions = await client.functions.createBatch([
  {
    name: 'function1',
    runtime: 'javascript',
    code: '...'
  },
  {
    name: 'function2',
    runtime: 'python',
    code: '...'
  }
]);

// Create multiple triggers
const triggers = await client.triggers.createBatch([
  {
    name: 'trigger1',
    functionId: 'func_123',
    type: 'schedule',
    schedule: '0 * * * *'
  },
  {
    name: 'trigger2',
    functionId: 'func_456',
    type: 'event',
    event: {
      type: 'blockchain',
      network: 'neo_n3',
      contract: '0x...',
      event: 'Transfer'
    }
  }
]);
```

#### 2. Error Handling

```typescript
import { NeoServiceError } from '@neo-service/sdk';

try {
  await client.functions.create({
    name: 'invalid function',
    runtime: 'unknown'
  });
} catch (error) {
  if (error instanceof NeoServiceError) {
    console.error('API Error:', error.code, error.message);
    console.error('Details:', error.details);
  } else {
    console.error('Unknown error:', error);
  }
}
```

#### 3. Middleware

```typescript
// Add request/response logging
client.use(async (ctx, next) => {
  console.log('Request:', ctx.request);
  const start = Date.now();
  await next();
  console.log('Response:', ctx.response, 'Time:', Date.now() - start);
});

// Add retry logic
client.use(async (ctx, next) => {
  let attempts = 0;
  while (attempts < 3) {
    try {
      await next();
      return;
    } catch (error) {
      if (!error.isRetryable || attempts === 2) throw error;
      attempts++;
      await new Promise(resolve => setTimeout(resolve, 1000 * attempts));
    }
  }
});
```

## Python SDK

### Installation

```bash
# Using pip
pip install neo-service-sdk

# Using poetry
poetry add neo-service-sdk
```

### Basic Usage

```python
from neo_service import NeoServiceClient

# Initialize the client
client = NeoServiceClient(
    base_url='https://api.neo-service-layer.io/v1',
    jwt='your_jwt_token_here'
)

# Example: Create a function
async def create_price_alert_function():
    func = await client.functions.create(
        name='price_alert',
        description='NEO price alert function',
        runtime='python',
        code='''
async def handler(context, event):
    price = await context.price_feed.get_price('NEO/USD')
    if price > 100:
        await context.notify('NEO price alert', f'Price is {price}')
        ''',
        environment={
            'ALERT_THRESHOLD': '100'
        }
    )
    
    print('Created function:', func)

# Example: Create a trigger
async def create_daily_trigger():
    trigger = await client.triggers.create(
        name='daily_price_check',
        function_id='func_123',
        type='schedule',
        schedule='0 0 * * *'
    )
    
    print('Created trigger:', trigger)

# Example: Monitor gas balance
async def monitor_gas_balance():
    balance = await client.gas.get_balance()
    print('Gas balance:', balance)
    
    # Request more gas if needed
    if int(balance.available) < 100000:
        await client.gas.request(
            amount='100000'
        )

# Example: WebSocket integration
ws = client.create_websocket()

def on_price_update(data):
    print('Price update:', data)

ws.subscribe('prices', symbols=['NEO/USD'])
ws.on('prices', on_price_update)
```

### Advanced Features

#### 1. Async Context Manager

```python
async with NeoServiceClient(...) as client:
    func = await client.functions.create(...)
    trigger = await client.triggers.create(...)
```

#### 2. Pagination Helper

```python
async for func in client.functions.list_all(status='active'):
    print('Function:', func)

async for trigger in client.triggers.list_all(type='schedule'):
    print('Trigger:', trigger)
```

## Go SDK

### Installation

```bash
go get github.com/will/neo-service-sdk-go
```

### Basic Usage

```go
package main

import (
    "context"
    "log"
    
    neo "github.com/will/neo-service-sdk-go"
)

func main() {
    // Initialize the client
    client, err := neo.NewClient(
        neo.WithBaseURL("https://api.neo-service-layer.io/v1"),
        neo.WithJWT("your_jwt_token_here"),
    )
    if err != nil {
        log.Fatal(err)
    }
    
    ctx := context.Background()
    
    // Example: Create a function
    func, err := client.Functions.Create(ctx, &neo.FunctionCreateParams{
        Name:        "price_alert",
        Description: "NEO price alert function",
        Runtime:     "go",
        Code: `
package main

func Handler(ctx *neo.Context, event *neo.Event) error {
    price, err := ctx.PriceFeed.GetPrice("NEO/USD")
    if err != nil {
        return err
    }
    
    if price.GreaterThan(decimal.NewFromInt(100)) {
        return ctx.Notify("NEO price alert", fmt.Sprintf("Price is %s", price))
    }
    
    return nil
}
        `,
        Environment: map[string]string{
            "ALERT_THRESHOLD": "100",
        },
    })
    if err != nil {
        log.Fatal(err)
    }
    
    log.Printf("Created function: %+v\n", func)
    
    // Example: Create a trigger
    trigger, err := client.Triggers.Create(ctx, &neo.TriggerCreateParams{
        Name:       "daily_price_check",
        FunctionID: "func_123",
        Type:       "schedule",
        Schedule:   "0 0 * * *",
    })
    if err != nil {
        log.Fatal(err)
    }
    
    log.Printf("Created trigger: %+v\n", trigger)
    
    // Example: Monitor gas balance
    balance, err := client.Gas.GetBalance(ctx)
    if err != nil {
        log.Fatal(err)
    }
    
    log.Printf("Gas balance: %+v\n", balance)
    
    // Request more gas if needed
    if balance.Available.LessThan(decimal.NewFromInt(100000)) {
        _, err := client.Gas.Request(ctx, &neo.GasRequestParams{
            Amount: "100000",
        })
        if err != nil {
            log.Fatal(err)
        }
    }
    
    // Example: WebSocket integration
    ws := client.NewWebSocket()
    
    ws.Subscribe("prices", &neo.SubscribeParams{
        Symbols: []string{"NEO/USD"},
    })
    
    ws.On("prices", func(data interface{}) {
        log.Printf("Price update: %+v\n", data)
    })
    
    // Start WebSocket connection
    if err := ws.Connect(); err != nil {
        log.Fatal(err)
    }
    defer ws.Close()
    
    // Keep the connection alive
    select {}
}
```

### Advanced Features

#### 1. Custom HTTP Client

```go
client, err := neo.NewClient(
    neo.WithHTTPClient(&http.Client{
        Timeout: time.Second * 30,
        Transport: &http.Transport{
            MaxIdleConns:        100,
            MaxIdleConnsPerHost: 100,
            IdleConnTimeout:     90 * time.Second,
        },
    }),
)
```

#### 2. Retry Configuration

```go
client, err := neo.NewClient(
    neo.WithRetry(&neo.RetryConfig{
        MaxAttempts:      3,
        InitialInterval:  time.Second,
        MaxInterval:      time.Second * 5,
        BackoffMultiplier: 2,
    }),
)
```

## Best Practices

1. **Error Handling**
   - Always check for errors
   - Use specific error types for better handling
   - Implement proper logging and monitoring

2. **Resource Management**
   - Close WebSocket connections when done
   - Use context for cancellation
   - Clean up resources properly

3. **Configuration**
   - Use environment variables for sensitive data
   - Implement proper configuration management
   - Follow security best practices

4. **Performance**
   - Reuse client instances
   - Implement proper caching
   - Use batch operations when possible

5. **Testing**
   - Write unit tests
   - Use mocks for testing
   - Implement integration tests

## Support

For SDK support:
- Email: api@neo-service-layer.io
- Discord: [Neo Service Layer Community](https://discord.gg/neo-service-layer)
- GitHub Issues: [Report a bug](https://github.com/will/neo_service_layer/issues) 