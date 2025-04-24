# Neo Service Layer - Service Communication Flow

## Communication Patterns

The Neo Service Layer uses a combination of direct method calls and VSOCK communication to enable secure interaction between services and the enclave.

```
+----------------+     +----------------+     +----------------+
|                |     |                |     |                |
|  Client        +---->+  API Gateway   +---->+  Service Layer |
|                |     |                |     |                |
+----------------+     +----------------+     +-------+--------+
                                                      |
                                                      |
                                                      v
                                        +-------------+-------------+
                                        |                           |
                                        |  VSOCK Communication      |
                                        |                           |
                                        +-------------+-------------+
                                                      |
                                                      |
                                                      v
                                        +-------------+-------------+
                                        |                           |
                                        |  Enclave Services         |
                                        |                           |
                                        +---------------------------+
```

## Request Flow

1. Client sends a request to the API Gateway
2. API Gateway routes the request to the appropriate Service Layer component
3. Service Layer processes the request and determines if enclave operations are needed
4. If enclave operations are required, the Service Layer sends a request to the Enclave via VSOCK
5. Enclave processes the request securely and returns the result
6. Service Layer completes the request processing and returns the response to the client

## VSOCK Communication Protocol

The VSOCK communication between the parent instance and the enclave follows a request-response pattern:

```
Parent Instance                                 Enclave
     |                                            |
     |  1. Serialize Request                      |
     |  2. Send Request Length                    |
     |  ----------------------------------------> |
     |  3. Send Request Data                      |
     |  ----------------------------------------> |
     |                                            |
     |                                            |  4. Process Request
     |                                            |
     |  <---------------------------------------- |  5. Send Response Length
     |  <---------------------------------------- |  6. Send Response Data
     |  7. Deserialize Response                   |
     |                                            |
```

## Message Format

All messages exchanged between the parent instance and the enclave use a standardized JSON format:

```
{
    "requestId": "unique-request-identifier",
    "serviceType": "service-name",
    "operation": "operation-name",
    "payload": { ... operation-specific data ... }
}
```

Response format:

```
{
    "requestId": "unique-request-identifier",
    "success": true/false,
    "errorMessage": "error message if success is false",
    "payload": { ... operation-specific response data ... }
}
```

## Service Routing

The enclave routes requests to the appropriate service based on the `serviceType` field:

```
                      +------------------+
                      |                  |
                      |  VSOCK Server    |
                      |                  |
                      +--------+---------+
                               |
                               v
                      +--------+---------+
                      |                  |
                      |  Request Router  |
                      |                  |
                      +--------+---------+
                               |
           +------------------+------------------+
           |                  |                  |
           v                  v                  v
+----------+------+  +--------+-------+  +-------+--------+
|                 |  |                |  |                |
| Account Service |  | Wallet Service |  | Secrets Service|
|                 |  |                |  |                |
+-----------------+  +----------------+  +----------------+
           |                  |                  |
           v                  v                  v
+----------+------+  +--------+-------+  +-------+--------+
|                 |  |                |  |                |
| Function Service|  | Price Feed     |  | Gas Bank       |
|                 |  |                |  |                |
+-----------------+  +----------------+  +----------------+
```

This architecture ensures that all sensitive operations are performed within the secure enclave while maintaining a clean separation of concerns between different service components.
