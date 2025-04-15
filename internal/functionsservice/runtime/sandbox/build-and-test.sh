#!/bin/bash

set -e

echo "Building and testing JavaScript sandbox package..."

# Change to the project root directory
cd $(dirname "$0")
BASE_DIR=$(pwd)

echo "Running go vet..."
go vet .

echo "Running go build..."
go build .

echo "Running go test..."
go test -v .

echo "Checking file structure..."
FILES=(
  "config.go"
  "context.go"
  "execution.go" 
  "json.go"
  "memory.go"
  "models.go"
  "sandbox.go"
  "services.go"
)

for file in "${FILES[@]}"; do
  if [ ! -f "$file" ]; then
    echo "Missing file: $file"
    exit 1
  fi
done

echo "All sandbox files exist."

echo "Checking imports and dependencies..."
go mod tidy

echo "All tests passed!" 