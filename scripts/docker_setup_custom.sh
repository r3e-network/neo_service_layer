#!/bin/bash

# Set the script to exit on error
set -e

# Print a message to the console
echo "Setting up Neo Service Layer using Docker..."

# Change to the project root directory
cd "$(dirname "$0")/.."

# Create necessary directories
mkdir -p custom
mkdir -p Templates

# Stop any running containers
echo "Stopping any running containers..."
docker-compose -f docker-compose.custom.yml down || true

# Start the services
echo "Starting the services..."
docker-compose -f docker-compose.custom.yml up -d

# Wait for the services to start
echo "Waiting for the services to start..."
sleep 10

# Check if the services are running
echo "Checking if the services are running..."
docker-compose -f docker-compose.custom.yml ps

# Print a success message
echo "Neo Service Layer setup completed!"
echo "API is available at http://localhost:8080"
echo "Swagger UI is available at http://localhost:8081"
echo "Grafana is available at http://localhost:3001"
echo "Prometheus is available at http://localhost:9090"
echo "MailHog is available at http://localhost:8025"
echo "MongoDB is available at localhost:27017"
echo "Redis is available at localhost:6379"
