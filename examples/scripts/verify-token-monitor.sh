#!/bin/bash

# Verification script for token transfer monitor
# This script simulates a token transfer and verifies the monitor's response

set -e # Exit on error

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}Verifying token transfer monitor...${NC}"

# Get function ID
FUNC_ID=$(./bin/cli functions list | grep token-transfer-monitor | awk '{print $1}')

# Deploy test token if needed
TOKEN_HASH="0x43cf4863eb7d1148a0b7d833a4acd2e432748334"
if ! ./bin/cli contracts info $TOKEN_HASH &>/dev/null; then
    echo -e "\n${YELLOW}Deploying test token contract...${NC}"
    ./bin/cli contracts deploy examples/contracts/test-token.nef
    TOKEN_HASH=$(./bin/cli contracts list | grep TestToken | awk '{print $1}')
fi

# Get test account
TEST_ACCOUNT=$(./bin/cli accounts list | head -n 1 | awk '{print $1}')

# Mint tokens to test account
echo -e "\n${YELLOW}Minting test tokens...${NC}"
./bin/cli contracts invoke $TOKEN_HASH mint \
    --account $TEST_ACCOUNT \
    --args "['$TEST_ACCOUNT', 2000000000]"

# Setup test transfer
echo -e "\n${YELLOW}Setting up test transfer...${NC}"
RECIPIENT="NZvyUfPLVqtxLPXbbb9L5wo5qwdxP2RpHN"
AMOUNT="1500000000" # Above threshold

# Monitor logs
LOG_FILE="/tmp/token-monitor-test.log"
./bin/cli logs tail -f > $LOG_FILE &
LOG_PID=$!

# Perform transfer
echo -e "\n${YELLOW}Executing test transfer...${NC}"
./bin/cli contracts invoke $TOKEN_HASH transfer \
    --account $TEST_ACCOUNT \
    --args "['$TEST_ACCOUNT', '$RECIPIENT', $AMOUNT]"

# Wait for monitor to process
sleep 5

# Check logs for monitor response
if grep -q "Large transfer notification sent" $LOG_FILE; then
    echo -e "\n${GREEN}Success! Monitor detected large transfer:${NC}"
    grep -A 5 "Large transfer notification sent" $LOG_FILE
else
    echo -e "\n${RED}Error: Monitor did not detect transfer${NC}"
    exit 1
fi

# Cleanup
kill $LOG_PID
rm $LOG_FILE

echo -e "\n${GREEN}Token transfer monitor verification complete!${NC}"