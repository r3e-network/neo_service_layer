# Automation Service: Implementation

*Last Updated: 2025-04-14*

This document provides detailed information about the implementation of the Automation Service, including its architecture, components, workflows, integration points, and deployment guidance.

## System Overview

The Automation Service enables users to create and manage automated jobs that execute actions in response to blockchain triggers. The service continuously monitors for trigger conditions and executes the configured actions when those conditions are met.

### System Workflow

```
┌────────────────┐     ┌────────────────┐     ┌────────────────┐
│                │     │                │     │                │
│   Monitoring   │────▶│   Evaluation   │────▶│   Execution    │
│                │     │                │     │                │
└────────────────┘     └────────────────┘     └────────────────┘
        ▲                                             │
        │                                             │
        └─────────────────────────────────────────────┘
                       Continuous Cycle
```

### Interaction with External Services

```
                           ┌────────────────────┐
                           │                    │
                           │   Neo N3 Blockchain│
                           │                    │
                           └─────────^──────────┘
                                     │
                                     │ Monitor Events
                                     │ & Blocks
┌────────────────┐     ┌─────────────v──────────┐     ┌────────────────┐
│                │     │                        │     │                │
│  Gas Bank      │◄────┤  Automation Service    │────▶│  Function      │
│  Service       │     │                        │     │  Service       │
│                │     └────────────────────────┘     │                │
└────────────────┘              │      ▲              └────────────────┘
                                │      │
                               ▼       │
                          ┌────────────────┐
                          │                │
                          │   Client       │
                          │   Application  │
                          │                │
                          └────────────────┘
```

## Detailed Workflows

### Job Creation and Management Workflow

```
┌─────────┐        ┌─────────────────┐        ┌───────────────┐
│         │ Create │                 │ Store  │               │
│ Client  │───────▶│ Job Controller  │───────▶│ Job Repository│
│         │ Job    │                 │ Job    │               │
└─────────┘        └─────────────────┘        └───────────────┘
                           │
                           │ Register with
                           ▼
                   ┌───────────────┐
                   │               │
                   │ Job Scheduler │
                   │               │
                   └───────────────┘
```

1. **Client** sends a request to create a new automation job
2. **Job Controller** validates the request and creates a job entity
3. **Job Repository** stores the job details in the database
4. **Job Scheduler** registers the job for monitoring based on its trigger type

### Trigger Evaluation Workflow

```
┌────────────┐      ┌────────────────┐      ┌────────────────┐      ┌────────────┐
│            │ Poll │                │ Get  │                │      │            │
│ Block      │─────▶│ Trigger        │─────▶│ Job Repository │─────▶│ Job        │
│ Monitor    │ Jobs │ Evaluator      │ Jobs │                │      │ Evaluator  │
│            │      │                │      │                │      │            │
└────────────┘      └────────────────┘      └────────────────┘      └─────┬──────┘
                                                                           │
      ┌────────────────┐                    ┌────────────────┐             │ Evaluate
      │                │                    │                │             │ Trigger
      │ Time-based     │                    │ Event          │             │ Conditions
      │ Monitor        │                    │ Monitor        │             │
      │                │                    │                │             │
      └────────────────┘                    └────────────────┘             │
                                                                           │
                                           ┌────────────────┐              │
                                           │                │ If triggered │
                                           │ Job Executor   │◄─────────────┘
                                           │                │
                                           └────────────────┘
```

1. **Monitors** (Block, Time-based, Event) continuously check for relevant conditions
2. When conditions are met, the **Trigger Evaluator** evaluates trigger conditions for relevant jobs
3. **Job Repository** provides job details for evaluation
4. If a trigger condition is met, the **Job Executor** is notified to execute the job's action

### Job Execution Workflow

```
┌────────────┐      ┌────────────────┐      ┌────────────────┐
│            │      │                │ Call │                │
│ Job        │─────▶│ Action         │─────▶│ Function       │
│ Executor   │      │ Executor       │      │ Service        │
│            │      │                │      │                │
└────────────┘      └────────────────┘      └────────────────┘
       │                                            │
       │                                            │
       │ Record                                     │ Return
       │ Execution                                  │ Result
       ▼                                            │
┌────────────────┐                                  │
│                │                                  │
│ Execution      │◄─────────────────────────────────┘
│ Repository     │
│                │
└────────────────┘
```

