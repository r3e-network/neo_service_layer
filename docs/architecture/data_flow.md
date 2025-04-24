# Neo Service Layer - Data Flow Diagram

## Overview

This document illustrates the data flow within the Neo Service Layer, showing how data moves between different components and services.

## Client Request Flow

```
+--------+     +--------+     +--------+     +--------+     +--------+
|        |     |        |     |        |     |        |     |        |
| Client +---->+ API    +---->+ Auth   +---->+ Service+---->+ Storage|
|        |     | Gateway|     | Service|     | Layer  |     | Layer  |
|        |     |        |     |        |     |        |     |        |
+--------+     +--------+     +--------+     +--------+     +--------+
    ^                                            |
    |                                            |
    |                                            v
    |                                        +--------+
    |                                        |        |
    +----------------------------------------+ Enclave|
                                             | Service|
                                             |        |
                                             +--------+
```

1. Client sends a request to the API Gateway
2. API Gateway validates the request format and routes it to the Auth Service
3. Auth Service authenticates the request and authorizes the operation
4. Service Layer processes the business logic
5. If needed, the Service Layer interacts with the Enclave Service for secure operations
6. Service Layer stores/retrieves data from the Storage Layer
7. Response flows back to the client

## Function Execution Flow

```
+--------+     +--------+     +--------+     +--------+
|        |     |        |     |        |     |        |
| Client +---->+ API    +---->+ Function+---->+ Runtime|
|        |     | Gateway|     | Service |     | Engine |
|        |     |        |     |         |     |        |
+--------+     +--------+     +--------+     +--------+
                                  |              |
                                  v              v
                              +--------+     +--------+
                              |        |     |        |
                              | Storage+---->+ Enclave|
                              | Layer  |     | Service|
                              |        |     |        |
                              +--------+     +--------+
                                  |              |
                                  v              v
                              +--------+     +--------+
                              |        |     |        |
                              | Neo N3 |     | Gas    |
                              | RPC    |     | Bank   |
                              |        |     |        |
                              +--------+     +--------+
```

1. Client submits a function execution request
2. API Gateway validates and routes the request
3. Function Service prepares the execution environment
4. Runtime Engine executes the function code
5. During execution, the function may:
   - Access data from the Storage Layer
   - Request secure operations from the Enclave Service
   - Interact with the Neo N3 blockchain via RPC
   - Consume GAS from the Gas Bank
6. Results are returned to the client

## Price Feed Data Flow

```
+--------+     +--------+     +--------+     +--------+
|        |     |        |     |        |     |        |
| External+---->+ Price  +---->+ Enclave +---->+ Storage|
| Sources|     | Feed   |     | Service |     | Layer  |
|        |     | Service|     |         |     |        |
+--------+     +--------+     +--------+     +--------+
                    |              |
                    v              v
                +--------+     +--------+
                |        |     |        |
                | Event  |     | Neo N3 |
                | Monitor|     | RPC    |
                |        |     |        |
                +--------+     +--------+
```

1. External price sources provide data
2. Price Feed Service collects and validates the data
3. Enclave Service securely processes and signs the data
4. Processed data is stored in the Storage Layer
5. Event Monitor tracks relevant blockchain events
6. Price data is submitted to the Neo N3 blockchain when needed

## Wallet Operation Flow

```
+--------+     +--------+     +--------+     +--------+
|        |     |        |     |        |     |        |
| Client +---->+ API    +---->+ Wallet +---->+ Enclave|
|        |     | Gateway|     | Service|     | Service|
|        |     |        |     |        |     |        |
+--------+     +--------+     +--------+     +--------+
                                  |              |
                                  v              v
                              +--------+     +--------+
                              |        |     |        |
                              | Storage+---->+ Neo N3 |
                              | Layer  |     | RPC    |
                              |        |     |        |
                              +--------+     +--------+
```

1. Client sends a wallet operation request
2. API Gateway validates and routes the request
3. Wallet Service processes the request
4. For sensitive operations (key management, signing), the Enclave Service is used
5. Wallet data is stored in the Storage Layer
6. Transactions are submitted to the Neo N3 blockchain

## Secrets Management Flow

```
+--------+     +--------+     +--------+     +--------+
|        |     |        |     |        |     |        |
| Client +---->+ API    +---->+ Secrets+---->+ Enclave|
|        |     | Gateway|     | Service|     | Service|
|        |     |        |     |        |     |        |
+--------+     +--------+     +--------+     +--------+
                                  |              |
                                  v              v
                              +--------+     +--------+
                              |        |     |        |
                              | Storage+---->+ Function|
                              | Layer  |     | Service |
                              |        |     |        |
                              +--------+     +--------+
```

1. Client sends a secrets management request
2. API Gateway validates and routes the request
3. Secrets Service processes the request
4. Sensitive operations are performed in the Enclave Service
5. Encrypted secrets are stored in the Storage Layer
6. Function Service can access secrets when authorized

## Event Monitoring Flow

```
+--------+     +--------+     +--------+     +--------+
|        |     |        |     |        |     |        |
| Neo N3 +---->+ Event  +---->+ Storage+---->+ Function|
| Network|     | Monitor|     | Layer  |     | Service |
|        |     |        |     |        |     |        |
+--------+     +--------+     +--------+     +--------+
                    |                            |
                    v                            v
                +--------+                   +--------+
                |        |                   |        |
                | Notifi-|                   | Client |
                | cation |                   | Apps   |
                |        |                   |        |
                +--------+                   +--------+
```

1. Neo N3 Network generates events
2. Event Monitor captures and processes events
3. Event data is stored in the Storage Layer
4. Function Service can be triggered by events
5. Notifications can be sent to subscribed clients
6. Client applications can query event data

## Analytics Data Flow

```
+--------+     +--------+     +--------+     +--------+
|        |     |        |     |        |     |        |
| Service+---->+ Metrics+---->+ Analytics+-->+ Storage|
| Layer  |     | Service|     | Service |    | Layer  |
|        |     |        |     |         |    |        |
+--------+     +--------+     +--------+     +--------+
                                  |              |
                                  v              v
                              +--------+     +--------+
                              |        |     |        |
                              | Dash-  |     | Alert  |
                              | boards |     | System |
                              |        |     |        |
                              +--------+     +--------+
```

1. Service Layer components generate metrics and logs
2. Metrics Service collects and processes metrics
3. Analytics Service performs data analysis
4. Processed data is stored in the Storage Layer
5. Dashboards visualize the analytics data
6. Alert System monitors for anomalies and triggers notifications

These data flow diagrams provide a high-level overview of how data moves through the Neo Service Layer, illustrating the interactions between different components and services.
