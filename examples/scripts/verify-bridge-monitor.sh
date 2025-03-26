#!/bin/bash

# Verification script for bridge monitor function
# This script simulates a bridge transfer and verifies the monitor's response

set -e # Exit on error

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}Verifying bridge monitor function...${NC}"

# Get function ID
FUNC_ID=$(./bin/cli functions list | grep bridge-monitor | awk '{print $1}')

# Deploy bridge contract if needed
BRIDGE_HASH="0x48c40d4666f93408be1bef038b6722404d9a4c2a"
if ! ./bin/cli contracts info $BRIDGE_HASH &>/dev/null; then
    echo -e "\n${YELLOW}Deploying bridge contract...${NC}"
    ./bin/cli contracts deploy examples/contracts/bridge-contract.nef
    BRIDGE_HASH=$(./bin/cli contracts list | grep BridgeContract | awk '{print $1}')
fi

# Setup test transfer event
echo -e "\n${YELLOW}Setting up test bridge transfer...${NC}"
TX_HASH="0x$(openssl rand -hex 32)"
SOURCE_CHAIN="Ethereum"
TARGET_CHAIN="Neo"
AMOUNT="2000000000" # 20 tokens
TOKEN="0x$(openssl rand -hex 20)"
SENDER="0x$(openssl rand -hex 20)"
RECIPIENT="NZvyUfPLVqtxLPXbbb9L5wo5qwdxP2RpHN"

# Monitor logs
LOG_FILE="/tmp/bridge-monitor-test.log"
./bin/cli logs tail -f > $LOG_FILE &
LOG_PID=$!

# Simulate bridge transfer event
echo -e "\n${YELLOW}Simulating bridge transfer event...${NC}"
./bin/cli contracts invoke $BRIDGE_HASH emitBridgeTransfer \
    --args "[
        '$TX_HASH',
        '$SOURCE_CHAIN',
        '$TARGET_CHAIN',
        '$AMOUNT',
        '$TOKEN',
        '$SENDER',
        '$RECIPIENT'
    ]"

# Wait for monitor to process
sleep 10

# Check logs for monitor response
if grep -q "Bridge transfer processed successfully" $LOG_FILE; then
    echo -e "\n${GREEN}Success! Bridge transfer processed:${NC}"
    grep -A 10 "Bridge transfer processed successfully" $LOG_FILE
    
    # Verify metrics
    echo -e "\n${YELLOW}Verifying metrics...${NC}"
    METRICS=$(./bin/cli metrics query bridge_transfer \
        --start "$(date -u +"%Y-%m-%dT%H:%M:%SZ" -d "1 minute ago")" \
        --end "$(date -u +"%Y-%m-%dT%H:%M:%SZ")")
    
    if [ ! -z "$METRICS" ]; then
        echo -e "${GREEN}Metrics recorded successfully:${NC}"
        echo "$METRICS" | jq '.'
    else
        echo -e "${RED}Error: No metrics recorded${NC}"
        exit 1
    fi
else
    echo -e "\n${RED}Error: Bridge transfer not processed${NC}"
    exit 1
fi

# Cleanup
kill $LOG_PID
rm $LOG_FILE

echo -e "\n${GREEN}Bridge monitor function verification complete!${NC}"