1. **Job Executor** receives a notification that a job should be executed
2. **Action Executor** handles the specific action type (Function, Smart Contract, etc.)
3. For Function actions, the **Function Service** is called to execute the user's function
4. **Execution Repository** records the execution details, including result and status

## Integration Points

The Automation Service integrates with several external services:

### Neo N3 Blockchain Integration

```go
// BlockMonitor implementation for Neo N3
type NeoBlockMonitor struct {
    client neo.Client
    logger *zap.Logger
}

func (m *NeoBlockMonitor) MonitorBlocks(ctx context.Context) (<-chan BlockEvent, error) {
    events := make(chan BlockEvent)
    
    go func() {
        defer close(events)
        
        currentHeight := uint64(0)
        ticker := time.NewTicker(10 * time.Second)
        defer ticker.Stop()
        
        for {
            select {
            case <-ctx.Done():
                return
            case <-ticker.C:
                height, err := m.client.GetBlockCount()
                if err != nil {
                    m.logger.Error("Failed to get block count", zap.Error(err))
                    continue
                }
                
                if height > currentHeight {
                    for h := currentHeight + 1; h <= height; h++ {
                        block, err := m.client.GetBlockByIndex(h)
                        if err != nil {
                            m.logger.Error("Failed to get block", zap.Uint64("height", h), zap.Error(err))
                            continue
                        }
                        
                        events <- BlockEvent{
                            Height:    h,
                            Hash:      block.Hash.StringLE(),
                            Timestamp: time.Unix(int64(block.Timestamp), 0),
                        }
                    }
                    
                    currentHeight = height
                }
            }
        }
    }()
    
    return events, nil
}
```

### Function Service Integration

```go
// FunctionExecutor implementation using the Function Service
type FunctionServiceExecutor struct {
    client functionservice.Client
    logger *zap.Logger
}

func (e *FunctionServiceExecutor) ExecuteFunction(ctx context.Context, functionID string, parameters map[string]interface{}) (map[string]interface{}, error) {
    // Convert parameters to protobuf format
    protoParams, err := structpb.NewStruct(parameters)
    if err != nil {
        return nil, fmt.Errorf("failed to convert parameters: %w", err)
    }
    
    // Call Function Service
    req := &functionservice.ExecuteRequest{
        FunctionId: functionID,
        Parameters: protoParams,
    }
    
    resp, err := e.client.Execute(ctx, req)
    if err != nil {
        return nil, fmt.Errorf("function execution failed: %w", err)
    }
    
    // Convert result back to map
    result := resp.Result.AsMap()
    return result, nil
}
```

### Gas Bank Service Integration

```go
// GasBankExecutor implementation using the Gas Bank Service
type GasBankExecutor struct {
    client gasbankservice.Client
    logger *zap.Logger
}

func (e *GasBankExecutor) ExecuteTransaction(ctx context.Context, userID string, contract string, operation string, params []any) (string, error) {
    // Convert parameters to protobuf format
    protoParams, err := convertParamsToProto(params)
    if err != nil {
        return "", fmt.Errorf("failed to convert parameters: %w", err)
    }
    
    // Call Gas Bank Service
    req := &gasbankservice.InvokeContractRequest{
        UserId:       userID,
        ContractHash: contract,
        Operation:    operation,
        Parameters:   protoParams,
    }
    
    resp, err := e.client.InvokeContract(ctx, req)
    if err != nil {
        return "", fmt.Errorf("transaction execution failed: %w", err)
    }
    
    return resp.TransactionId, nil
}

func convertParamsToProto(params []any) ([]*gasbankservice.ContractParameter, error) {
    // Implementation of parameter conversion
    // ...
}
```

## Enhanced Integration Workflows

### Complete Service Integration Map

