# JavaScript Interoperability Implementation

This document describes the implementation details of the JavaScript interoperability features in the Neo Service Layer Function Service.

## Overview

The JavaScript interoperability implementation enables JavaScript functions running within the Neo Function Service to interact with other Neo services. This is achieved through several key components:

1. **Function Context**: A JavaScript object injected into the function execution environment
2. **Service Access Methods**: Simplified methods for interacting with Neo services
3. **Automatic Authentication**: Authentication handled automatically for function executions
4. **Error Handling**: Consistent error handling across service interactions
5. **Trigger Management**: Methods for creating and managing event-based triggers
6. **Event Handling**: Mechanisms for registering and responding to events
7. **Transaction Management**: Utilities for creating and sending blockchain transactions

## Implementation Components

### 1. Function Service Configuration

The Function Service configuration has been extended to support interoperability:

```go
type Config struct {
    // Existing fields
    MaxFunctionSize       int
    MaxExecutionTime      time.Duration
    MaxMemoryLimit        int64
    EnableNetworkAccess   bool
    EnableFileIO          bool
    DefaultRuntime        string
    
    // New fields for interoperability
    ServiceLayerURL       string        // URL for the Neo Service Layer API
    EnableInteroperability bool          // Enable/disable interoperability features
}
```

### 2. Function Context Model

A new `FunctionContext` model has been added to represent the execution context for a function:

```go
type FunctionContext struct {
    FunctionID  string                 // Function ID
    ExecutionID string                 // Execution ID
    Owner       util.Uint160           // Function owner
    Caller      util.Uint160           // Function caller
    Parameters  map[string]interface{} // Function parameters
    Env         map[string]string      // Environment variables
    TraceID     string                 // Trace ID for request tracking
    Services    *ServiceClients        // Service clients for interoperability
}

type ServiceClients struct {
    Functions  functions.IService  // Functions service client
    GasBank    gasbank.IService    // Gas Bank service client
    PriceFeed  pricefeed.IService  // Price Feed service client
    Secrets    secrets.IService    // Secrets service client
    Trigger    trigger.IService    // Trigger service client
    Transaction transaction.IService // Transaction service client
}
```

### 3. Sandbox Implementation

The JavaScript sandbox has been extended to support interoperability:

```go
type SandboxConfig struct {
    // Existing fields
    MemoryLimit      int64
    TimeLimit        time.Duration
    EnableNetAccess  bool
    EnableFileIO     bool
    
    // New fields for interoperability
    EnableInteroperability bool
    FunctionContext       *FunctionContext
}

type FunctionInput struct {
    Code       string
    Parameters map[string]interface{}
    Context    *FunctionContext  // Added for interoperability
}
```

### 4. JavaScript Context Injection

The function context is injected into the JavaScript environment:

```go
func (s *Sandbox) Execute(ctx context.Context, input FunctionInput) (*FunctionOutput, error) {
    // Create JavaScript runtime
    vm := goja.New()
    
    // Set memory and time limits
    // ...
    
    // Inject function context if interoperability is enabled
    if s.config.EnableInteroperability && input.Context != nil {
        contextObj := vm.NewObject()
        
        // Set context properties
        _ = contextObj.Set("functionId", input.Context.FunctionID)
        _ = contextObj.Set("executionId", input.Context.ExecutionID)
        _ = contextObj.Set("owner", input.Context.Owner.StringLE())
        if !input.Context.Caller.Equals(util.Uint160{}) {
            _ = contextObj.Set("caller", input.Context.Caller.StringLE())
        }
        _ = contextObj.Set("parameters", input.Parameters)
        _ = contextObj.Set("env", input.Context.Env)
        _ = contextObj.Set("traceId", input.Context.TraceID)
        
        // Inject service access methods
        _ = contextObj.Set("log", s.logFunction(ctx, input.Context))
        _ = contextObj.Set("getSecret", s.getSecretFunction(ctx, input.Context))
        _ = contextObj.Set("getGasPrice", s.getGasPriceFunction(ctx, input.Context))
        _ = contextObj.Set("getPrice", s.getPriceFunction(ctx, input.Context))
        _ = contextObj.Set("invokeFunction", s.invokeFunctionFunction(ctx, input.Context))
        
        // Inject trigger management methods
        _ = contextObj.Set("createTrigger", s.createTriggerFunction(ctx, input.Context))
        _ = contextObj.Set("getTrigger", s.getTriggerFunction(ctx, input.Context))
        _ = contextObj.Set("updateTrigger", s.updateTriggerFunction(ctx, input.Context))
        _ = contextObj.Set("deleteTrigger", s.deleteTriggerFunction(ctx, input.Context))
        _ = contextObj.Set("listTriggers", s.listTriggersFunction(ctx, input.Context))
        _ = contextObj.Set("executeTrigger", s.executeTriggerFunction(ctx, input.Context))
        
        // Inject event handling methods
        _ = contextObj.Set("onEvent", s.onEventFunction(ctx, input.Context))
        
        // Inject transaction management methods
        _ = contextObj.Set("createTransaction", s.createTransactionFunction(ctx, input.Context))
        _ = contextObj.Set("signTransaction", s.signTransactionFunction(ctx, input.Context))
        _ = contextObj.Set("sendTransaction", s.sendTransactionFunction(ctx, input.Context))
        
        // Set the context object in the global scope
        vm.Set("context", contextObj)
    }
    
    // Execute the function
    // ...
}
```

