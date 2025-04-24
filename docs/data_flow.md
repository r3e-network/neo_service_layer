# Neo Service Layer Data Flow

## Overview

This document describes the data flow within the Neo Service Layer, detailing how data moves between different components and services, including interactions with the Neo N3 blockchain and AWS Nitro Enclave.

## User Registration and Authentication Flow

```
+--------+    1. Register    +--------+    2. Create Account    +--------+
|        |----------------->|        |----------------------->|        |
| User   |                  | API    |                        | Account|
|        |<-----------------|        |<-----------------------| Service|
+--------+    8. Response   +--------+    7. Account Created  +--------+
                                |                                  |
                                |   3. Store Account               |
                                v                                  v
                            +--------+    4. Encrypt Data     +--------+
                            |        |----------------------->|        |
                            | DB     |                        | Enclave|
                            |        |<-----------------------|        |
                            +--------+    5. Encrypted Data   +--------+
                                ^                                  |
                                |   6. Confirmation                |
                                +----------------------------------+
```

1. User submits registration information
2. API Gateway forwards request to Account Service
3. Account Service prepares account data
4. Sensitive account data is sent to the enclave for encryption
5. Enclave returns encrypted data
6. Enclave confirms successful encryption
7. Account Service confirms account creation
8. Response is returned to the user

## Function Deployment Flow

```
+--------+    1. Deploy Function    +--------+    2. Validate    +--------+
|        |------------------------>|        |------------------>|        |
| User   |                         | API    |                   |Function|
|        |<------------------------| Gateway|<------------------| Service|
+--------+    8. Deployment Status +--------+    7. Status     +--------+
                                       |                           |
                                       |   3. Store Function       |
                                       v                           v
                                   +--------+    4. Function   +--------+
                                   |        |----------------->|        |
                                   |Function|                  |Function|
                                   | Store  |<-----------------| Runner |
                                   +--------+    5. Validation +--------+
                                       ^                           |
                                       |   6. Ready Status         |
                                       +---------------------------+
```

1. User submits function code for deployment
2. API Gateway forwards to Function Service for validation
3. Function code is stored in Function Store
4. Function is sent to Function Runner for validation
5. Function Runner validates the function
6. Function Runner reports ready status
7. Function Service reports deployment status
8. Deployment status is returned to the user

## Function Execution Flow

```
+--------+    1. Execute    +--------+    2. Request     +--------+
|        |---------------->|        |------------------>|        |
| Trigger|                 | API    |                   |Function|
|        |                 | Gateway|                   | Service|
+--------+                 +--------+                   +--------+
                                |                           |
                                |   3. Fetch Function       |
                                v                           v
                            +--------+    4. Function   +--------+
                            |        |----------------->|        |
                            |Function|                  | Enclave|
                            | Store  |                  |        |
                            +--------+                  +--------+
                                                            |
                                                            | 5. Execute
                                                            v
+--------+    8. Result     +--------+    7. Result     +--------+
|        |<----------------| API    |<-----------------| Function|
| Client |                 | Gateway|                  | Runner  |
|        |                 |        |                  |        |
+--------+                 +--------+                  +--------+
                                                            |
                                                            | 6. Access Resources
                                                            v
                                                        +--------+
                                                        |Resources|
                                                        | - Secrets|
                                                        | - Storage|
                                                        | - Neo N3 |
                                                        +--------+
```

1. Trigger (HTTP, Event, Schedule) initiates function execution
2. API Gateway forwards execution request to Function Service
3. Function Service fetches function code from Function Store
4. Function is sent to the enclave for secure execution
5. Function is executed in the enclave
6. Function accesses required resources (secrets, storage, blockchain)
7. Execution result is returned
8. Result is forwarded to the client

## Wallet Operation Flow

```
+--------+    1. Transaction Request    +--------+    2. Forward    +--------+
|        |--------------------------->|        |----------------->|        |
| User   |                            | API    |                  | Wallet |
|        |<----------------------------| Gateway|<-----------------| Service|
+--------+    8. Transaction Status   +--------+    7. Status    +--------+
                                          |                          |
                                          |   3. Request             |
                                          v                          v
                                      +--------+    4. Sign      +--------+
                                      |        |---------------->|        |
                                      | Wallet |                 | Enclave|
                                      | Data   |<----------------|        |
                                      +--------+    5. Signed    +--------+
                                          |                          |
                                          |   6. Submit              |
                                          v                          v
                                      +--------+                 +--------+
                                      |        |                 |        |
                                      | Neo N3 |                 | TX     |
                                      |        |                 | Monitor|
                                      +--------+                 +--------+
```

1. User submits transaction request
2. API Gateway forwards to Wallet Service
3. Wallet Service prepares transaction data
4. Transaction is sent to enclave for signing
5. Enclave returns signed transaction
6. Signed transaction is submitted to Neo N3 blockchain
7. Transaction status is reported
8. Status is returned to the user

## Price Feed Flow