```
┌─────────────────────────────────────────────────────────────────────┐
│                                                                     │
│                        Neo N3 Blockchain                            │
│                                                                     │
└───────────────────────────────┬─────────────────────────────────────┘
                                │
                                │ RPC Calls & Events
                                ▼
┌─────────────────────────────────────────────────────────────────────┐
│                                                                     │
│                       Automation Service                            │
│                                                                     │
│  ┌────────────┐      ┌────────────┐      ┌────────────────────┐    │
│  │            │      │            │      │                    │    │
│  │  Monitor   │─────▶│  Trigger   │─────▶│  Job Executor      │    │
│  │  Service   │      │  Evaluator │      │                    │    │
│  │            │      │            │      │                    │    │
│  └────────────┘      └────────────┘      └──────────┬─────────┘    │
│                                                      │              │
└──────────────────────────────────────────────────────┼──────────────┘
                                                       │
                                                       │
                 ┌─────────────────────────────────────┼─────────────────────┐
                 │                                     │                     │
                 ▼                                     ▼                     ▼
    ┌───────────────────────┐            ┌───────────────────┐     ┌──────────────────┐
    │                       │            │                   │     │                  │
    │  Gas Bank Service     │            │ Function Service  │     │ Secrets Service  │
    │                       │            │                   │     │                  │
    └───────────────────────┘            └───────────────────┘     └──────────────────┘
             │                                     │                          │
             │                                     │                          │
             ▼                                     ▼                          ▼
    ┌───────────────────────┐            ┌───────────────────┐     ┌──────────────────┐
    │  Transaction          │            │  User-defined     │     │  Encrypted       │
    │  Execution            │            │  Functions        │     │  Secrets         │
    │                       │            │                   │     │                  │
    └───────────────────────┘            └───────────────────┘     └──────────────────┘
```

### Automation Service to Gas Bank Service Integration

```
┌───────────────────┐                 ┌───────────────────┐
│                   │                 │                   │
│  Job Executor     │                 │  Gas Bank         │
│                   │                 │  Service          │
└─────────┬─────────┘                 └─────────┬─────────┘
          │                                     │
          │ 1. Request Gas for Transaction      │
          │────────────────────────────────────▶│
          │                                     │
          │ 2. Allocate Gas                     │
          │◀────────────────────────────────────│
          │                                     │
          │ 3. Submit Transaction               │
          │────────────────────────────────────▶│
          │                                     │
          │ 4. Sign & Broadcast                 │
          │                                     │
          │ 5. Transaction Hash                 │
          │◀────────────────────────────────────│
          │                                     │
          │ 6. Poll for Confirmation            │
          │────────────────────────────────────▶│
          │                                     │
          │ 7. Confirmation Status              │
          │◀────────────────────────────────────│
          │                                     │
┌─────────┴─────────┐                 ┌─────────┴─────────┐
│                   │                 │                   │
│  Job Executor     │                 │  Gas Bank         │
│                   │                 │  Service          │
└───────────────────┘                 └───────────────────┘
```

**Implementation Example:**

```go
// GasBankClient for interacting with the Gas Bank Service
type GasBankClient struct {
    client    gasbank.Client
    logger    *zap.Logger
    accountID string
}

// RequestTransactionExecution submits a transaction to the Gas Bank
func (c *GasBankClient) RequestTransactionExecution(ctx context.Context, tx *transaction.Transaction) (string, error) {
    // Convert the transaction to the format expected by Gas Bank
    txRequest := &gasbank.TransactionRequest{
        Script:      tx.Script,
        Parameters:  tx.Parameters,
        AccountID:   c.accountID,
        Priority:    gasbank.Priority_MEDIUM,
        Description: "Automation Service Job Execution",
    }
    
    // Submit the transaction
    resp, err := c.client.SubmitTransaction(ctx, txRequest)
    if err != nil {
        c.logger.Error("Failed to submit transaction", zap.Error(err))
        return "", fmt.Errorf("failed to submit transaction: %w", err)
    }
    
    // Return the transaction hash
    return resp.TransactionHash, nil
}

// WaitForConfirmation polls the Gas Bank Service until the transaction is confirmed
func (c *GasBankClient) WaitForConfirmation(ctx context.Context, txHash string, timeout time.Duration) (*gasbank.TransactionStatus, error) {
    ctx, cancel := context.WithTimeout(ctx, timeout)
    defer cancel()
    
    ticker := time.NewTicker(3 * time.Second)
    defer ticker.Stop()
    
    for {
        select {
        case <-ctx.Done():
            return nil, ctx.Err()
        case <-ticker.C:
            status, err := c.client.GetTransactionStatus(ctx, &gasbank.TransactionStatusRequest{
                TransactionHash: txHash,
            })
            if err != nil {
                c.logger.Error("Failed to get transaction status", zap.String("txHash", txHash), zap.Error(err))
                continue
            }
            
            if status.Status == gasbank.TransactionStatus_CONFIRMED {
                return status, nil
            } else if status.Status == gasbank.TransactionStatus_FAILED {
                return status, fmt.Errorf("transaction failed: %s", status.ErrorMessage)
            }
            
            c.logger.Debug("Transaction not yet confirmed", zap.String("txHash", txHash), zap.String("status", status.Status.String()))
        }
    }
}
```

