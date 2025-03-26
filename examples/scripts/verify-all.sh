#!/bin/bash

# Main verification script that runs all example verifications

set -e # Exit on error

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to run a verification script
run_verification() {
    local script=$1
    local name=$(basename $script .sh | sed 's/verify-//')
    
    echo -e "\n${BLUE}========================================${NC}"
    echo -e "${BLUE}Running $name verification...${NC}"
    echo -e "${BLUE}========================================${NC}\n"
    
    if bash "$script"; then
        echo -e "\n${GREEN}✓ $name verification passed${NC}"
        return 0
    else
        echo -e "\n${RED}✗ $name verification failed${NC}"
        return 1
    fi
}

# Print header
echo -e "${YELLOW}"
echo "=================================="
echo "Neo N3 Service Layer Example Tests"
echo "=================================="
echo -e "${NC}"

# Check if service layer is running
if ! nc -z localhost 3000 >/dev/null 2>&1; then
    echo -e "${RED}Error: Neo N3 Service Layer is not running${NC}"
    echo "Please run: ./scripts/start-testnet.sh"
    exit 1
fi

# Run setup script
echo -e "\n${YELLOW}Running setup script...${NC}"
bash examples/scripts/setup.sh

# Initialize test results
PASSED=0
FAILED=0
FAILED_TESTS=()

# Run all verification scripts
for script in examples/scripts/verify-*.sh; do
    if [ "$script" != "examples/scripts/verify-all.sh" ]; then
        if run_verification "$script"; then
            ((PASSED++))
        else
            ((FAILED++))
            FAILED_TESTS+=("$(basename $script .sh | sed 's/verify-//')")
        fi
    fi
done

# Print summary
echo -e "\n${BLUE}========================================${NC}"
echo -e "${YELLOW}Test Summary:${NC}"
echo -e "${GREEN}Passed: $PASSED${NC}"
if [ $FAILED -gt 0 ]; then
    echo -e "${RED}Failed: $FAILED${NC}"
    echo -e "\n${RED}Failed tests:${NC}"
    for test in "${FAILED_TESTS[@]}"; do
        echo -e "${RED}- $test${NC}"
    done
    exit 1
else
    echo -e "\n${GREEN}All verifications passed successfully!${NC}"
fi