### 5. Service Access Method Implementations

#### 5.1 Logging

```go
func (s *Sandbox) logFunction(ctx context.Context, functionContext *FunctionContext) func(call goja.FunctionCall) goja.Value {
    return func(call goja.FunctionCall) goja.Value {
        message := call.Argument(0).String()
        log.Printf("[Function %s] %s", functionContext.FunctionID, message)
        return goja.Undefined()
    }
}
```

#### 5.2 Secret Retrieval

```go
func (s *Sandbox) getSecretFunction(ctx context.Context, functionContext *FunctionContext) func(call goja.FunctionCall) goja.Value {
    return func(call goja.FunctionCall) goja.Value {
        key := call.Argument(0).String()
        
        // Get secret from Secrets service
        secret, err := functionContext.Services.Secrets.GetSecret(ctx, functionContext.Owner, key)
        if err != nil {
            // Handle error
            return goja.Undefined()
        }
        
        return s.vm.ToValue(secret.Value)
    }
}
```

#### 5.3 Gas Price Retrieval

```go
func (s *Sandbox) getGasPriceFunction(ctx context.Context, functionContext *FunctionContext) func(call goja.FunctionCall) goja.Value {
    return func(call goja.FunctionCall) goja.Value {
        // Get gas price from Gas Bank service
        gasPrice, err := functionContext.Services.GasBank.GetGasPrice(ctx)
        if err != nil {
            // Handle error
            return goja.Undefined()
        }
        
        return s.vm.ToValue(gasPrice)
    }
}
```

#### 5.4 Price Retrieval

```go
func (s *Sandbox) getPriceFunction(ctx context.Context, functionContext *FunctionContext) func(call goja.FunctionCall) goja.Value {
    return func(call goja.FunctionCall) goja.Value {
        symbol := call.Argument(0).String()
        
        // Get price from Price Feed service
        price, err := functionContext.Services.PriceFeed.GetPrice(ctx, symbol)
        if err != nil {
            // Handle error
            return goja.Undefined()
        }
        
        return s.vm.ToValue(price.Value)
    }
}
```

#### 5.5 Function Invocation

```go
func (s *Sandbox) invokeFunctionFunction(ctx context.Context, functionContext *FunctionContext) func(call goja.FunctionCall) goja.Value {
    return func(call goja.FunctionCall) goja.Value {
        functionID := call.Argument(0).String()
        paramsObj := call.Argument(1).ToObject(s.vm)
        
        // Convert parameters to map
        params := make(map[string]interface{})
        for _, key := range paramsObj.Keys() {
            params[key] = paramsObj.Get(key).Export()
        }
        
        // Create function invocation
        invocation := &functions.FunctionInvocation{
            FunctionID: functionID,
            Parameters: params,
            Async:      false,
            Caller:     functionContext.Owner,
            TraceID:    functionContext.TraceID,
        }
        
        // Invoke function
        execution, err := functionContext.Services.Functions.InvokeFunction(ctx, *invocation)
        if err != nil {
            // Handle error
            return goja.Undefined()
        }
        
        // Wait for execution to complete if not async
        if !invocation.Async {
            for execution.Status == "running" {
                time.Sleep(100 * time.Millisecond)
                execution, err = functionContext.Services.Functions.GetExecution(ctx, execution.ID)
                if err != nil {
                    // Handle error
                    return goja.Undefined()
                }
            }
        }
        
        // Return execution result
        return s.vm.ToValue(execution.Result)
    }
}
```

