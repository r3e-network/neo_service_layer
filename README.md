# Neo Service Layer

A serverless platform for Neo N3 blockchain with AWS Nitro Enclave support.

## Overview

Neo Service Layer is a comprehensive serverless platform designed for Neo N3 blockchain, providing chainlink-like services with AWS Nitro Enclave support. The system enables users to deploy and execute JavaScript, Python, and C# functions securely, with integrated blockchain interaction capabilities.

## Features

- User registration and account management
- Secure wallet management with encryption
- Secrets management with versioning and rotation
- Price feed integration with oracle contract submission
- Event monitoring for blockchain events
- Function execution in secure enclaves
- Storage capabilities for persistent data
- Metrics collection and monitoring

## Getting Started

### Prerequisites

- .NET 7.0 SDK
- Docker and Docker Compose
- AWS CLI (for Nitro Enclave support)
- SQL Server
- Redis

### Installation

#### Option 1: Using Docker Compose (Recommended)

1. Clone the repository
   ```bash
   git clone https://github.com/r3e-network/neo-service-layer.git
   cd neo-service-layer
   ```

2. Run the Docker Compose setup
   ```bash
   ./scripts/docker_setup_custom.sh
   ```

3. Access the services:
   - API: http://localhost:8080
   - Swagger UI: http://localhost:8081
   - Grafana: http://localhost:3001
   - Prometheus: http://localhost:9090
   - MailHog: http://localhost:8025

4. To stop the services
   ```bash
   ./scripts/docker_stop_custom.sh
   ```

#### Option 2: Manual Installation

1. Clone the repository
   ```bash
   git clone https://github.com/r3e-network/neo-service-layer.git
   cd neo-service-layer
   ```

2. Build the enclave application
   ```bash
   dotnet publish -c Release -o ./publish src/NeoServiceLayer.Enclave/NeoServiceLayer.Enclave.csproj
   ```

3. Build the API application
   ```bash
   dotnet publish -c Release -o ./publish src/NeoServiceLayer.Api/NeoServiceLayer.Api.csproj
   ```

4. Run the application
   ```bash
   dotnet run --project src/NeoServiceLayer.Api/NeoServiceLayer.Api.csproj
   ```

## Documentation

For detailed documentation, see the `docs` directory:

- [System Architecture](docs/system_architecture.md)
- [Data Flow](docs/data_flow.md)
- [Workflow](docs/workflow.md)
- [Implementation Guide](docs/implementation.md)
- [API Documentation](docs/api-documentation.md)
- [Deployment Guide](docs/deployment-guide.md)
- [Monitoring Guide](docs/monitoring-guide.md)
- [Security](docs/security.md)
- [Production Readiness](docs/production-readiness.md)
- [Future Enhancements](docs/future-enhancements.md)

## Docker Compose Setup

The Docker Compose setup includes the following services:

- **api**: The Neo Service Layer API with Function Service
- **mongodb**: MongoDB database for persistent storage
- **redis**: Redis for caching and pub/sub messaging
- **mailhog**: SMTP server for testing email notifications
- **prometheus**: Prometheus for metrics collection
- **grafana**: Grafana for metrics visualization
- **swagger-ui**: Swagger UI for API documentation

The setup is configured to use Docker volumes for persistent storage:

- **mongodb-data**: Storage for MongoDB data
- **redis-data**: Storage for Redis data
- **prometheus-data**: Storage for Prometheus data
- **grafana-data**: Storage for Grafana data

## Core Components

### Enclave Services

- **EnclaveAccountService**: Manages user accounts and authentication
- **EnclaveWalletService**: Manages wallets, keys, and blockchain transactions
- **EnclaveSecretsService**: Manages secrets with encryption, versioning, and rotation
- **EnclaveFunctionService**: Executes user-defined functions in a secure environment
- **EnclavePriceFeedService**: Fetches, aggregates, and submits price data to the blockchain

### Utilities

- **LoggingUtility**: Standardized logging across the application
- **EncryptionUtility**: Secure encryption and decryption operations
- **ValidationUtility**: Input validation and sanitization
- **JsonUtility**: JSON serialization and deserialization
- **MetricsUtility**: Performance and operational metrics collection
- **ExceptionUtility**: Standardized exception handling

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