### Automation Service to Function Service Integration

```
┌───────────────────┐                 ┌───────────────────┐
│                   │                 │                   │
│  Job Executor     │                 │  Function         │
│                   │                 │  Service          │
└─────────┬─────────┘                 └─────────┬─────────┘
          │                                     │
          │ 1. Request Function Execution       │
          │────────────────────────────────────▶│
          │                                     │
          │ 2. Validate Function & Parameters   │
          │                                     │
          │ 3. Check Permissions                │
          │                                     │
          │ 4. Schedule Execution               │
          │                                     │
          │ 5. Execute in TEE                   │
          │                                     │
          │ 6. Return Result                    │
          │◀────────────────────────────────────│
          │                                     │
┌─────────┴─────────┐                 ┌─────────┴─────────┐
│                   │                 │                   │
│  Job Executor     │                 │  Function         │
│                   │                 │  Service          │
└───────────────────┘                 └───────────────────┘
```

**Implementation Example:**

```go
// FunctionServiceClient for interacting with the Function Service
type FunctionServiceClient struct {
    client functions.Client
    logger *zap.Logger
}

// ExecuteFunction calls the Function Service to execute a user-defined function
func (c *FunctionServiceClient) ExecuteFunction(ctx context.Context, request *ExecuteFunctionRequest) (*ExecuteFunctionResponse, error) {
    // Convert parameters to appropriate format
    params := make([]*functions.Parameter, 0, len(request.Parameters))
    for key, value := range request.Parameters {
        paramValue, err := convertToParameterValue(value)
        if err != nil {
            return nil, fmt.Errorf("invalid parameter value for %s: %w", key, err)
        }
        
        params = append(params, &functions.Parameter{
            Name:  key,
            Value: paramValue,
        })
    }
    
    // Create execution request
    execRequest := &functions.ExecuteRequest{
        FunctionID:  request.FunctionID,
        Parameters:  params,
        ReturnType:  functions.ReturnType_JSON,
        ExecutionID: uuid.New().String(),
        Metadata: map[string]string{
            "source":     "automation-service",
            "job_id":     request.JobID,
            "trigger_id": request.TriggerID,
        },
    }
    
    // Call Function Service
    c.logger.Debug("Executing function", 
        zap.String("functionID", request.FunctionID),
        zap.String("jobID", request.JobID))
    
    resp, err := c.client.Execute(ctx, execRequest)
    if err != nil {
        c.logger.Error("Function execution failed", 
            zap.String("functionID", request.FunctionID),
            zap.String("jobID", request.JobID),
            zap.Error(err))
        return nil, fmt.Errorf("function execution failed: %w", err)
    }
    
    // Parse and return the result
    result, err := parseFunctionResult(resp.Result)
    if err != nil {
        return nil, fmt.Errorf("failed to parse function result: %w", err)
    }
    
    return &ExecuteFunctionResponse{
        ExecutionID: resp.ExecutionID,
        Success:     resp.Success,
        Result:      result,
        Duration:    time.Duration(resp.DurationMs) * time.Millisecond,
    }, nil
}

// Helper function to convert Go values to Function Service parameter values
func convertToParameterValue(value interface{}) (*functions.ParameterValue, error) {
    switch v := value.(type) {
    case string:
        return &functions.ParameterValue{
            Type:  functions.ParameterType_STRING,
            Value: &functions.ParameterValue_StringValue{StringValue: v},
        }, nil
    case int:
        return &functions.ParameterValue{
            Type:  functions.ParameterType_INTEGER,
            Value: &functions.ParameterValue_IntValue{IntValue: int64(v)},
        }, nil
    case int64:
        return &functions.ParameterValue{
            Type:  functions.ParameterType_INTEGER,
            Value: &functions.ParameterValue_IntValue{IntValue: v},
        }, nil
    case float64:
        return &functions.ParameterValue{
            Type:  functions.ParameterType_FLOAT,
            Value: &functions.ParameterValue_FloatValue{FloatValue: v},
        }, nil
    case bool:
        return &functions.ParameterValue{
            Type:  functions.ParameterType_BOOLEAN,
            Value: &functions.ParameterValue_BoolValue{BoolValue: v},
        }, nil
    case []interface{}:
        values := make([]*functions.ParameterValue, len(v))
        for i, item := range v {
            paramValue, err := convertToParameterValue(item)
            if err != nil {
                return nil, err
            }
            values[i] = paramValue
        }
        return &functions.ParameterValue{
            Type:  functions.ParameterType_ARRAY,
            Value: &functions.ParameterValue_ArrayValue{ArrayValue: &functions.ArrayValue{Values: values}},
        }, nil
    case map[string]interface{}:
        fields := make(map[string]*functions.ParameterValue)
        for key, val := range v {
            paramValue, err := convertToParameterValue(val)
            if err != nil {
                return nil, err
            }
            fields[key] = paramValue
        }
        return &functions.ParameterValue{
            Type:  functions.ParameterType_OBJECT,
            Value: &functions.ParameterValue_ObjectValue{ObjectValue: &functions.ObjectValue{Fields: fields}},
        }, nil
    default:
        return nil, fmt.Errorf("unsupported parameter type: %T", value)
    }
}
```

