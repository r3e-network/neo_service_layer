# Trigger Service Documentation

## Overview

The Trigger Service is a critical component of the Neo Service Layer that enables event-driven automation. It allows users to define conditions that, when met, automatically execute predefined actions. This capability is specifically designed for the Neo N3 blockchain ecosystem.

## Key Features

- **Event-based Triggers**: Execute actions when specific blockchain events occur
- **Schedule-based Triggers**: Execute actions on a time schedule (CRON expressions)
- **Contract Automation**: Automatically invoke smart contract methods when conditions are met
- **Custom Conditions**: Support for complex condition logic using an expression language
- **Execution History**: Track all trigger executions and their outcomes
- **Rate Limiting**: Controls to prevent excessive executions

## Service Architecture

The Trigger Service consists of several components:

1. **Manager**: Core component responsible for creating, updating, retrieving, and executing triggers
2. **Store**: Persistent storage for triggers and their execution history
3. **Scheduler**: Handles time-based triggers and scheduling logic
4. **Evaluator**: Evaluates trigger conditions to determine if actions should be executed
5. **Executor**: Executes the defined actions when conditions are met
6. **Metrics Collector**: Gathers performance and usage metrics for triggers

## Data Models

### Trigger

```go
type Trigger struct {
    ID        string                 // Unique identifier
    Type      string                 // Type of trigger (event, schedule, etc.)
    Condition string                 // Condition for trigger activation
    Action    string                 // Action to perform when triggered
    Owner     util.Uint160           // Trigger owner address
    CreatedAt time.Time              // Creation timestamp
    UpdatedAt time.Time              // Last update timestamp
    Active    bool                   // Whether the trigger is active
    Metadata  map[string]interface{} // Additional metadata
}
```

### TriggerExecution

```go
type TriggerExecution struct {
    ID        string      // Unique identifier
    TriggerID string      // ID of the trigger
    Status    string      // Execution status (pending, completed, failed)
    StartTime time.Time   // When execution started
    EndTime   time.Time   // When execution ended
    Duration  int64       // Execution duration in milliseconds
    Result    interface{} // Execution result
    Error     string      // Error message if execution failed
}
```

## API Endpoints

The Trigger Service exposes the following API endpoints through the API Service:

### Create Trigger

```
POST /api/v1/triggers
```

Creates a new trigger for the authenticated user.

**Request Body:**
```json
{
  "type": "schedule",
  "condition": "0 */5 * * * *",
  "action": "invokeFunction('function-id', { param1: 'value1' })",
  "metadata": {
    "description": "Run function every 5 minutes"
  }
}
```

**Response:**
```json
{
  "id": "trigger-123",
  "type": "schedule",
  "condition": "0 */5 * * * *",
  "action": "invokeFunction('function-id', { param1: 'value1' })",
  "owner": "0x1234567890abcdef1234567890abcdef12345678",
  "createdAt": "2023-03-25T12:00:00Z",
  "updatedAt": "2023-03-25T12:00:00Z",
  "active": true,
  "metadata": {
    "description": "Run function every 5 minutes"
  }
}
```

### Get Trigger

```
GET /api/v1/triggers/{id}
```

Retrieves a specific trigger by ID.

**Response:**
```json
{
  "id": "trigger-123",
  "type": "schedule",
  "condition": "0 */5 * * * *",
  "action": "invokeFunction('function-id', { param1: 'value1' })",
  "owner": "0x1234567890abcdef1234567890abcdef12345678",
  "createdAt": "2023-03-25T12:00:00Z",
  "updatedAt": "2023-03-25T12:00:00Z",
  "active": true,
  "metadata": {
    "description": "Run function every 5 minutes"
  }
}
```

### List Triggers

```
GET /api/v1/triggers
```

Lists all triggers for the authenticated user.

**Response:**
```json
[
  {
    "id": "trigger-123",
    "type": "schedule",
    "condition": "0 */5 * * * *",
    "action": "invokeFunction('function-id', { param1: 'value1' })",
    "owner": "0x1234567890abcdef1234567890abcdef12345678",
    "createdAt": "2023-03-25T12:00:00Z",
    "updatedAt": "2023-03-25T12:00:00Z",
    "active": true,
    "metadata": {
      "description": "Run function every 5 minutes"
    }
  },
  {
    "id": "trigger-456",
    "type": "event",
    "condition": "event.type == 'transfer' && event.amount > 100",
    "action": "invokeContract('0x1234567890abcdef', 'process', [event.txid])",
    "owner": "0x1234567890abcdef1234567890abcdef12345678",
    "createdAt": "2023-03-24T10:00:00Z",
    "updatedAt": "2023-03-24T10:00:00Z",
    "active": true,
    "metadata": {
      "description": "Process large transfers"
    }
  }
]
```

