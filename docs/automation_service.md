# Automation Service

## Overview

The Automation Service provides contract automation capabilities similar to Chainlink Keeper for the Neo N3 blockchain. It enables automated execution of smart contract functions based on time schedules or custom conditions. The service integrates with other Neo Service Layer components to offer a complete automation solution with secure, reliable, and gas-efficient contract invocations.

## Key Features

- **Scheduled Execution**: Run contract functions on regular time schedules using CRON syntax
- **Condition-Based Automation**: Trigger contract calls when specific conditions are met
- **TEE Function Integration**: Execute secure trusted execution environment (TEE) functions before contract calls
- **Gas Management**: Automatic allocation and usage tracking of gas for contract calls
- **Retry Mechanisms**: Configurable retry logic for handling temporary failures
- **Result Tracking**: Historical storage of execution results for auditing and debugging
- **Concurrency Control**: Limit parallel executions to ensure system stability

## Service Architecture

The Automation Service consists of several key components:

1. **Task Manager**: Handles registration, retrieval, and management of automation tasks
2. **Scheduler**: Manages time-based scheduling of tasks using CRON expressions
3. **Condition Evaluator**: Interfaces with the Trigger Service to evaluate custom conditions
4. **Execution Engine**: Handles the actual execution of contract calls, including TEE functions
5. **Result Recorder**: Stores and provides access to task execution results

## Configuration

The Automation Service can be configured with the following parameters:

| Parameter | Description | Default |
|-----------|-------------|---------|
| `CheckInterval` | How frequently to check conditions (for non-scheduled tasks) | 30 seconds |
| `MaxConcurrentChecks` | Maximum number of concurrent condition checks | 10 |
| `DefaultGasLimit` | Default gas limit for contract calls if not specified | 1 GAS (10^8 units) |
| `RetryInterval` | How long to wait between retries | 5 seconds |
| `MaxRetries` | Maximum number of retry attempts | 3 |

## Task Types

### Scheduled Tasks

Scheduled tasks run at specific times using CRON expressions:

```go
task := &automation.Task{
    ID:          "daily-update",
    Name:        "Daily Price Update",
    Contract:    "0x1234567890abcdef1234567890abcdef12345678",
    Method:      "updatePrices",
    Parameters:  []interface{}{"BTC", "ETH", "NEO"},
    Schedule:    "0 0 * * *",  // Daily at midnight
    GasLimit:    200000,
    UserID:      "user-123",
}
```

### Condition-Based Tasks

Condition-based tasks run when specific conditions are met:

```go
task := &automation.Task{
    ID:            "price-trigger",
    Name:          "Price-Based Swap",
    Contract:      "0xabcdef1234567890abcdef1234567890abcdef12",
    Method:        "executeSwap",
    Parameters:    []interface{}{"1000", "BTC", "NEO"},
    ConditionType: "price",
    ConditionConfig: map[string]interface{}{
        "asset":      "BTC",
        "threshold":  "50000",
        "comparison": "above",
    },
    GasLimit:      150000,
    UserID:        "user-123",
}
```

### Function-Integrated Tasks

Tasks can first execute a secure TEE function and then call a contract:

```go
task := &automation.Task{
    ID:            "function-contract",
    Name:          "Secure Oracle Update",
    Contract:      "0x7890abcdef1234567890abcdef1234567890abcd",
    Method:        "submitData",
    Parameters:    []interface{}{},
    ConditionType: "time",
    ConditionConfig: map[string]interface{}{
        "interval": "daily",
        "time":     "00:00",
    },
    GasLimit:      300000,
    UserID:        "user-123",
    FunctionID:    "function-123",  // TEE function to execute first
}
```

## Condition Types

The Automation Service supports various condition types through the Trigger Service:

1. **Price Conditions**: Trigger based on asset price thresholds
2. **Time Conditions**: Trigger at specific times or intervals
3. **Contract State Conditions**: Trigger based on contract storage values
4. **Block Conditions**: Trigger based on block height or time
5. **Transaction Conditions**: Trigger based on specific transactions
6. **Custom Conditions**: Support for custom condition evaluators

