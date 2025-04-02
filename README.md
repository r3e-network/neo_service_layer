# Neo N3 Service Layer

A Go-based service layer for interacting with the Neo N3 blockchain.

## Features

- **Neo RPC Client**: Interact with Neo N3 blockchain nodes
- **Wallet Management**: Handle wallet operations and transaction signing
- **Trigger Service**: Execute smart contract calls and handle automation
- **Mock Services**: Development-friendly mock implementations

## Documentation

- [Usage Guide](docs/usage-guide.md): How to use the Neo service layer
- [Neo Client](docs/neo-client.md): Details about the Neo client implementation
- **Wallet Service**: Documentation for wallet operations

## Getting Started

### Prerequisites

- Go 1.18+
- Neo N3 wallet (for production use)

### Installation

```bash
git clone https://github.com/your-org/neo_service_layer.git
cd neo_service_layer
go mod tidy
```

### Basic Usage

```go
package main

import (
    "log"
    
    "github.com/your-org/neo_service_layer/internal/core/neo"
    "github.com/your-org/neo_service_layer/internal/services/wallet"
)

func main() {
    // Create Neo client
    neoClient, err := neo.NewClient(nil) // Uses default config
    if err != nil {
        log.Fatalf("Failed to create Neo client: %v", err)
    }
    
    // Create wallet service
    walletService := wallet.NewWalletService(nil) // Uses default config
    
    // Use the services for blockchain operations
    blockCount, err := neoClient.GetBlockCount()
    if err != nil {
        log.Fatalf("Failed to get block count: %v", err)
    }
    
    log.Printf("Current block height: %d", blockCount)
}
```

## Architecture

This service layer follows a modular architecture:

- **Core**: Low-level blockchain interactions
- **Services**: Business logic and operations
- **Models**: Data structures and types
- **Utils**: Helper functions and utilities

## Development

For development without a real Neo blockchain:

```go
// Use mock clients instead of real ones
neoClient := neo.NewMockClient()
walletService := wallet.NewMockWalletService()
```

## License

[MIT License](LICENSE)

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.