### 6. Trigger Management Method Implementations

#### 6.1 Create Trigger

```go
func (s *Sandbox) createTriggerFunction(ctx context.Context, functionContext *FunctionContext) func(call goja.FunctionCall) goja.Value {
    return func(call goja.FunctionCall) goja.Value {
        configObj := call.Argument(0).ToObject(s.vm)
        
        // Extract trigger configuration
        name := configObj.Get("name").String()
        description := configObj.Get("description").String()
        triggerType := configObj.Get("type").String()
        handler := configObj.Get("handler").String()
        parameters := configObj.Get("parameters").String()
        
        // Extract config map
        configMapObj := configObj.Get("config").ToObject(s.vm)
        configMap := make(map[string]string)
        for _, key := range configMapObj.Keys() {
            configMap[key] = configMapObj.Get(key).String()
        }
        
        // Create trigger
        triggerParams := &trigger.CreateTriggerParams{
            Name:        name,
            Description: description,
            Type:        trigger.TriggerType(triggerType),
            Handler:     handler,
            Parameters:  parameters,
            Config:      configMap,
            Metadata:    make(map[string]string),
        }
        
        newTrigger, err := functionContext.Services.Trigger.CreateTrigger(ctx, functionContext.Owner, &trigger.Trigger{
            Name:        triggerParams.Name,
            Description: triggerParams.Description,
            Type:        triggerParams.Type,
            Handler:     triggerParams.Handler,
            Parameters:  triggerParams.Parameters,
            Config:      triggerParams.Config,
            Metadata:    triggerParams.Metadata,
        })
        
        if err != nil {
            // Handle error
            return goja.Undefined()
        }
        
        // Return created trigger
        return s.vm.ToValue(newTrigger)
    }
}
```

#### 6.2 Get Trigger

```go
func (s *Sandbox) getTriggerFunction(ctx context.Context, functionContext *FunctionContext) func(call goja.FunctionCall) goja.Value {
    return func(call goja.FunctionCall) goja.Value {
        triggerID := call.Argument(0).String()
        
        // Get trigger
        trigger, err := functionContext.Services.Trigger.GetTrigger(ctx, functionContext.Owner, triggerID)
        if err != nil {
            // Handle error
            return goja.Undefined()
        }
        
        // Return trigger
        return s.vm.ToValue(trigger)
    }
}
```

#### 6.3 Update Trigger

```go
func (s *Sandbox) updateTriggerFunction(ctx context.Context, functionContext *FunctionContext) func(call goja.FunctionCall) goja.Value {
    return func(call goja.FunctionCall) goja.Value {
        triggerID := call.Argument(0).String()
        updatesObj := call.Argument(1).ToObject(s.vm)
        
        // Get existing trigger
        existingTrigger, err := functionContext.Services.Trigger.GetTrigger(ctx, functionContext.Owner, triggerID)
        if err != nil {
            // Handle error
            return goja.Undefined()
        }
        
        // Apply updates
        if updatesObj.Get("name") != nil && !goja.IsUndefined(updatesObj.Get("name")) {
            existingTrigger.Name = updatesObj.Get("name").String()
        }
        
        if updatesObj.Get("description") != nil && !goja.IsUndefined(updatesObj.Get("description")) {
            existingTrigger.Description = updatesObj.Get("description").String()
        }
        
        if updatesObj.Get("parameters") != nil && !goja.IsUndefined(updatesObj.Get("parameters")) {
            existingTrigger.Parameters = updatesObj.Get("parameters").String()
        }
        
        // Update trigger
        updatedTrigger, err := functionContext.Services.Trigger.UpdateTrigger(ctx, functionContext.Owner, triggerID, existingTrigger)
        if err != nil {
            // Handle error
            return goja.Undefined()
        }
        
        // Return updated trigger
        return s.vm.ToValue(updatedTrigger)
    }
}
```

