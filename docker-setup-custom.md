# Neo Service Layer Docker Setup - Custom

This document provides instructions for setting up and running the Neo Service Layer using Docker with a custom configuration.

## Overview

The Neo Service Layer Docker setup includes the following services:

- **Core Services**:
  - API: The main API service for the Neo Service Layer
  - MongoDB: The database service for the Neo Service Layer
  - Redis: The cache service for the Neo Service Layer

- **Support Services**:
  - MailHog: A development mail server for testing email functionality
  - Prometheus: A monitoring service for collecting metrics
  - Grafana: A visualization service for displaying metrics
  - Swagger UI: A documentation service for the API

## Prerequisites

- Docker
- Docker Compose

## Setup

1. Clone the repository:

```bash
git clone https://github.com/your-username/neo-service-layer.git
cd neo-service-layer
```

2. Run the setup script:

```bash
./scripts/docker_setup_custom.sh
```

This will start all the services in Docker containers.

## Services

The following services are available:

- API: http://localhost:8080
- Swagger UI: http://localhost:8081
- Grafana: http://localhost:3001
- Prometheus: http://localhost:9090
- MailHog: http://localhost:8025
- MongoDB: localhost:27017
- Redis: localhost:6379

## Configuration

The Docker setup uses the following configuration files:

- **custom/docker-appsettings.custom.json**: Configuration for the API service
- **custom/Dockerfile.api.custom**: Dockerfile for the API service
- **custom/FunctionServiceExtensions.cs**: Custom implementation of the FunctionServiceExtensions class
- **docker-compose.custom.yml**: Docker Compose file for the Neo Service Layer

## Scripts

The Docker setup includes the following scripts:

- **scripts/docker_setup_custom.sh**: Sets up and runs the Neo Service Layer using Docker
- **scripts/docker_stop_custom.sh**: Stops the Neo Service Layer

## Troubleshooting

If you encounter issues with the Docker setup, check the following:

1. Make sure Docker and Docker Compose are installed and running
2. Check the logs of the services using `docker-compose -f docker-compose.custom.yml logs <service-name>`
3. Make sure the configuration files are correct
4. Try stopping and starting the services again

## Next Steps

To further improve the Docker setup, consider the following:

1. Add support for AWS Nitro Enclave in the Docker setup
2. Add support for running the Neo Service Layer in a production environment
3. Add support for running the Neo Service Layer in a Kubernetes cluster
4. Add support for running the Neo Service Layer in a cloud environment (AWS, Azure, GCP)