## Deployment Guidelines

### Deployment Architecture

The Automation Service should be deployed with redundancy to ensure high availability:

```
┌─────────────────────────────────────────────────────────────────────┐
│                           Load Balancer                             │
└───────────────┬─────────────────────────────────┬───────────────────┘
                │                                 │
                ▼                                 ▼
┌───────────────────────────┐       ┌───────────────────────────┐
│  Automation API Service   │       │  Automation API Service   │
│  Instance 1               │       │  Instance 2               │
└───────────────────────────┘       └───────────────────────────┘
                │                                 │
                └─────────────┬──────────────────┬┘
                              │                  │
                              ▼                  ▼
              ┌───────────────────────┐  ┌───────────────────────┐
              │  Monitor Service      │  │  Monitor Service      │
              │  Instance 1           │  │  Instance 2           │
              └───────────────────────┘  └───────────────────────┘
                           │                       │
                           └─────────┬─────────────┘
                                     │
                                     ▼
                        ┌───────────────────────┐
                        │                       │
                        │   Database Cluster    │
                        │                       │
                        └───────────────────────┘
```

### Resource Requirements

For a production deployment, the following minimum resources are recommended:

| Component          | CPU    | Memory | Disk  | Notes                                  |
|--------------------|--------|--------|-------|----------------------------------------|
| API Service        | 2 CPU  | 4 GB   | 20 GB | Min 2 instances for high availability  |
| Monitor Service    | 4 CPU  | 8 GB   | 40 GB | Min 2 instances, scales with load      |
| Database           | 4 CPU  | 16 GB  | 100 GB| Replicated for redundancy              |
| Load Balancer      | 2 CPU  | 4 GB   | N/A   | Managed service recommended            |

### Environment Variables

```
# Database Configuration
DB_CONNECTION_STRING=postgres://user:password@postgres:5432/automationdb
DB_MAX_CONNECTIONS=20
DB_IDLE_CONNECTIONS=5

# Neo N3 Configuration
NEO_RPC_ENDPOINTS=https://rpc1.neo.org:10331,https://rpc2.neo.org:10331
NEO_NETWORK_MAGIC=860833102

# Service Integration
GASBANK_SERVICE_ENDPOINT=gasbank-service:50051
FUNCTION_SERVICE_ENDPOINT=function-service:50051
SECRETS_SERVICE_ENDPOINT=secrets-service:50051

# Monitoring Configuration
BLOCK_POLLING_INTERVAL_MS=5000
EVENT_POLLING_INTERVAL_MS=2000

# Execution Configuration
MAX_CONCURRENT_EXECUTIONS=100
EXECUTION_TIMEOUT_SEC=60
MAX_RETRIES=3
RETRY_DELAY_MS=5000

# Security
TLS_CERT_FILE=/certs/server.crt
TLS_KEY_FILE=/certs/server.key
TLS_CA_FILE=/certs/ca.crt
```

### Kubernetes Deployment Example

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: automation-service
  namespace: neo-service-layer
