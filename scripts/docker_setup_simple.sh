#!/bin/bash

# Neo Service Layer Simple Docker Setup Script
# This script sets up and runs the essential Neo Service Layer services using Docker

# Exit on error
set -e

# Set the base directory to the current directory
BASE_DIR=$(pwd)

echo "Setting up Neo Service Layer using Docker (Simple Version)..."

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "Error: Docker is not installed. Please install Docker from https://www.docker.com/get-started"
    exit 1
fi

# Check if Docker Compose is installed
if ! command -v docker-compose &> /dev/null; then
    echo "Error: Docker Compose is not installed. Please install Docker Compose from https://docs.docker.com/compose/install/"
    exit 1
fi

# Check if Docker is running
if ! docker info &> /dev/null; then
    echo "Error: Docker is not running. Please start Docker and try again."
    exit 1
fi

# Make the init-mongodb.sh script executable
chmod +x "$BASE_DIR/init-mongodb.sh"

# Build and start the Docker containers
echo "Building and starting Docker containers..."
docker-compose -f docker-compose.simple.yml build
docker-compose -f docker-compose.simple.yml up -d

echo "Neo Service Layer Docker setup (Simple Version) completed successfully!"
echo ""
echo "The following services are now running:"
echo "  - API: http://localhost:8080"
echo "  - MongoDB: localhost:27017"
echo "  - Redis: localhost:6379"
echo ""
echo "You can view the logs with:"
echo "  docker-compose -f docker-compose.simple.yml logs -f"
echo ""
echo "To stop the services, run:"
echo "  docker-compose -f docker-compose.simple.yml down"
