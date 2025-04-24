# Neo Service Layer Documentation

## Overview

Welcome to the Neo Service Layer documentation. This documentation provides comprehensive information about the Neo Service Layer, a serverless platform for Neo N3 blockchain with AWS Nitro Enclave support.

## Table of Contents

### 1. Introduction
- [Executive Summary](summary.md): High-level overview of the Neo Service Layer
- [System Architecture](system_architecture.md): Detailed system architecture and component descriptions

### 2. Technical Documentation
- [Data Flow](data_flow.md): Data flow diagrams and explanations
- [Workflow](workflow.md): Step-by-step workflows for various operations
- [Implementation Guide](implementation.md): Detailed implementation guidelines and technical specifications
- [Function Execution](function_execution.md): Details on function execution implementation and testing
- [TEE Integration](tee_integration.md): Technical details on Trusted Execution Environment integration

### 3. User Guides
- [Setup and Run Guide](setup_and_run.md): Instructions for setting up and running the Neo Service Layer

## Key Features

- **User Registration and Account Management**: Supports both Auth0 integration and Neo N3 account-based registration
- **Secure Wallet Management**: Manages service layer wallets for blockchain interaction within the enclave
- **Secrets Management**: Allows users to securely store and access private data with strict access controls
- **Price Feed Integration**: Fetches price data from various sources and submits to Neo N3 oracle smart contract
- **Event Monitoring**: Monitors Neo N3 events and other triggers to execute functions automatically
- **Function Execution**: Supports JavaScript, Python, and C# functions in a secure environment
- **Storage Capabilities**: Provides persistent storage for functions and services
- **Metrics Collection**: Tracks function execution, service performance, and resource usage

## System Components

1. **API Gateway**: Entry point for all external requests
2. **Service Orchestrator**: Coordinates interaction between services
3. **Account Service**: Manages user registration and authentication
4. **Wallet Service**: Handles blockchain wallet operations
5. **Secrets Service**: Provides secure storage and management of user secrets
6. **PriceFeed Service**: Fetches and submits price data
7. **Event Monitor**: Tracks blockchain events and triggers
8. **Function Runner**: Executes user-deployed functions
9. **Storage Service**: Provides persistent storage capabilities
10. **Metrics Service**: Collects and reports system metrics

## Security Architecture

The Neo Service Layer leverages AWS Nitro Enclaves to provide a secure, isolated execution environment for sensitive operations:

- **Isolated Memory**: Enclave memory is encrypted and isolated from the parent instance
- **No Persistent Storage**: Enclaves have no persistent storage, enhancing security
- **No Interactive Access**: No SSH or other interactive access to the enclave
- **Cryptographic Attestation**: Provides proof of the enclave's identity and integrity
- **Secure Communication**: VSOCK-based communication between parent instance and enclave

## Getting Started

To get started with the Neo Service Layer, follow these steps:

1. Set up the development environment as described in the [Setup and Run Guide](setup_and_run.md)
2. Create the project structure using the provided script
3. Build and run the application
4. Deploy to AWS with Nitro Enclave support for production use

## Contributing

We welcome contributions to the Neo Service Layer. Please follow these steps to contribute:

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.
