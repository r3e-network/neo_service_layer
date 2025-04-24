#!/bin/bash

# Script to build the Neo Service Layer project
echo "Building Neo Service Layer project..."

# Navigate to project root
cd "$(dirname "$0")/.."
PROJECT_ROOT=$(pwd)

echo "Project root: $PROJECT_ROOT"

# Restore dependencies
echo "Restoring dependencies..."
dotnet restore

# Build the solution
echo "Building solution..."
dotnet build --no-restore

echo "Build completed!"
