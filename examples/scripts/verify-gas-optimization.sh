#!/bin/bash

# Verification script for gas optimization function
# This script verifies the gas optimization function's behavior

set -e # Exit on error

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}Verifying gas optimization function...${NC}"

# Get function ID
FUNC_ID=$(./bin/cli functions list | grep gas-optimization | awk '{print $1}')

# Deploy test contract if needed
CONTRACT_HASH="0x668e0c1f9d7b70a99dd9e06eadd4c784d641385d"
if ! ./bin/cli contracts info $CONTRACT_HASH &>/dev/null; then
    echo -e "\n${YELLOW}Deploying test contract...${NC}"
    ./bin/cli contracts deploy examples/contracts/test-contract.nef
    CONTRACT_HASH=$(./bin/cli contracts list | grep TestContract | awk '{print $1}')
fi

# Set initial gas balance
echo -e "\n${YELLOW}Setting up initial gas balance...${NC}"
INITIAL_GAS="500000000" # 5 GAS
./bin/cli gas transfer $CONTRACT_HASH --amount $INITIAL_GAS

# Record initial metrics
echo -e "\n${YELLOW}Recording initial metrics...${NC}"
./bin/cli metrics record contract_gas_usage \
    --contract $CONTRACT_HASH \
    --value $INITIAL_GAS

# Monitor logs
LOG_FILE="/tmp/gas-optimization-test.log"
./bin/cli logs tail -f > $LOG_FILE &
LOG_PID=$!

# Trigger optimization
echo -e "\n${YELLOW}Triggering gas optimization...${NC}"
./bin/cli functions invoke $FUNC_ID

# Wait for optimization
sleep 5

# Check logs for optimization results
if grep -q "Gas allocation increased\|Excess gas returned" $LOG_FILE; then
    echo -e "\n${GREEN}Success! Gas optimization performed:${NC}"
    grep -A 5 "Gas allocation increased\|Excess gas returned" $LOG_FILE
    
    # Verify new gas balance
    NEW_BALANCE=$(./bin/cli contracts info $CONTRACT_HASH | grep "GAS Balance" | awk '{print $3}')
    echo -e "\nNew contract GAS balance: $NEW_BALANCE"
else
    echo -e "\n${RED}Error: Gas optimization did not occur${NC}"
    exit 1
fi

# Cleanup
kill $LOG_PID
rm $LOG_FILE

echo -e "\n${GREEN}Gas optimization function verification complete!${NC}"