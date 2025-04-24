#!/bin/bash

# Script to update documentation
# Usage: ./scripts/update_docs.sh

# Navigate to project root
cd "$(dirname "$0")/.."
PROJECT_ROOT=$(pwd)

echo "Project root: $PROJECT_ROOT"

# Check if mkdocs is installed
if ! command -v mkdocs &> /dev/null; then
    echo "Error: mkdocs is not installed"
    echo "Please install mkdocs: pip install mkdocs mkdocs-material"
    exit 1
fi

# Build documentation
echo "Building documentation..."
mkdocs build

# Serve documentation (optional)
read -p "Do you want to serve the documentation locally? (y/n) " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo "Serving documentation at http://localhost:8000"
    echo "Press Ctrl+C to stop"
    mkdocs serve
fi
