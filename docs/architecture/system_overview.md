# Neo Service Layer - System Overview

## Architecture Overview

The Neo Service Layer is a centralized chainlink providing various services for the Neo N3 blockchain. It functions as a serverless platform allowing users to deploy JS/Python/C# functions with secure enclave support.

```
+---------------------+     +------------------------+     +-------------------+
|                     |     |                        |     |                   |
|  External Clients   +---->+  Neo Service Layer API +---->+  Service Layer    |
|  (dApps, Users)     |     |  (Public Endpoints)    |     |  (Core Services)  |
|                     |     |                        |     |                   |
+---------------------+     +------------------------+     +--------+----------+
                                                                   |
                                                                   |
                                                                   v
                                      +----------------------------+----------------------------+
                                      |                                                        |
                                      |                  AWS Nitro Enclave                     |
                                      |                                                        |
                                      |  +----------------+  +------------------------+        |
                                      |  |                |  |                        |        |
                                      |  | Wallet Service |  | Secrets Management     |        |
                                      |  |                |  |                        |        |
                                      |  +----------------+  +------------------------+        |
                                      |                                                        |
                                      |  +----------------+  +------------------------+        |
                                      |  |                |  |                        |        |
                                      |  | Account Mgmt   |  | Function Execution     |        |
                                      |  |                |  |                        |        |
                                      |  +----------------+  +------------------------+        |
                                      |                                                        |
                                      |  +----------------+  +------------------------+        |
                                      |  |                |  |                        |        |
                                      |  | Price Feed     |  | Gas Bank               |        |
                                      |  |                |  |                        |        |
                                      |  +----------------+  +------------------------+        |
                                      |                                                        |
                                      +--------------------------------------------------------+
                                                           |
                                                           |
                                                           v
                                      +--------------------------------------------------------+
                                      |                                                        |
                                      |                    Neo N3 Blockchain                   |
                                      |                                                        |
                                      +--------------------------------------------------------+
```

## Key Components

1. **API Layer**: Public-facing endpoints that handle client requests
2. **Service Layer**: Core business logic implementation
3. **Enclave Services**: Secure execution environment for sensitive operations
4. **Storage Layer**: Persistence layer for service data
5. **Blockchain Integration**: Communication with Neo N3 blockchain

## Security Model

The Neo Service Layer uses AWS Nitro Enclaves to provide hardware-level isolation for sensitive operations:

```
+------------------------------------------+
|                                          |
|             Parent Instance              |
|                                          |
|  +----------------------------------+    |
|  |                                  |    |
|  |  Public Services & API Endpoints |    |
|  |                                  |    |
|  +---------------+------------------+    |
|                  |                       |
|                  | VSOCK                 |
|                  | Communication         |
|  +---------------v------------------+    |
|  |                                  |    |
|  |        Nitro Enclave             |    |
|  |                                  |    |
|  |  +---------------------------+   |    |
|  |  |                           |   |    |
|  |  |  Sensitive Operations     |   |    |
|  |  |  - Key Management         |   |    |
|  |  |  - Transaction Signing    |   |    |
|  |  |  - Secret Management      |   |    |
|  |  |                           |   |    |
|  |  +---------------------------+   |    |
|  |                                  |    |
|  +----------------------------------+    |
|                                          |
+------------------------------------------+
```

All sensitive operations, including private key management, transaction signing, and secret management, are performed within the secure enclave to ensure that even if the parent instance is compromised, the sensitive data remains protected.
