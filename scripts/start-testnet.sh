#!/bin/bash

# Exit on error
set -e

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m'

echo -e "${GREEN}Starting Neo N3 private network for testing...${NC}"

# Check if neo-go is installed
if ! command -v neo-go &> /dev/null; then
    echo -e "${RED}neo-go is not installed. Installing...${NC}"
    go install github.com/nspcc-dev/neo-go@latest
fi

# Start the private network in the background
echo -e "${GREEN}Starting private network...${NC}"
neo-go node --config-path test-chain/protocol.privnet.yml --privnet &
CHAIN_PID=$!

# Wait for node to start
echo "Waiting for node to start..."
sleep 10

# Export test node URL
export NEO_TEST_NODE_URL="http://localhost:10332"

echo -e "${GREEN}Neo N3 private network is running on $NEO_TEST_NODE_URL${NC}"
echo -e "${GREEN}Chain PID: $CHAIN_PID${NC}"

# Run the tests
echo -e "${GREEN}Running tests...${NC}"
./scripts/test.sh

# Cleanup
echo -e "${GREEN}Cleaning up...${NC}"
kill $CHAIN_PID

echo -e "${GREEN}Test environment shutdown complete${NC}"