# Neo Service Layer - Services

This directory contains the core services of the Neo Service Layer:

- `pricefeed/`: Price feed service for publishing oracle data
- `gasbank/`: Gas management service for user operations
- `trigger/`: Event monitoring and automated function execution
- `metrics/`: System and contract performance monitoring
- `logging/`: Centralized logging service
- `secrets/`: Secure secrets management
- `functions/`: User-defined function management
- `api/`: RESTful API service

Each service follows a consistent structure:
```
service/
├── docs/           # Service-specific documentation
├── internal/       # Internal implementation
├── models/         # Data models
├── handlers/       # Request handlers
├── middleware/     # Service middleware
├── store/         # Data storage
└── tests/         # Service tests
```

## Services

The NEO Service Layer provides the following services:

- **Account**: User account management and related functionality.
- **API**: REST API endpoints for client interaction.
- **Automation**: Contract automation and upkeep scheduling.
- **Functions**: Serverless function execution and management.
- **GasBank**: GAS management for service operations.
- **Logging**: Centralized logging for all services.
- **Metrics**: Monitoring and performance metrics collection.
- **PriceFeed**: Oracle data feeds for NEO blockchain.
- **Secrets**: Secure secret storage and management.
- **Trigger**: Event monitoring and triggers for function execution.
- **Transaction**: NEO blockchain transaction handling.
- **Wallet**: Centralized wallet management for all services.

Each service is designed to be independent but can work together with other services when needed.