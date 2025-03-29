# Trigger Service

The Trigger Service is a component of the Neo Service Layer that enables event-driven automation by monitoring blockchain events and executing predefined actions when specific conditions are met.

## Features

- Monitor blockchain events from smart contracts
- Define triggers with custom conditions and actions
- Support for multiple action types:
  - Contract calls
  - Webhooks
  - Notifications
- Flexible condition evaluation with support for:
  - Comparison operators (equals, greater than, less than, etc.)
  - String operations (contains, not contains)
  - Array operations (in, not in)
  - Nested field access using dot notation

## Configuration

The trigger service requires the following configuration:

```go
type Config struct {
    // Contract configuration
    ContractHash     util.Uint160  // Hash of the contract to monitor
    ContractMethod   string        // Method to call for contract actions

    // Network configuration
    RPCEndpoint     string        // Neo RPC endpoint URL

    // Trigger configuration
    MaxTriggers            int           // Maximum number of triggers allowed
    MaxEventsPerTrigger    int           // Maximum number of events per trigger
    EventPollingInterval   time.Duration // Interval between event polling
    EventRetentionPeriod   time.Duration // How long to retain event history
}
```

## Usage

### Creating a Trigger Service

```go
// Create configuration
config := trigger.Config{
    ContractHash:         contractHash,
    ContractMethod:       "update",
    RPCEndpoint:         "http://localhost:10332",
    MaxTriggers:         100,
    MaxEventsPerTrigger: 1000,
    EventPollingInterval: time.Second * 15,
    EventRetentionPeriod: time.Hour * 24,
}

// Create service
service, err := trigger.NewService(config, wallet)
if err != nil {
    log.Fatal(err)
}

// Start service
if err := service.Start(context.Background()); err != nil {
    log.Fatal(err)
}
```

### Creating a Trigger

```go
// Create trigger
trigger := &trigger.Trigger{
    ID:           "price-alert",
    Name:         "Price Alert",
    Description:  "Trigger when price exceeds threshold",
    ContractHash: contractHash,
    EventName:    "PriceUpdate",
    Condition:    `{"field": "price", "operator": "gt", "value": 100}`,
    Action:       `{"type": "webhook", "parameters": {"url": "https://api.example.com/notify"}}`,
}

// Add trigger to service
if err := service.CreateTrigger(trigger); err != nil {
    log.Fatal(err)
}
```

### Action Types

#### Contract Call

Executes a smart contract method:

```json
{
    "type": "contract_call",
    "parameters": {
        "contract_hash": "0x1234...",
        "method": "transfer",
        "parameters": ["arg1", "arg2"]
    }
}
```

#### Webhook

Sends an HTTP POST request:

```json
{
    "type": "webhook",
    "parameters": {
        "url": "https://api.example.com/webhook",
        "headers": {
            "Authorization": "Bearer token"
        }
    }
}
```

#### Notification

Sends a notification:

```json
{
    "type": "notify",
    "parameters": {
        "type": "email",
        "message": "Price threshold exceeded!"
    }
}
```

### Condition Operators

The following operators are supported for condition evaluation:

- `eq`: Equals
- `neq`: Not equals
- `gt`: Greater than
- `gte`: Greater than or equal
- `lt`: Less than
- `lte`: Less than or equal
- `contains`: String contains
- `not_contains`: String does not contain
- `in`: Value in array
- `not_in`: Value not in array

Example condition:

```json
{
    "field": "data.price",
    "operator": "gt",
    "value": 100
}
```

## Error Handling

The service defines various error types for different scenarios:

- Service errors (e.g., `ErrServiceAlreadyRunning`)
- Trigger errors (e.g., `ErrTriggerNotFound`, `ErrMaxTriggersReached`)
- Configuration errors (e.g., `ErrInvalidContractHash`)
- Event errors (e.g., `ErrEventNotFound`, `ErrEventProcessingFailed`)

## Best Practices

1. **Condition Complexity**: Keep conditions simple and focused. Complex conditions can be split into multiple triggers.

2. **Action Timeout**: Actions should complete quickly. Long-running actions should be handled asynchronously.

3. **Error Handling**: Implement proper error handling and logging for actions, especially for webhooks and contract calls.

4. **Resource Management**: Monitor trigger count and event volume to prevent resource exhaustion.

5. **Testing**: Test triggers with various conditions and actions before deploying to production.

## Limitations

- Maximum number of triggers is configurable but should be set based on available resources
- Event polling interval should be balanced between responsiveness and RPC load
- Webhook timeouts are fixed at 10 seconds
- Complex condition evaluation is not supported (only single conditions)
- Notification types are limited to basic implementations 