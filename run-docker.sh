#!/bin/bash

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "Warning: Docker is not running."
    echo "Please start Docker Desktop and then run this script again."
    echo "On macOS: Open Docker Desktop from the Applications folder"
    echo "On Windows: Open Docker Desktop from the Start menu"
    echo "On Linux: Run 'sudo systemctl start docker'"
    exit 1
fi

# Stop any running containers
echo "Stopping any running containers..."
docker-compose down

# Build the containers
echo "Building containers..."
docker-compose build

# Start the containers
echo "Starting containers..."
docker-compose up -d

# Wait for services to start
echo "Waiting for services to start..."
sleep 10

# Check if services are running
echo "Checking if services are running..."
docker-compose ps

# Show logs
echo "Showing logs..."
docker-compose logs --tail=20

echo ""
echo "Neo Service Layer is now running!"
echo "MongoDB is available at: localhost:27017"
echo "Redis is available at: localhost:6379"
echo ""
echo "To view logs, run: docker-compose logs -f"
echo "To stop the services, run: docker-compose down"
echo ""
echo "Once the basic services are working, you can uncomment the api, enclave, and mailhog services in docker-compose.yml to run the full stack."
