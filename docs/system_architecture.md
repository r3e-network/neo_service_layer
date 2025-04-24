# Neo Service Layer System Architecture

## Overview

Neo Service Layer is a serverless platform designed for Neo N3 blockchain, providing chainlink-like services with AWS Nitro Enclave support. The system enables users to deploy and execute JavaScript, Python, and C# functions securely, with integrated blockchain interaction capabilities.

## System Goals

1. Provide a secure, scalable serverless platform for Neo N3 blockchain
2. Enable function execution in a secure enclave environment
3. Support blockchain interaction through various services
4. Ensure data privacy and security through TEE (Trusted Execution Environment)
5. Offer comprehensive account and wallet management
6. Provide price feed and event monitoring capabilities

## High-Level Architecture

```
                                  +-------------------+
                                  |                   |
                                  |   User Interface  |
                                  |                   |
                                  +--------+----------+
                                           |
                                           v
+------------------+            +----------+---------+            +------------------+
|                  |            |                    |            |                  |
|  Authentication  +<---------->+  API Gateway       +<---------->+  Function Store  |
|                  |            |                    |            |                  |
+------------------+            +----------+---------+            +------------------+
                                           |
                                           v
                               +-----------+------------+
                               |                        |
                               |  Service Orchestrator  |
                               |                        |
                               +---+----------------+---+
                                   |                |
         +----------------------+  |                |  +----------------------+
         |                      |  |                |  |                      |
         v                      v  v                v  v                      v
+--------+-------+    +---------+--+--+    +-------+---+---+    +------------+----+
|                |    |              |    |               |    |                 |
| Account Service|    | Wallet Service|    | Secrets Service|    | PriceFeed Service|
|                |    |              |    |               |    |                 |
+----------------+    +--------------+    +---------------+    +-----------------+
         |                   |                   |                      |
         |                   |                   |                      |
         v                   v                   v                      v
+--------+-------------------+-------------------+----------------------+-------+
|                                                                               |
|                             AWS Nitro Enclave                                 |
|                                                                               |
+-------------------------------------------------------------------------------+
         |                   |                   |                      |
         v                   v                   v                      v
+--------+-------+    +------+-------+    +------+-------+    +--------+-------+
|                |    |              |    |              |    |                |
| Event Monitor  |    |Function Runner|    |Storage Service|    |  Metrics Service|
|                |    |              |    |              |    |                |
+----------------+    +--------------+    +--------------+    +----------------+
         |                   |                   |                      |
         v                   v                   v                      v
+--------+-------------------+-------------------+----------------------+-------+
|                                                                               |
|                               Neo N3 Blockchain                               |
|                                                                               |
+-------------------------------------------------------------------------------+
```

## Core Components

### 1. API Gateway
Serves as the entry point for all external requests, handling authentication, request routing, and response formatting.

### 2. Service Orchestrator
Coordinates the interaction between various services, manages service discovery, and handles inter-service communication.

### 3. Account Service
Manages user registration, authentication, and account management. Integrates with Auth0 and supports Neo N3 accounts for registration.

### 4. Wallet Service
Manages the service layer's own accounts and wallets for blockchain interaction. Handles transaction signing and submission.

### 5. Secrets Service
Provides secure storage and management of user secrets, with strict access control and permission validation.

### 6. PriceFeed Service
Fetches price data from various sources and submits it to the Neo N3 oracle smart contract. Maintains local data copies for function access.

### 7. Event Monitor
Monitors Neo N3 blockchain events and other triggers (time-based, date-based, etc.) to execute functions automatically.

### 8. Function Runner
Executes user-deployed JavaScript, Python, and C# functions in a secure environment.

### 9. Storage Service
Provides persistent storage capabilities for functions and services.

### 10. Metrics Service
Collects and reports metrics on function execution, service performance, and resource usage.

## Security Architecture

### AWS Nitro Enclave Integration

The Neo Service Layer leverages AWS Nitro Enclaves to provide a secure, isolated execution environment for sensitive operations:

