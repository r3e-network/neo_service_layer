#!/bin/bash

# Neo Service Layer Run Script
# This script runs the Neo Service Layer services

# Exit on error
set -e

# Set the base directory to the current directory
BASE_DIR=$(pwd)

echo "Running Neo Service Layer services..."

# Check if the services are built
if [ ! -d "$BASE_DIR/dist/api" ] || [ ! -d "$BASE_DIR/dist/enclave" ]; then
    echo "Error: Services are not built. Please run the build script first:"
    echo "  ./scripts/build_services.sh"
    exit 1
fi

# Check if Docker is running
if command -v docker &> /dev/null; then
    if ! docker info &> /dev/null; then
        echo "Warning: Docker is not running. Please start Docker to run the database and Redis services."
    else
        # Start the database and Redis services
        echo "Starting database and Redis services..."
        docker-compose up -d db redis
        
        # Wait for services to be ready
        echo "Waiting for services to be ready..."
        sleep 5
    fi
fi

# Run mode (api, enclave, or all)
RUN_MODE=${1:-all}

# Run the API service
run_api() {
    echo "Starting API service..."
    cd "$BASE_DIR/dist/api"
    
    # Copy the configuration file if it exists
    if [ -f "$BASE_DIR/config/appsettings.Development.json" ]; then
        cp "$BASE_DIR/config/appsettings.Development.json" ./appsettings.Development.json
    fi
    
    # Run the API service
    ASPNETCORE_ENVIRONMENT=Development dotnet NeoServiceLayer.Api.dll &
    API_PID=$!
    echo "API service started with PID $API_PID"
}

# Run the Enclave service
run_enclave() {
    echo "Starting Enclave service..."
    cd "$BASE_DIR/dist/enclave"
    
    # Copy the configuration file if it exists
    if [ -f "$BASE_DIR/config/appsettings.Development.json" ]; then
        cp "$BASE_DIR/config/appsettings.Development.json" ./appsettings.Development.json
    fi
    
    # Check if Nitro Enclave is enabled
    ENCLAVE_ENABLED=$(grep -o '"Enabled": true' ./appsettings.Development.json || echo "")
    
    if [ -n "$ENCLAVE_ENABLED" ]; then
        echo "Nitro Enclave is enabled. Starting Enclave service in Nitro Enclave..."
        # TODO: Add Nitro Enclave specific startup commands
        echo "Warning: Nitro Enclave support is not fully implemented yet."
    fi
    
    # Run the Enclave service
    ASPNETCORE_ENVIRONMENT=Development dotnet NeoServiceLayer.Enclave.dll &
    ENCLAVE_PID=$!
    echo "Enclave service started with PID $ENCLAVE_PID"
}

# Run the services based on the run mode
case $RUN_MODE in
    api)
        run_api
        ;;
    enclave)
        run_enclave
        ;;
    all)
        run_api
        run_enclave
        ;;
    *)
        echo "Error: Invalid run mode. Valid modes are: api, enclave, all"
        exit 1
        ;;
esac

# Wait for user input to stop the services
echo ""
echo "Services are running. Press Ctrl+C to stop."
echo ""

# Handle Ctrl+C to stop the services gracefully
trap 'echo "Stopping services..."; kill $API_PID $ENCLAVE_PID 2>/dev/null || true; echo "Services stopped."; exit 0' INT

# Wait for the services to exit
wait
