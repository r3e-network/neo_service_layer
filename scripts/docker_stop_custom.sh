#!/bin/bash

# Set the script to exit on error
set -e

# Print a message to the console
echo "Stopping Neo Service Layer..."

# Change to the project root directory
cd "$(dirname "$0")/.."

# Stop the services
echo "Stopping the services..."
docker-compose -f docker-compose.custom.yml down

# Print a success message
echo "Neo Service Layer stopped!"
