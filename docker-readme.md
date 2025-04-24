# Neo Service Layer Docker Setup

This document provides instructions for setting up and running the Neo Service Layer using Docker.

## Prerequisites

- Docker: [Install Docker](https://www.docker.com/get-started)
- Docker Compose: [Install Docker Compose](https://docs.docker.com/compose/install/)

## Quick Start

### Full Setup

The easiest way to set up and run the complete Neo Service Layer is to use the provided setup script:

```bash
./scripts/docker_setup.sh
```

This script will:
1. Create the necessary runtime files for JavaScript, Python, and .NET
2. Update the MongoDB initialization script
3. Build and start all the Docker containers

### Simple Setup

If you're experiencing network issues or just want to run the essential services, you can use the simplified setup script:

```bash
./scripts/docker_setup_simple.sh
```

This script will:
1. Build and start only the essential Docker containers (MongoDB, Redis, and API)
2. Configure the API to run without the enclave

### Minimal Setup

If you only need the database and cache services, you can use the minimal setup script:

```bash
./scripts/docker_setup_minimal.sh
```

This script will:
1. Build and start only the MongoDB and Redis containers
2. This is useful for development when you want to run the API and other services directly on your machine

## Manual Setup

If you prefer to set up the services manually, follow these steps:

### 1. Create Runtime Files

Create the necessary runtime files for JavaScript, Python, and .NET:

```bash
# Create JavaScript runtime files
mkdir -p src/NeoServiceLayer.Enclave/Enclave/Execution/JavaScript
# Create package.json and runtime.js files

# Create Python runtime files
mkdir -p src/NeoServiceLayer.Enclave/Enclave/Execution/Python
# Create requirements.txt and runtime.py files

# Create .NET runtime files
mkdir -p src/NeoServiceLayer.Enclave/Enclave/Execution/DotNet
# Create Runtime.cs and DotNetRuntime.csproj files
```

### 2. Build and Start the Services

Build and start all the Docker containers:

```bash
docker-compose build
docker-compose up -d
```

## Running the API Directly on the Host Machine

If you're developing the API and want to run it directly on your host machine, you can use the minimal Docker setup to run only the MongoDB and Redis services:

```bash
./scripts/docker_setup_minimal.sh
```

Then, you can run the API directly on your host machine:

```bash
cd src/NeoServiceLayer.Api
dotnet run
```

Make sure to update the appsettings.json file to point to the Docker containers:

```json
{
  "ConnectionStrings": {
    "MongoDB": "mongodb://localhost:27017/neo_service_layer",
    "Redis": "localhost:6379"
  }
}
```

## Services

The Neo Service Layer consists of the following services:

### Core Services

- **API**: The main API service that provides the REST API for the Neo Service Layer
  - URL: http://localhost:8080
  - Swagger UI: http://localhost:8081 (Full setup only)

- **Enclave**: The enclave service that runs the secure operations (Full setup only)
  - In a real AWS Nitro Enclave environment, this would be replaced with the actual enclave setup
  - For local development, it's simulated as a regular container

### Function Runtime Services (Full setup only)

- **JavaScript Runtime**: Executes JavaScript functions
- **Python Runtime**: Executes Python functions
- **DotNet Runtime**: Executes .NET functions

### Database and Cache Services

- **MongoDB**: The database service
  - Port: 27017

- **Redis**: The cache service
  - Port: 6379

### Support Services (Full setup only)

- **MailHog**: A development SMTP server for testing email functionality
  - SMTP Port: 1025
  - Web UI: http://localhost:8025

- **Prometheus**: Metrics collection and storage
  - URL: http://localhost:9090

- **Grafana**: Metrics visualization and dashboards
  - URL: http://localhost:3000
  - Default credentials: admin/admin

## Configuration

The Neo Service Layer is configured using the `docker-appsettings.json` file. This file is mounted into the API and Enclave containers.

## Logs

To view the logs of all services:

```bash
docker-compose logs -f
```

To view the logs of a specific service:

```bash
docker-compose logs -f <service-name>
```

For example:

```bash
docker-compose logs -f api
```

## Stopping the Services

### Full Setup

To stop all services in the full setup:

```bash
docker-compose down
```

To stop all services and remove all data:

```bash
docker-compose down -v
```

### Simple Setup

To stop all services in the simple setup:

```bash
docker-compose -f docker-compose.simple.yml down
```

To stop all services and remove all data:

```bash
docker-compose -f docker-compose.simple.yml down -v
```

### Minimal Setup

To stop all services in the minimal setup:

```bash
docker-compose -f docker-compose.minimal.yml down
```

To stop all services and remove all data:

```bash
docker-compose -f docker-compose.minimal.yml down -v
```

## Troubleshooting

### Services Not Starting

If a service fails to start, check the logs:

```bash
docker-compose logs <service-name>
```

### Database Connection Issues

If the API service can't connect to the database, make sure the MongoDB service is running:

```bash
docker-compose ps mongodb
```

If MongoDB is running but the API still can't connect, check the MongoDB logs:

```bash
docker-compose logs mongodb
```

### Function Execution Issues

If function execution fails, check the logs of the corresponding runtime service:

```bash
docker-compose logs js-runtime
docker-compose logs python-runtime
docker-compose logs dotnet-runtime
```
