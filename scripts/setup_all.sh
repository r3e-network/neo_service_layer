#!/bin/bash

# Neo Service Layer Master Setup Script
# This script runs all the setup scripts in the correct order

# Exit on error
set -e

# Set the base directory to the current directory
BASE_DIR=$(pwd)

echo "Setting up Neo Service Layer..."

# Make all scripts executable
chmod +x "$BASE_DIR/scripts/"*.sh

# Create project structure (if needed)
if [ ! -d "$BASE_DIR/src" ]; then
    echo "Creating project structure..."
    "$BASE_DIR/scripts/create_project_structure.sh"
fi

# Set up development environment
echo "Setting up development environment..."
"$BASE_DIR/scripts/setup_environment.sh"

# Set up JavaScript runtime
echo "Setting up JavaScript runtime..."
"$BASE_DIR/scripts/setup_js_runtime.sh"

# Build services
echo "Building services..."
"$BASE_DIR/scripts/build_services.sh"

# Run tests
echo "Running tests..."
"$BASE_DIR/scripts/run_tests.sh"

echo "Neo Service Layer setup completed successfully!"
echo "You can now run the services using the following command:"
echo "  ./scripts/run_services.sh"