```
+--------+    1. Fetch Prices    +--------+    2. Process    +--------+
|        |-------------------->|        |----------------->|        |
|Scheduler|                     |PriceFeed|                 | Enclave|
|        |                     | Service |                 |        |
+--------+                     +--------+                 +--------+
                                   |                          |
                                   |   3. Store Prices        |
                                   v                          v
                               +--------+    4. Sign      +--------+
                               |        |---------------->|        |
                               | Price  |                 | Wallet |
                               | Store  |<----------------|        |
                               +--------+    5. Signed    +--------+
                                   |                          |
                                   |   6. Submit              |
                                   v                          v
                               +--------+                 +--------+
                               |        |                 |        |
                               | Neo N3 |                 | Oracle |
                               | Oracle |                 | Monitor|
                               +--------+                 +--------+
```

1. Scheduler triggers price feed update
2. Price Feed Service fetches and processes price data
3. Price data is stored locally
4. Price data is signed in the enclave
5. Signed price data is prepared for submission
6. Price data is submitted to Neo N3 Oracle

## Event Monitoring Flow

```
+--------+    1. Monitor Events    +--------+    2. Process    +--------+
|        |---------------------->|        |----------------->|        |
| Event  |                        | Event  |                 | Event  |
| Source |                        | Monitor|                 |Processor|
+--------+                        +--------+                 +--------+
                                      |                          |
                                      |   3. Match Event         |
                                      v                          v
                                  +--------+    4. Trigger   +--------+
                                  |        |---------------->|        |
                                  | Event  |                 |Function|
                                  | Rules  |                 | Service|
                                  +--------+                 +--------+
                                                                 |
                                                                 | 5. Execute
                                                                 v
                                                             +--------+
                                                             |        |
                                                             |Function|
                                                             | Runner |
                                                             +--------+
```

1. Event Source (Neo N3, time-based, external) generates events
2. Event Monitor captures and processes events
3. Events are matched against event rules
4. Matching events trigger function execution
5. Functions are executed in response to events

## Secrets Management Flow

```
+--------+    1. Store Secret    +--------+    2. Process    +--------+
|        |-------------------->|        |----------------->|        |
| User   |                     | API    |                 | Secrets|
|        |                     | Gateway|                 | Service|
+--------+                     +--------+                 +--------+
                                   |                          |
                                   |   3. Encrypt             |
                                   v                          v
                               +--------+    4. Store     +--------+
                               |        |---------------->|        |
                               | Enclave|                 | Secrets|
                               |        |<----------------| Store  |
                               +--------+    5. Confirm   +--------+
                                   |                          |
                                   |   6. Access Control      |
                                   v                          v
                               +--------+                 +--------+
                               |        |                 |        |
                               |Function|                 | Access |
                               | Access |                 | Logs   |
                               +--------+                 +--------+
```

1. User submits secret for storage
2. API Gateway forwards to Secrets Service
3. Secret is encrypted in the enclave
4. Encrypted secret is stored
5. Storage is confirmed
6. Access control rules are established for the secret

## Data Storage Flow

```
+--------+    1. Store Data    +--------+    2. Process    +--------+
|        |------------------>|        |----------------->|        |
|Function|                   | Storage|                  | Storage|
|        |                   | API    |                  | Service|
+--------+                   +--------+                  +--------+
                                 |                           |
                                 |   3. Validate             |
                                 v                           v
                             +--------+    4. Store      +--------+
                             |        |------------------>|        |
                             | Access |                   | Data   |
                             | Control|<------------------| Store  |
                             +--------+    5. Confirm    +--------+
                                 |                           |
                                 |   6. Index                |
                                 v                           v
                             +--------+                  +--------+
                             |        |                  |        |
                             | Search |                  | Backup |
                             | Index  |                  | Service|
                             +--------+                  +--------+
```

1. Function requests data storage
2. Storage API processes the request
3. Access control validates the request
4. Data is stored
5. Storage is confirmed
6. Data is indexed for search and backed up

## Metrics Collection Flow

```
+--------+    1. Generate Metrics    +--------+    2. Process    +--------+
|        |------------------------>|        |----------------->|        |
| System |                         | Metrics|                  | Metrics|
|Component|                        | Agent  |                  |Collector|
+--------+                         +--------+                  +--------+
                                       |                           |
                                       |   3. Aggregate            |
                                       v                           v
                                   +--------+    4. Store      +--------+
                                   |        |------------------>|        |
                                   | Metrics|                   | Metrics|
                                   |Processor|<------------------| Store  |
                                   +--------+    5. Confirm    +--------+
                                       |                           |
                                       |   6. Alert                |
                                       v                           v
                                   +--------+                  +--------+
                                   |        |                  |        |
                                   | Alert  |                  |Dashboard|
                                   | System |                  | Service|
                                   +--------+                  +--------+
```

1. System components generate metrics
2. Metrics Agent collects and forwards metrics
3. Metrics are aggregated and processed
4. Processed metrics are stored
5. Storage is confirmed
6. Alerts are generated for anomalies and dashboards are updated