```
+-------------------------------------------+
|              EC2 Instance                 |
|                                           |
|  +-----------------------------------+    |
|  |         Parent Instance           |    |
|  |                                   |    |
|  |  +---------------------------+    |    |
|  |  |                           |    |    |
|  |  |    Non-sensitive          |    |    |
|  |  |    Components             |    |    |
|  |  |                           |    |    |
|  |  +---------------------------+    |    |
|  |                                   |    |
|  +-----------------------------------+    |
|                                           |
|  +-----------------------------------+    |
|  |         Nitro Enclave            |    |
|  |                                   |    |
|  |  +---------------------------+    |    |
|  |  |                           |    |    |
|  |  |    Sensitive Components   |    |    |
|  |  |    - Account Management   |    |    |
|  |  |    - Wallet (Private Keys)|    |    |
|  |  |    - Secrets Management   |    |    |
|  |  |    - Function Execution   |    |    |
|  |  |    - PriceFeed            |    |    |
|  |  |                           |    |    |
|  |  +---------------------------+    |    |
|  |                                   |    |
|  +-----------------------------------+    |
|                                           |
+-------------------------------------------+
```

Key security features:
1. **Isolated Memory**: Enclave memory is encrypted and isolated from the parent instance
2. **No Persistent Storage**: Enclaves have no persistent storage, enhancing security
3. **No Interactive Access**: No SSH or other interactive access to the enclave
4. **Cryptographic Attestation**: Provides proof of the enclave's identity and integrity
5. **Secure Communication**: VSOCK-based communication between parent instance and enclave

### Data Flow Security

```
+----------------+     +----------------+     +----------------+
|                |     |                |     |                |
|  Client        +---->+  API Gateway   +---->+  Service       |
|                |     |                |     |  Orchestrator  |
+----------------+     +----------------+     +-------+--------+
                                                      |
                                                      v
+----------------+     +----------------+     +-------+--------+
|                |     |                |     |                |
|  Neo N3        |<----+  Enclave       |<----+  Service       |
|  Blockchain    |     |  Services      |     |  Request       |
|                |     |                |     |                |
+----------------+     +----------------+     +----------------+
```

All sensitive operations, including account management, wallet operations, secrets management, and function execution, are performed within the secure enclave environment.

## Communication Architecture

### Inter-Service Communication

Services communicate using a combination of:
1. **Direct API Calls**: For synchronous operations
2. **Message Queue**: For asynchronous operations
3. **Event Bus**: For event-driven communication

### Enclave Communication

Communication with the enclave is handled through:
1. **VSOCK**: For direct communication between parent instance and enclave
2. **Attestation**: For verifying enclave identity and integrity

## Deployment Architecture

The Neo Service Layer is deployed as a set of containerized services on AWS, with sensitive components running in Nitro Enclaves:

```
+-------------------------------------------+
|              AWS Environment              |
|                                           |
|  +----------------+  +----------------+   |
|  |                |  |                |   |
|  |  EC2 Instance  |  |  EC2 Instance  |   |
|  |  with Enclave  |  |  with Enclave  |   |
|  |                |  |                |   |
|  +----------------+  +----------------+   |
|                                           |
|  +----------------+  +----------------+   |
|  |                |  |                |   |
|  |  Load Balancer |  |  API Gateway   |   |
|  |                |  |                |   |
|  +----------------+  +----------------+   |
|                                           |
|  +----------------+  +----------------+   |
|  |                |  |                |   |
|  |  Database      |  |  Storage       |   |
|  |                |  |                |   |
|  +----------------+  +----------------+   |
|                                           |
+-------------------------------------------+
```

## Scalability Architecture

The system is designed to scale horizontally by adding more instances as needed:

```
+-------------------+
|                   |
| Load Balancer     |
|                   |
+--------+----------+
         |
         v
+--------+----------+
|                   |
| Auto Scaling Group|
|                   |
+---+----------+----+
    |          |
    v          v
+---+---+  +---+---+
|       |  |       |
| EC2   |  | EC2   |
|       |  |       |
+-------+  +-------+
```

Each component is designed to be stateless where possible, enabling easy scaling and high availability.
