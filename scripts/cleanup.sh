#!/bin/bash

# Script to clean up the project for GitHub release
echo "Cleaning up Neo Service Layer project for GitHub release..."

# Navigate to project root
cd "$(dirname "$0")/.."
PROJECT_ROOT=$(pwd)

echo "Project root: $PROJECT_ROOT"

# Clean up build artifacts
echo "Removing build artifacts..."
find . -type d -name "bin" -o -name "obj" | xargs rm -rf

# Clean up temporary files
echo "Removing temporary files..."
find . -type f -name "*.tmp" -o -name "*.log" -o -name "*.bak" -o -name "*.cache" | xargs rm -f

# Clean up IDE specific files
echo "Removing IDE specific files..."
rm -rf .idea .vscode .vs .DS_Store **/.DS_Store

# Clean up cursor files
echo "Removing cursor files..."
rm -rf .cursor

# Clean up Docker volumes and containers (optional)
read -p "Do you want to clean up Docker containers and volumes? (y/n) " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]
then
    echo "Stopping and removing Docker containers..."
    docker-compose -f docker-compose.yml down -v 2>/dev/null || true
    docker-compose -f docker-compose.custom.yml down -v 2>/dev/null || true
    
    echo "Removing Docker images..."
    docker rmi neo_service_layer-api 2>/dev/null || true
fi

echo "Cleanup completed successfully!"
