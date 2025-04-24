# Neo Service Layer Docker Setup Summary

This document provides a summary of the Docker setup for the Neo Service Layer project.

## Docker Setup Options

We've created three different Docker setup options to accommodate different use cases:

### 1. Full Setup

The full setup includes all the services required for the Neo Service Layer:

- Core Services: API and Enclave
- Function Runtime Services: JavaScript, Python, and .NET
- Database and Cache Services: MongoDB and Redis
- Support Services: MailHog, Prometheus, and Grafana
- Development Tools: Swagger UI

To use the full setup:

```bash
./scripts/docker_setup.sh
```

### 2. Simple Setup

The simple setup includes only the essential services:

- Core Services: API (without enclave)
- Database and Cache Services: MongoDB and Redis

To use the simple setup:

```bash
./scripts/docker_setup_simple.sh
```

### 3. Minimal Setup

The minimal setup includes only the database and cache services:

- Database and Cache Services: MongoDB and Redis

This is useful for development when you want to run the API and other services directly on your host machine.

To use the minimal setup:

```bash
./scripts/docker_setup_minimal.sh
```

## Docker Compose Files

We've created three different Docker Compose files to support the different setup options:

### 1. docker-compose.yml

This file includes all the services required for the Neo Service Layer.

### 2. docker-compose.simple.yml

This file includes only the essential services: API, MongoDB, and Redis.

### 3. docker-compose.minimal.yml

This file includes only the database and cache services: MongoDB and Redis.

## Configuration

The Docker setup uses the following configuration files:

- **docker-appsettings.json**: Configuration for the API and Enclave services
- **init-mongodb.sh**: Initialization script for MongoDB
- **prometheus.yml**: Configuration for Prometheus
- **grafana/provisioning/datasources/prometheus.yml**: Configuration for Grafana datasources
- **grafana/provisioning/dashboards/dashboards.yml**: Configuration for Grafana dashboards
- **grafana/dashboards/neo-service-layer.json**: Sample dashboard for Neo Service Layer

## Scripts

We've created several scripts to help with the Docker setup:

- **docker_setup.sh**: Sets up and runs the complete Neo Service Layer using Docker
- **docker_setup_simple.sh**: Sets up and runs the essential Neo Service Layer services using Docker
- **docker_setup_minimal.sh**: Sets up and runs only the MongoDB and Redis services using Docker

## Documentation

We've created comprehensive documentation for the Docker setup:

- **docker-readme.md**: Provides instructions for setting up and running the Neo Service Layer using Docker
- **docker-setup-summary.md**: Provides a summary of the Docker setup for the Neo Service Layer project

## Troubleshooting

If you encounter issues with the Docker setup, check the following:

1. Make sure Docker and Docker Compose are installed and running
2. Check the logs of the services using `docker-compose logs <service-name>`
3. Make sure the configuration files are correct
4. Try using a different setup option (full, simple, or minimal)

## Current Status

We've successfully set up the minimal Docker environment with MongoDB and Redis services. However, we're encountering issues with the API service due to dependencies on the Function service. We need to modify the API service to properly disable the Function service.

The minimal setup is working correctly, allowing you to run MongoDB and Redis in Docker containers while running the API and other services directly on your host machine. This is useful for development and testing.

## Next Steps

To further improve the Docker setup, consider the following:

1. Fix the API service to properly disable the Function service
2. Add support for AWS Nitro Enclave in the Docker setup
3. Add support for running the Neo Service Layer in a production environment
4. Add support for running the Neo Service Layer in a Kubernetes cluster
5. Add support for running the Neo Service Layer in a cloud environment (AWS, Azure, GCP)