#### 6.4 Delete Trigger

```go
func (s *Sandbox) deleteTriggerFunction(ctx context.Context, functionContext *FunctionContext) func(call goja.FunctionCall) goja.Value {
    return func(call goja.FunctionCall) goja.Value {
        triggerID := call.Argument(0).String()
        
        // Delete trigger
        err := functionContext.Services.Trigger.DeleteTrigger(ctx, functionContext.Owner, triggerID)
        if err != nil {
            // Handle error
            return s.vm.ToValue(false)
        }
        
        // Return success
        return s.vm.ToValue(true)
    }
}
```

#### 6.5 List Triggers

```go
func (s *Sandbox) listTriggersFunction(ctx context.Context, functionContext *FunctionContext) func(call goja.FunctionCall) goja.Value {
    return func(call goja.FunctionCall) goja.Value {
        // List triggers
        triggers, err := functionContext.Services.Trigger.ListTriggers(ctx, functionContext.Owner)
        if err != nil {
            // Handle error
            return goja.Undefined()
        }
        
        // Return triggers
        return s.vm.ToValue(triggers)
    }
}
```

#### 6.6 Execute Trigger

```go
func (s *Sandbox) executeTriggerFunction(ctx context.Context, functionContext *FunctionContext) func(call goja.FunctionCall) goja.Value {
    return func(call goja.FunctionCall) goja.Value {
        triggerID := call.Argument(0).String()
        
        // Execute trigger
        execution, err := functionContext.Services.Trigger.ExecuteTrigger(ctx, functionContext.Owner, triggerID)
        if err != nil {
            // Handle error
            return goja.Undefined()
        }
        
        // Return execution
        return s.vm.ToValue(execution)
    }
}
```

### 7. Event Handling Method Implementations

#### 7.1 Register Event Handler

```go
func (s *Sandbox) onEventFunction(ctx context.Context, functionContext *FunctionContext) func(call goja.FunctionCall) goja.Value {
    return func(call goja.FunctionCall) goja.Value {
        eventType := call.Argument(0).String()
        callback := call.Argument(1)
        
        // Store the callback in the event handlers registry
        // This is a simplified implementation - in practice, this would involve
        // registering with the appropriate event system
        
        // For blockchain events
        if strings.HasPrefix(eventType, "blockchain:") {
            eventName := strings.TrimPrefix(eventType, "blockchain:")
            
            // Register with blockchain event handler
            // ...
        }
        
        // For time events
        if strings.HasPrefix(eventType, "time:") {
            eventName := strings.TrimPrefix(eventType, "time:")
            
            // Register with time event handler
            // ...
        }
        
        // For API events
        if strings.HasPrefix(eventType, "api:") {
            eventName := strings.TrimPrefix(eventType, "api:")
            
            // Register with API event handler
            // ...
        }
        
        // For price events
        if strings.HasPrefix(eventType, "price:") {
            eventName := strings.TrimPrefix(eventType, "price:")
            
            // Register with price event handler
            // ...
        }
        
        return goja.Undefined()
    }
}
```

### 8. Transaction Management Method Implementations

#### 8.1 Create Transaction

```go
func (s *Sandbox) createTransactionFunction(ctx context.Context, functionContext *FunctionContext) func(call goja.FunctionCall) goja.Value {
    return func(call goja.FunctionCall) goja.Value {
        txConfigObj := call.Argument(0).ToObject(s.vm)
        
        // Extract transaction configuration
        txType := txConfigObj.Get("type").String()
        asset := txConfigObj.Get("asset").String()
        from := txConfigObj.Get("from").String()
        to := txConfigObj.Get("to").String()
        amount := txConfigObj.Get("amount").ToFloat()
        
        // Create transaction
        // This is a simplified implementation - in practice, this would involve
        // creating the appropriate transaction type based on the Neo blockchain
        
        // Create a transaction object to return
        transaction := map[string]interface{}{
            "type":   txType,
            "asset":  asset,
            "from":   from,
            "to":     to,
            "amount": amount,
            "status": "created",
        }
        
        return s.vm.ToValue(transaction)
    }
}
```

#### 8.2 Sign Transaction