### Update Trigger

```
PUT /api/v1/triggers/{id}
```

Updates an existing trigger.

**Request Body:**
```json
{
  "condition": "0 0 */1 * * *",
  "action": "invokeFunction('function-id', { param1: 'new-value' })",
  "active": false,
  "metadata": {
    "description": "Run function hourly"
  }
}
```

**Response:**
```json
{
  "id": "trigger-123",
  "type": "schedule",
  "condition": "0 0 */1 * * *",
  "action": "invokeFunction('function-id', { param1: 'new-value' })",
  "owner": "0x1234567890abcdef1234567890abcdef12345678",
  "createdAt": "2023-03-25T12:00:00Z",
  "updatedAt": "2023-03-25T14:30:00Z",
  "active": false,
  "metadata": {
    "description": "Run function hourly"
  }
}
```

### Delete Trigger

```
DELETE /api/v1/triggers/{id}
```

Deletes a trigger.

**Response:**
```
204 No Content
```

### Execute Trigger

```
POST /api/v1/triggers/{id}/execute
```

Manually executes a trigger.

**Response:**
```json
{
  "id": "execution-789",
  "triggerId": "trigger-123",
  "status": "completed",
  "startTime": "2023-03-25T15:00:00Z",
  "endTime": "2023-03-25T15:00:01Z",
  "duration": 1000,
  "result": {
    "success": true,
    "data": {
      "message": "Function executed successfully"
    }
  },
  "error": ""
}
```

### Get Trigger Executions

```
GET /api/v1/triggers/{id}/executions
```

Retrieves execution history for a trigger.

**Response:**
```json
[
  {
    "id": "execution-789",
    "triggerId": "trigger-123",
    "status": "completed",
    "startTime": "2023-03-25T15:00:00Z",
    "endTime": "2023-03-25T15:00:01Z",
    "duration": 1000,
    "result": {
      "success": true,
      "data": {
        "message": "Function executed successfully"
      }
    },
    "error": ""
  },
  {
    "id": "execution-456",
    "triggerId": "trigger-123",
    "status": "failed",
    "startTime": "2023-03-25T14:00:00Z",
    "endTime": "2023-03-25T14:00:02Z",
    "duration": 2000,
    "result": null,
    "error": "Function execution timed out"
  }
]
```

## Trigger Types

### Schedule Triggers

Schedule triggers use CRON expressions to define when they should execute:

```json
{
  "type": "schedule",
  "condition": "0 */15 * * * *",  // Every 15 minutes
  "action": "invokeFunction('function-id')"
}
```

### Event Triggers

Event triggers execute when specific blockchain events occur:

```json
{
  "type": "event",
  "condition": "event.contract == '0x1234567890abcdef' && event.name == 'Transfer'",
  "action": "invokeFunction('process-transfer', { txid: event.txid })"
}
```

### Contract State Triggers

Contract state triggers monitor contract storage for specific conditions:

```json
{
  "type": "state",
  "condition": "contract('0x1234567890abcdef').balance > 1000",
  "action": "invokeContract('0x1234567890abcdef', 'distribute', [])"
}
```

## Integration with Other Services

The Trigger Service integrates with several other services in the Neo Service Layer:

- **Functions Service**: Executes serverless functions when triggers are activated
- **Gas Bank Service**: Allocates GAS for trigger executions
- **API Service**: Exposes REST endpoints for trigger management
- **Metrics Service**: Collects performance metrics for trigger executions

## Configuration Options

The Trigger Service can be configured with the following options:

```go
type Config struct {
    MaxTriggers      int           // Maximum triggers per user
    MaxExecutions    int           // Maximum stored executions per trigger
    ExecutionWindow  time.Duration // Time window for rate limiting
    MaxExecutionRate int           // Maximum executions per window
}
```

## Deployment Recommendations

For production deployments:

1. Use a distributed scheduler for high availability
2. Implement a persistent storage backend for trigger data
3. Configure appropriate rate limits to prevent resource exhaustion
4. Monitor trigger execution metrics for performance issues
5. Set up alerting for failed trigger executions

## Security Considerations

- Triggers execute with the permissions of the user who created them
- Authentication and authorization are enforced for all trigger operations
- Resource limits prevent abuse of the system
- Access to sensitive trigger data is restricted to the owner 