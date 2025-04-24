#!/bin/bash

echo "Starting Docker..."
echo ""
echo "Please note that Docker needs to be installed and running on your system."
echo ""
echo "If Docker is not installed, please install it from https://www.docker.com/products/docker-desktop"
echo ""
echo "If Docker is installed but not running:"
echo "  - On macOS: Open Docker Desktop from the Applications folder"
echo "  - On Windows: Open Docker Desktop from the Start menu"
echo "  - On Linux: Run 'sudo systemctl start docker'"
echo ""
echo "Once Docker is running, you can run ./run-docker.sh to start the Neo Service Layer."
echo ""

# Check if Docker is running
if docker info > /dev/null 2>&1; then
    echo "Docker is already running. You can now run ./run-docker.sh to start the Neo Service Layer."
else
    echo "Docker is not running. Please start Docker and then run ./run-docker.sh."
fi