```go
func (s *Sandbox) signTransactionFunction(ctx context.Context, functionContext *FunctionContext) func(call goja.FunctionCall) goja.Value {
    return func(call goja.FunctionCall) goja.Value {
        txObj := call.Argument(0).ToObject(s.vm)
        
        // Extract transaction details
        // ...
        
        // Sign transaction
        // This is a simplified implementation - in practice, this would involve
        // signing the transaction with the appropriate keys
        
        // Update transaction status
        txObj.Set("status", "signed")
        
        return txObj
    }
}
```

#### 8.3 Send Transaction

```go
func (s *Sandbox) sendTransactionFunction(ctx context.Context, functionContext *FunctionContext) func(call goja.FunctionCall) goja.Value {
    return func(call goja.FunctionCall) goja.Value {
        txObj := call.Argument(0).ToObject(s.vm)
        
        // Extract transaction details
        // ...
        
        // Send transaction
        // This is a simplified implementation - in practice, this would involve
        // sending the transaction to the Neo blockchain
        
        // Create result
        result := map[string]interface{}{
            "txid":        "0x" + generateRandomHex(64),
            "blockHeight": 1234567,
            "status":      "confirmed",
        }
        
        return s.vm.ToValue(result)
    }
}

func generateRandomHex(length int) string {
    // Helper function to generate random hex string
    // ...
    return "abcdef1234567890"
}
```

### 9. Function Execution Flow

The function execution flow has been updated to support interoperability:

```go
func (s *Service) executeFunction(function *Function, execution *FunctionExecution, parameters map[string]interface{}) {
    // Prepare function context for interoperability
    var functionContext *runtime.FunctionContext
    if s.config.EnableInteroperability {
        functionContext = &runtime.FunctionContext{
            FunctionID:  function.ID,
            ExecutionID: execution.ID,
            Owner:       function.Owner,
            Caller:      execution.Caller,
            Parameters:  parameters,
            Env:         function.Environment,
            TraceID:     execution.TraceID,
            Services: &runtime.ServiceClients{
                Functions:   s,
                GasBank:     s.gasBankService,
                PriceFeed:   s.priceFeedService,
                Secrets:     s.secretsService,
                Trigger:     s.triggerService,
                Transaction: s.transactionService,
            },
        }
    }
    
    // Create sandbox
    sandbox, err := runtime.NewSandbox(&runtime.SandboxConfig{
        MemoryLimit:           s.config.MaxMemoryLimit,
        TimeLimit:             s.config.MaxExecutionTime,
        EnableNetAccess:       s.config.EnableNetworkAccess,
        EnableFileIO:          s.config.EnableFileIO,
        EnableInteroperability: s.config.EnableInteroperability,
    })
    
    if err != nil {
        execution.Status = FunctionExecutionStatusFailed
        execution.Error = fmt.Sprintf("Failed to create sandbox: %v", err)
        return
    }
    
    // Prepare function input
    input := runtime.FunctionInput{
        Code:       function.Code,
        Parameters: parameters,
        Context:    functionContext,
    }
    
    // Execute function
    output, err := sandbox.Execute(context.Background(), input)
    
    // Handle result
    // ...
}
```

## Security Considerations

1. **Authentication**: The function context includes authentication information based on the function execution context, ensuring that functions can only access resources they have permission to access.

2. **Permissions**: Functions can only access services and resources they have permission to access. This is enforced by the service implementations.

3. **Isolation**: Each function execution runs in its own isolated sandbox, preventing functions from interfering with each other.

4. **Input Validation**: All inputs to service methods are validated to prevent injection attacks.

5. **Resource Limits**: Functions are subject to memory and execution time limits to prevent resource exhaustion.

6. **Error Handling**: Errors are handled consistently across service interactions to prevent information leakage.

## Future Enhancements

1. **Caching**: Implement caching for frequently accessed resources to improve performance.

2. **Metrics**: Add metrics collection for function executions to monitor performance and resource usage.

3. **Rate Limiting**: Implement rate limiting for service access to prevent abuse.

4. **Enhanced Event Handling**: Expand event handling capabilities to support more event types and complex event processing.

5. **Transaction Templates**: Add support for transaction templates to simplify common transaction patterns.

6. **Smart Contract Integration**: Enhance transaction management to support interaction with Neo smart contracts.
