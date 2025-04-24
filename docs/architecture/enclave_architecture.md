# Neo Service Layer - Enclave Architecture

## Enclave Overview

The Neo Service Layer uses AWS Nitro Enclaves to provide a secure, isolated environment for processing sensitive operations. The enclave architecture ensures that even if the parent instance is compromised, the sensitive data and operations remain protected.

```
+------------------------------------------+
|                                          |
|             Parent Instance              |
|                                          |
+------------------+---------------------+-+
                   |                     |
                   | VSOCK               |
                   | Communication       |
                   |                     |
+------------------v---------------------v-+
|                                          |
|             Nitro Enclave                |
|                                          |
|  +----------------------------------+    |
|  |                                  |    |
|  |         VSOCK Server             |    |
|  |                                  |    |
|  +---------------+------------------+    |
|                  |                       |
|                  |                       |
|  +---------------v------------------+    |
|  |                                  |    |
|  |       Request Router             |    |
|  |                                  |    |
|  +--+-------------+-------------+---+    |
|     |             |             |        |
|     |             |             |        |
|  +--v----+     +--v----+     +--v----+  |
|  |       |     |       |     |       |  |
|  | Acct  |     |Wallet |     |Secret |  |
|  | Svc   |     | Svc   |     | Svc   |  |
|  |       |     |       |     |       |  |
|  +-------+     +-------+     +-------+  |
|                                          |
|  +-------+     +-------+     +-------+  |
|  |       |     |       |     |       |  |
|  | Func  |     | Price |     | Gas   |  |
|  | Svc   |     | Feed  |     | Bank  |  |
|  |       |     |       |     |       |  |
|  +-------+     +-------+     +-------+  |
|                                          |
+------------------------------------------+
```

## Enclave Components

### VSOCK Server

The VSOCK Server is responsible for handling communication between the parent instance and the enclave. It:

1. Listens for incoming connections on a predefined port
2. Receives and deserializes requests
3. Routes requests to the appropriate service
4. Serializes and sends responses back to the parent instance

### Request Router

The Request Router determines which enclave service should handle a particular request based on the `serviceType` and `operation` fields in the request. It:

1. Validates incoming requests
2. Routes requests to the appropriate service
3. Handles error conditions and generates appropriate error responses

### Enclave Services

The enclave hosts several services that handle sensitive operations:

1. **Account Service**: Manages user accounts and permissions
2. **Wallet Service**: Manages cryptographic keys and signs transactions
3. **Secrets Service**: Securely stores and manages sensitive data
4. **Function Service**: Executes user-defined functions in a secure environment
5. **Price Feed Service**: Securely fetches, validates, and provides price data
6. **Gas Bank Service**: Manages gas allocation and usage for transactions

## Security Features

```
+------------------------------------------+
|                                          |
|             Enclave Security             |
|                                          |
|  +----------------------------------+    |
|  |                                  |    |
|  |     Memory Encryption            |    |
|  |                                  |    |
|  +----------------------------------+    |
|                                          |
|  +----------------------------------+    |
|  |                                  |    |
|  |     Attestation                  |    |
|  |                                  |    |
|  +----------------------------------+    |
|                                          |
|  +----------------------------------+    |
|  |                                  |    |
|  |     Secure Key Management        |    |
|  |                                  |    |
|  +----------------------------------+    |
|                                          |
|  +----------------------------------+    |
|  |                                  |    |
|  |     Isolated Execution           |    |
|  |                                  |    |
|  +----------------------------------+    |
|                                          |
+------------------------------------------+
```

1. **Memory Encryption**: All memory within the enclave is encrypted, ensuring that sensitive data cannot be accessed even if the parent instance is compromised.

2. **Attestation**: The enclave provides cryptographic proof of its identity and integrity, allowing clients to verify that they are communicating with a legitimate enclave.

3. **Secure Key Management**: Cryptographic keys are generated and stored within the enclave, never leaving the secure environment.

4. **Isolated Execution**: The enclave runs in a separate, isolated environment with its own memory and CPU resources, preventing the parent instance from accessing its operations.

## Data Flow Within the Enclave

```
+------------------+     +------------------+     +------------------+
|                  |     |                  |     |                  |
| Request Received +---->+ Request Validated+---->+ Service Selected |
|                  |     |                  |     |                  |
+------------------+     +------------------+     +--------+---------+
                                                           |
                                                           v
+------------------+     +------------------+     +--------+---------+
|                  |     |                  |     |                  |
| Response Sent    +<----+ Result Processed +<----+ Operation        |
|                  |     |                  |     | Executed         |
+------------------+     +------------------+     +------------------+
```

1. Request is received by the VSOCK Server
2. Request is validated for proper format and permissions
3. Appropriate service is selected based on the request type
4. Operation is executed within the secure enclave
5. Result is processed and formatted for response
6. Response is sent back to the parent instance

This architecture ensures that all sensitive operations are performed within the secure enclave, protecting them from potential attacks on the parent instance.
