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