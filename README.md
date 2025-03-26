# Neo Service Layer

## Overview

Neo Service Layer is a comprehensive middleware platform designed to provide Chainlink-like functionality for the Neo N3 blockchain. It offers a suite of services that enable developers to create, deploy, and manage decentralized applications with enhanced capabilities such as external data integration, automated task execution, and secure function management.

## Core Services

### Functions Service

Manages serverless functions that can be executed in a Trusted Execution Environment (TEE):

- Function creation, execution, and management
- Support for JavaScript and other runtime environments
- Permission-based access control
- Resource usage limits and monitoring

### Secrets Service

Securely stores and manages sensitive information:

- Encrypted storage of sensitive credentials and keys
- Fine-grained access control for functions and users
- Automatic secret rotation and expiration
- Integration with TEE for secure access

### Gas Bank Service

Manages GAS allocation and usage for operations:

- Gas allocation for user operations
- Automated refill mechanisms
- Usage tracking and optimization
- Balance management and reporting

### Price Feed Service

Provides reliable price data for various assets:

- Collection of price data from multiple sources
- Validation and aggregation of data points
- On-chain publication of verified price data
- Historical price data storage and access

### Trigger Service

Enables event-driven automation:

- Conditional trigger execution based on blockchain events
- Schedule-based execution using CRON expressions
- Integration with Functions service for action execution
- Execution history and monitoring

### Metrics Service

Collects and reports on system performance and usage:

- Real-time metrics collection from all services
- Prometheus integration for monitoring
- Customizable dashboards for visualization
- Service health monitoring and alerting
- Historical data storage and analysis

## Supporting Services

### API Service

Provides a RESTful interface for external applications:

- Unified access to all services
- Signature-based authentication
- Rate limiting and security features
- Comprehensive endpoint documentation

### Logging Service

Centralizes log collection and analysis:

- Aggregated logs from all services
- Structured logging format
- Query interface for log analysis
- Retention policies and storage management

## Architecture

The Neo Service Layer follows a microservices architecture pattern, with distinct services that handle specific functionalities while communicating through well-defined interfaces. This design allows for better scalability, maintainability, and resilience.

For detailed architecture information, see [Architecture Overview](docs/architecture/architecture-overview.md).

## Documentation

- [Architecture Overview](docs/architecture/architecture-overview.md)
- [API Integration Guide](docs/api-integration.md)
- [Neo Blockchain Integration](docs/neo-integration.md)
- [Functions Runtime](docs/functions-runtime.md)
- [Gas Bank Service](docs/gasbank-service.md)
- [Trigger Service](docs/trigger-service.md)
- [Metrics Service](docs/metrics-service.md)
- [Integration Tests Documentation](docs/integration-tests.md)

## Getting Started

### Prerequisites

- Go 1.18 or higher
- Docker and Docker Compose (for local development)
- Neo N3 node access (local or remote)
- PostgreSQL 13 or higher (for production deployment)

### Local Development Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/will/neo_service_layer.git
   cd neo_service_layer
   ```

2. Install dependencies:
   ```bash
   go mod download
   ```

3. Run tests:
   ```bash
   go test ./...
   ```

4. Start the services locally:
   ```bash
   make run
   ```

### Configuration

The Neo Service Layer can be configured using environment variables or configuration files. See `config/examples` for sample configurations.

Key configuration files:
- `config/app.yaml`: Main application configuration
- `config/services/*.yaml`: Service-specific configurations

### Deployment

For production deployment instructions, see [Deployment Guide](docs/deployment.md).

## Development

### Project Structure

```
├── cmd/                  # Command-line applications
├── config/               # Configuration files
├── docs/                 # Documentation
├── internal/             # Internal packages
│   ├── common/           # Shared utilities
│   └── services/         # Service implementations
├── pkg/                  # Public API packages
├── scripts/              # Build and deployment scripts
└── tests/                # Integration and end-to-end tests
```

### Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/my-new-feature`)
3. Commit your changes (`git commit -m 'Add some feature'`)
4. Push to the branch (`git push origin feature/my-new-feature`)
5. Create a new Pull Request

## License

[MIT License](LICENSE)

## Contact

For questions or support, please contact the team at [will@neo_service_layer.com](mailto:will@neo_service_layer.com).
