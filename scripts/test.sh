#!/bin/bash

# Exit on error
set -e

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m'

echo "Running Neo Service Layer Phase 1 Tests"
echo "======================================="

# Run unit tests
echo -e "${GREEN}Running unit tests...${NC}"
go test -v ./internal/core/neo/tests/

echo -e "${GREEN}All unit tests completed successfully!${NC}"