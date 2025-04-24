#!/bin/bash

# Neo Service Layer Build Script
# This script builds all the services in the Neo Service Layer project

# Exit on error
set -e

# Set the base directory to the current directory
BASE_DIR=$(pwd)

echo "Building Neo Service Layer services..."

# Build configuration (Debug or Release)
BUILD_CONFIG=${1:-Debug}
echo "Build configuration: $BUILD_CONFIG"

# Clean the solution
echo "Cleaning solution..."
dotnet clean "$BASE_DIR/src/NeoServiceLayer.sln" --configuration $BUILD_CONFIG

# Restore NuGet packages
echo "Restoring NuGet packages..."
dotnet restore "$BASE_DIR/src/NeoServiceLayer.sln"

# Build the solution
echo "Building solution..."
dotnet build "$BASE_DIR/src/NeoServiceLayer.sln" --configuration $BUILD_CONFIG --no-restore

# Build the API project
echo "Building API project..."
dotnet publish "$BASE_DIR/src/NeoServiceLayer.Api/NeoServiceLayer.Api.csproj" --configuration $BUILD_CONFIG --no-build --output "$BASE_DIR/dist/api"

# Build the Enclave Host project
echo "Building Enclave Host project..."
dotnet publish "$BASE_DIR/src/NeoServiceLayer.Enclave/NeoServiceLayer.Enclave.csproj" --configuration $BUILD_CONFIG --no-build --output "$BASE_DIR/dist/enclave"

echo "Build completed successfully!"
echo "The built services are available in the dist directory:"
echo "  - API: $BASE_DIR/dist/api"
echo "  - Enclave: $BASE_DIR/dist/enclave"
echo ""
echo "You can run the services using the following command:"
echo "  ./scripts/run_services.sh"
