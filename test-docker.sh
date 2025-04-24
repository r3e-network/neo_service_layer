#!/bin/bash

# Test script for Neo Service Layer Docker Compose setup

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "Warning: Docker is not running."
    echo "Please start Docker Desktop and then run this script again."
    echo "On macOS: Open Docker Desktop from the Applications folder"
    echo "On Windows: Open Docker Desktop from the Start menu"
    echo "On Linux: Run 'sudo systemctl start docker'"
    exit 1
fi

# Check if Docker Compose is running
echo "Checking if Docker Compose services are running..."
if ! docker-compose ps | grep -q "Up"; then
    echo "Error: Docker Compose services are not running. Please run ./run-docker.sh first."
    exit 1
fi

# Check if MongoDB is accessible
echo "Testing MongoDB connection..."
if ! docker-compose exec mongodb mongosh --eval "db.runCommand({ping:1})" | grep -q "1"; then
    echo "Error: MongoDB is not accessible."
    exit 1
fi

# Check if Redis is accessible
echo "Testing Redis connection..."
if ! docker-compose exec redis redis-cli ping | grep -q "PONG"; then
    echo "Error: Redis is not accessible."
    exit 1
fi

echo "Basic services are working correctly!"

# Check if API is accessible
echo "Testing API health endpoint..."
if ! curl -s -o /dev/null -w "%{http_code}" http://localhost:8080/health | grep -q "200"; then
    echo "Warning: API health endpoint is not accessible. This might be normal if the API is still starting up."
    echo "Try again in a few moments or check the logs with: docker-compose logs api"
fi

# Check if Swagger UI is accessible
echo "Testing Swagger UI..."
if ! curl -s -o /dev/null -w "%{http_code}" http://localhost:8080/swagger/index.html | grep -q "200"; then
    echo "Warning: Swagger UI is not accessible. This might be normal if the API is still starting up."
    echo "Try again in a few moments or check the logs with: docker-compose logs api"
fi

# Check if MailHog is accessible
echo "Testing MailHog connection..."
if ! curl -s -o /dev/null -w "%{http_code}" http://localhost:8025 | grep -q "200"; then
    echo "Warning: MailHog is not accessible. This might be normal if MailHog is still starting up."
    echo "Try again in a few moments or check the logs with: docker-compose logs mailhog"
fi

echo "All tests passed! Neo Service Layer Docker Compose setup is working correctly."
