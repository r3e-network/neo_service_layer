#!/bin/bash

# Script to start the development environment
echo "Starting Neo Service Layer development environment..."

# Navigate to project root
cd "$(dirname "$0")/.."
PROJECT_ROOT=$(pwd)

echo "Project root: $PROJECT_ROOT"

# Stop any running containers
echo "Stopping any running containers..."
docker-compose -f docker-compose.dev.yml down

# Start the development environment
echo "Starting the development environment..."
docker-compose -f docker-compose.dev.yml up -d

# Wait for services to start
echo "Waiting for services to start..."
sleep 5

# Check if services are running
echo "Checking if services are running..."
docker-compose -f docker-compose.dev.yml ps

echo "Development environment started!"
echo "API is available at http://localhost:8080"
echo "Swagger UI is available at http://localhost:8081"
echo "Documentation is available at http://localhost:8000"
echo "Grafana is available at http://localhost:3001"
echo "Prometheus is available at http://localhost:9090"
echo "MailHog is available at http://localhost:8025"
echo "MongoDB is available at localhost:27017"
echo "Redis is available at localhost:6379"