## Gas Management

The Automation Service interacts with the Gas Bank Service to handle gas for contract calls:

1. **Allocation**: Gas is allocated before contract execution
2. **Usage Recording**: Actual gas usage is recorded after successful execution
3. **Release**: Unused gas is released back to the user's balance if execution fails
4. **Limits**: Users can set gas limits per task to control costs

## Integration with Other Services

### Functions Service

The Automation Service can call secure TEE functions before contract execution, enabling:
- Secure data processing before on-chain operations
- Private computation with sensitive data
- Complex calculations that would be expensive on-chain
- Data aggregation from multiple sources

### Gas Bank Service

Integration with the Gas Bank Service enables:
- Automatic gas allocation for tasks
- Usage tracking and reporting
- Efficient gas management across multiple tasks

### Trigger Service

The Trigger Service provides:
- Condition evaluation logic
- External data access for conditions
- Standardized condition definitions
- Reusable condition components

## Security Considerations

1. **Authorization**: Only the task creator or authorized users can update or delete tasks
2. **Gas Limits**: Maximum gas limits prevent excessive gas usage
3. **Concurrency Limits**: Prevent system overload from too many concurrent executions
4. **Error Handling**: Graceful handling of failures to avoid unexpected behavior
5. **Idempotency**: Tasks are designed to be safely retriable without side effects

## Example Use Cases

### DeFi Automation

- Rebalance portfolios based on price movements
- Execute limit orders when price conditions are met
- Harvest yield farming rewards on a schedule
- Repay loans before liquidation thresholds are reached

### Oracle Services

- Update price feeds at regular intervals
- Push data from off-chain sources to on-chain contracts
- Aggregate data from multiple sources securely in TEE

### Governance

- Execute governance proposals after voting periods end
- Distribute rewards on a schedule
- Update protocol parameters based on on-chain metrics

### Gaming & NFTs

- Update game state at regular intervals
- Distribute in-game rewards on a schedule
- Generate random numbers securely for gameplay

## API Reference

### Task Management

```go
// Register a new automation task
RegisterTask(task *Task) error

// Update an existing task
UpdateTask(task *Task) error

// Delete a task
DeleteTask(taskID string) error

// Get a task by ID
GetTask(taskID string) (*Task, error)

// List all tasks
ListTasks() []*Task

// Force immediate execution of a task
ForceRunTask(taskID string) error

// Get execution results for a task
GetTaskResults(taskID string) ([]TaskResult, error)
```

## Best Practices

1. **Start Small**: Begin with simple, well-tested automation tasks before implementing complex logic
2. **Monitor Results**: Regularly check task execution results for failures or unexpected behavior
3. **Gas Efficiency**: Optimize contract functions to minimize gas usage
4. **Retry Logic**: Implement appropriate retry logic in your contract functions to handle edge cases
5. **Time Buffer**: For time-sensitive operations, schedule tasks with a buffer time
6. **Idempotency**: Design contract functions to be idempotent (can be called multiple times without side effects)
7. **Conditions**: For condition-based tasks, ensure conditions are clearly defined and testable
8. **Fallbacks**: Implement manual fallback methods for critical operations

## Troubleshooting

### Common Issues

1. **Task Not Executing**: Check condition logic, gas balance, or contract validity
2. **High Gas Usage**: Optimize contract function or adjust gas limit
3. **Frequent Failures**: Check contract logic or external dependencies
4. **Condition Never Met**: Verify condition configuration and evaluation logic

### Debugging Tools

1. **Task Results**: Check execution history for error details
2. **Logs**: Service logs provide detailed execution flow information
3. **Metrics**: Monitor execution success rates and gas usage patterns

## Future Enhancements

1. **Advanced Conditions**: Support for more complex condition combinations
2. **Webhooks**: Notification of task execution results
3. **Time Window Execution**: Specify time windows for condition-based execution
4. **Priority Levels**: Specify task priorities for execution ordering
5. **Batch Execution**: Group related tasks for more efficient execution
6. **User Interfaces**: Graphical interfaces for task management and monitoring