# Trigger Evaluator

The Trigger Evaluator is responsible for evaluating conditions against event contexts to determine if a trigger should be executed. It provides a flexible and powerful expression evaluation system using the [expr](https://github.com/expr-lang/expr) library.

## Features

- Expression-based condition evaluation
- Support for comparison operators
- Multiple condition evaluation with AND/OR logic
- Context-aware evaluation
- Detailed error reporting

## Usage

### Basic Evaluation

The evaluator can evaluate any valid expression against a context:

```go
evaluator := NewEvaluator(logger)
result, err := evaluator.Evaluate("event.value > 10", context)
```

### Comparison Operations

The evaluator supports the following comparison operators:

- `eq` or `==`: Equal to
- `ne` or `!=`: Not equal to
- `gt` or `>`: Greater than
- `gte` or `>=`: Greater than or equal to
- `lt` or `<`: Less than
- `lte` or `<=`: Less than or equal to
- `contains`: String contains
- `startsWith`: String starts with
- `endsWith`: String ends with

Example:
```go
result, err := evaluator.EvaluateComparison("value", "gt", 10, context)
```

### Multiple Conditions

Multiple conditions can be evaluated with AND/OR logic:

```go
conditions := []string{
    "event.value > 10",
    "event.name == 'test'",
    "event.active == true",
}
result, err := evaluator.EvaluateMultiple(conditions, "and", context)
```

## Context Structure

The context passed to the evaluator should be a map with the following structure:

```go
context := map[string]interface{}{
    "event": map[string]interface{}{
        "value": 15,
        "name": "test",
        "active": true,
        // ... other event fields
    },
    // ... other context fields
}
```

## Error Handling

The evaluator provides detailed error messages for:
- Invalid expressions
- Compilation errors
- Runtime errors
- Unsupported operators
- Type conversion errors

## Best Practices

1. **Validate Expressions**: Always validate expressions before using them in production.
2. **Handle Errors**: Always check for errors when evaluating conditions.
3. **Use Type Safety**: Ensure the context contains the expected types for comparison.
4. **Log Results**: Use the debug logs to track evaluation results for troubleshooting.
5. **Keep Expressions Simple**: Break complex conditions into multiple simpler ones.

## Examples

### Simple Value Comparison
```go
context := map[string]interface{}{
    "event": map[string]interface{}{
        "value": 15,
    },
}
result, err := evaluator.Evaluate("event.value > 10", context)
```

### String Operations
```go
context := map[string]interface{}{
    "event": map[string]interface{}{
        "name": "test_event",
    },
}
result, err := evaluator.EvaluateComparison("name", "contains", "test", context)
```

### Multiple Conditions
```go
conditions := []string{
    "event.value > 10",
    "event.name == 'test'",
}
result, err := evaluator.EvaluateMultiple(conditions, "and", context)
```

## Integration with Trigger Service

The evaluator is used by the Trigger Service to:
1. Validate trigger conditions during creation
2. Evaluate conditions when events are received
3. Filter events based on condition results
4. Determine if a trigger should be executed

## Testing

The evaluator includes comprehensive tests covering:
- Basic expression evaluation
- Comparison operations
- Multiple condition evaluation
- Error cases
- Edge cases

Run the tests using:
```bash
go test -v ./pkg/trigger/...
``` 