spec:
  replicas: 2
  selector:
    matchLabels:
      app: automation-service
  template:
    metadata:
      labels:
        app: automation-service
    spec:
      containers:
      - name: automation-service
        image: neo-service-layer/automation-service:latest
        ports:
        - containerPort: 8080
          name: http
        - containerPort: 50051
          name: grpc
        env:
        - name: DB_CONNECTION_STRING
          valueFrom:
            secretKeyRef:
              name: automation-db-credentials
              key: connection-string
        - name: NEO_RPC_ENDPOINTS
          value: "https://rpc1.neo.org:10331,https://rpc2.neo.org:10331"
        - name: GASBANK_SERVICE_ENDPOINT
          value: "gasbank-service:50051"
        - name: FUNCTION_SERVICE_ENDPOINT
          value: "function-service:50051"
        resources:
          requests:
            cpu: "2"
            memory: "4Gi"
          limits:
            cpu: "4"
            memory: "8Gi"
        volumeMounts:
        - name: certs
          mountPath: /certs
          readOnly: true
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /ready
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
      volumes:
      - name: certs
        secret:
          secretName: automation-service-certs
---
apiVersion: v1
kind: Service
metadata:
  name: automation-service
  namespace: neo-service-layer
spec:
  selector:
    app: automation-service
  ports:
  - port: 8080
    name: http
    targetPort: 8080
  - port: 50051
    name: grpc
    targetPort: 50051
  type: ClusterIP
```

## Security Considerations

### Authentication and Authorization

The Automation Service uses a multi-layered security approach:

1. **API Layer Authentication**: JWT-based authentication for all API requests
2. **Service-to-Service Authentication**: mTLS for secure communication between services
3. **Blockchain Interaction Authorization**: Verify that job creator has appropriate permissions

### Data Protection

1. **In-Transit Protection**: All service communication uses TLS 1.3+
2. **At-Rest Protection**: Sensitive job configurations are encrypted in the database
3. **Key Rotation**: Regular rotation of encryption keys and credentials

### Example Security Implementation

```go
// SecureJobExecutor protects job execution with authentication and authorization
type SecureJobExecutor struct {
    executor JobExecutor
    auth     Authenticator
    logger   *zap.Logger
}

// ExecuteJob implements JobExecutor with added security checks
func (e *SecureJobExecutor) ExecuteJob(ctx context.Context, job *Job, trigger TriggerEvent) (*ExecutionResult, error) {
    // 1. Verify the job ownership and permissions
    if err := e.auth.VerifyJobOwnership(ctx, job.ID, job.Owner); err != nil {
        e.logger.Warn("Unauthorized job execution attempt", 
            zap.String("jobID", job.ID),
            zap.String("owner", job.Owner),
            zap.Error(err))
        return nil, fmt.Errorf("unauthorized: %w", err)
    }
    
    // 2. Verify action permissions
    if err := e.auth.VerifyActionPermissions(ctx, job.Owner, job.Action); err != nil {
        e.logger.Warn("Insufficient permissions for job action", 
            zap.String("jobID", job.ID),
            zap.String("actionType", job.Action.Type),
            zap.Error(err))
        return nil, fmt.Errorf("permission denied: %w", err)
    }
    
    // 3. Log the execution attempt for audit
    e.logger.Info("Authorized job execution", 
        zap.String("jobID", job.ID),
        zap.String("owner", job.Owner),
        zap.String("triggerType", trigger.Type))
    
    // 4. Execute the job with the secured context
    secureCtx := security.WithExecutionContext(ctx, &security.ExecutionContext{
        JobID:   job.ID,
        Owner:   job.Owner,
        Actions: []string{job.Action.Type},
    })
    
    return e.executor.ExecuteJob(secureCtx, job, trigger)
}
```

## Related Documentation

- [Overview](OVERVIEW.md) - Automation Service overview
- [Architecture](ARCHITECTURE.md) - Detailed architecture design
- [API Reference](API_REFERENCE.md) - API specifications
- [Trigger Evaluator](TRIGGER_EVALUATOR.md) - Detailed documentation on trigger evaluation
- [Action Executor](ACTION_EXECUTOR.md) - Detailed documentation on action execution
- [Function Service Integration](../functionservice/API_REFERENCE.md) - Function Service API documentation
- [Gas Bank Service Integration](../gasbankservice/API_REFERENCE.md) - Gas Bank Service API documentation
