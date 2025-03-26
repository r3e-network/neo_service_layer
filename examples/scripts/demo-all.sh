#!/bin/bash

# Neo N3 Service Layer Complete Demo Script
# This script demonstrates all major functionalities of the service layer

set -e # Exit on error

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Logging functions
log_header() {
    echo -e "\n${BLUE}========================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}========================================${NC}"
}

log_step() {
    echo -e "\n${CYAN}➜ $1${NC}"
}

log_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

log_warning() {
    echo -e "${YELLOW}! $1${NC}"
}

log_error() {
    echo -e "${RED}✗ $1${NC}"
    exit 1
}

# Function to parse JSON without jq
parse_json() {
    # Simple JSON parser using grep and sed
    # Usage: parse_json "key" "json_string"
    local key=$1
    local json=$2
    echo "$json" | grep -o "\"$key\":[^,}]*" | sed 's/.*://' | tr -d '"' | tr -d ' '
}

# Function to generate random hex
random_hex() {
    # Length in bytes (will produce twice as many hex characters)
    local length=$1
    if command -v openssl >/dev/null 2>&1; then
        openssl rand -hex $length
    else
        # Fallback to /dev/urandom if openssl is not available
        od -An -N$length -tx1 /dev/urandom | tr -d ' \n'
    fi
}

# Wait for service to be ready
wait_for_service() {
    local port=$1
    local service=$2
    local max_attempts=30
    local attempt=1
    
    while ! nc -z localhost $port >/dev/null 2>&1; do
        if [ $attempt -gt $max_attempts ]; then
            log_error "$service failed to start (port $port)"
        fi
        echo -n "."
        sleep 1
        ((attempt++))
    done
    echo ""
}

# Check dependencies and offer installation help
check_dependencies() {
    local missing_deps=()
    
    # Check for required commands
    if ! command -v nc >/dev/null 2>&1; then
        missing_deps+=("netcat")
    fi
    
    if ! command -v curl >/dev/null 2>&1; then
        missing_deps+=("curl")
    fi
    
    # jq is optional, we have a fallback
    if ! command -v jq >/dev/null 2>&1; then
        log_warning "jq is not installed - will use basic JSON parsing"
        log_warning "For better JSON handling, install jq:"
        log_warning "  Homebrew: brew install jq"
        log_warning "  Apt: sudo apt-get install jq"
        log_warning "  Yum: sudo yum install jq"
    fi
    
    if ! command -v openssl >/dev/null 2>&1; then
        missing_deps+=("openssl")
    fi
    
    # If there are missing dependencies, show installation instructions
    if [ ${#missing_deps[@]} -ne 0 ]; then
        log_error "Missing required dependencies: ${missing_deps[*]}\n\nPlease install them:\n\nHomebrew:\nbrew install ${missing_deps[*]}\n\nApt:\nsudo apt-get install ${missing_deps[*]}\n\nYum:\nsudo yum install ${missing_deps[*]}"
    fi
}

# Verify prerequisites
log_header "Checking Prerequisites"
check_dependencies

# Start Neo N3 Service Layer
log_header "Starting Neo N3 Service Layer"

if nc -z localhost 3000 >/dev/null 2>&1; then
    log_step "Stopping existing services..."
    pkill -f "bin/server|bin/worker" || true
    sleep 5
fi

log_step "Starting services..."
./scripts/start-testnet.sh &

log_step "Waiting for API service..."
wait_for_service 3000 "API Service"
log_success "Services started successfully"

# Setup example data
log_header "Setting Up Example Data"

log_step "Creating test accounts..."
TEST_ACCOUNT=$(./bin/cli accounts create | grep Address | awk '{print $2}')
log_success "Created test account: $TEST_ACCOUNT"

log_step "Setting up secrets..."
WEBHOOK_URL="https://webhook.site/$(random_hex 16)"
./bin/cli secrets set WEBHOOK_URL "$WEBHOOK_URL"
./bin/cli secrets set ALERT_EMAIL "test@example.com"
./bin/cli secrets set ORACLE_PRIVATE_KEY "$(random_hex 32)"
log_success "Secrets configured"

# Deploy example contracts
log_header "Deploying Example Contracts"

log_step "Deploying test token contract..."
./bin/cli contracts deploy examples/contracts/test-token.nef
TOKEN_HASH=$(./bin/cli contracts list | grep TestToken | awk '{print $1}')
log_success "Token contract deployed: $TOKEN_HASH"

log_step "Deploying bridge contract..."
./bin/cli contracts deploy examples/contracts/bridge-contract.nef
BRIDGE_HASH=$(./bin/cli contracts list | grep BridgeContract | awk '{print $1}')
log_success "Bridge contract deployed: $BRIDGE_HASH"

# Deploy functions
log_header "Deploying Example Functions"

for func in examples/functions/*.js; do
    log_step "Deploying $(basename $func)..."
    ./bin/cli functions deploy -f "$func"
done
log_success "All functions deployed"

# Create triggers
log_header "Creating Triggers"

for trigger in examples/functions/triggers/*.json; do
    log_step "Creating trigger from $(basename $trigger)..."
    ./bin/cli triggers create -f "$trigger"
done
log_success "All triggers created"

# Test Price Feed Service
log_header "Testing Price Feed Service"

log_step "Publishing test price data..."
./bin/cli price-feed publish NEO/GAS 1.75
sleep 2

log_step "Triggering price alert function..."
FUNC_ID=$(./bin/cli functions list | grep price-alert | awk '{print $1}')
./bin/cli functions invoke $FUNC_ID

log_step "Checking webhook for alert..."
sleep 5
RESPONSE=$(curl -s "$WEBHOOK_URL/requests")
if [ -n "$RESPONSE" ]; then
    if command -v jq >/dev/null 2>&1; then
        echo "$RESPONSE" | jq '.'
    else
        echo "$RESPONSE"
    fi
    log_success "Price alert received"
else
    log_error "Price alert not received"
fi

# Test Gas Bank Service
log_header "Testing Gas Bank Service"

log_step "Requesting gas allocation..."
ALLOCATION_RESPONSE=$(./bin/cli gas-bank allocate 1000000)
if command -v jq >/dev/null 2>&1; then
    ALLOCATION_ID=$(echo "$ALLOCATION_RESPONSE" | jq -r '.id')
else
    ALLOCATION_ID=$(parse_json "id" "$ALLOCATION_RESPONSE")
fi
log_success "Gas allocated: $ALLOCATION_ID"

log_step "Checking allocation status..."
./bin/cli gas-bank status $ALLOCATION_ID
log_success "Gas allocation active"

# Test Token Transfers
log_header "Testing Token Transfers"

log_step "Minting test tokens..."
./bin/cli contracts invoke $TOKEN_HASH mint \
    --account $TEST_ACCOUNT \
    --args "['$TEST_ACCOUNT', 2000000000]"
sleep 5

log_step "Executing large transfer..."
RECIPIENT="NZvyUfPLVqtxLPXbbb9L5wo5qwdxP2RpHN"
./bin/cli contracts invoke $TOKEN_HASH transfer \
    --account $TEST_ACCOUNT \
    --args "['$TEST_ACCOUNT', '$RECIPIENT', 1500000000]"
sleep 5

# Test Bridge Monitor
log_header "Testing Bridge Monitor"

log_step "Simulating bridge transfer..."
TX_HASH="0x$(random_hex 32)"
./bin/cli contracts invoke $BRIDGE_HASH emitBridgeTransfer \
    --args "[
        '$TX_HASH',
        'Ethereum',
        'Neo',
        '2000000000',
        '0x$(random_hex 20)',
        '0x$(random_hex 20)',
        '$RECIPIENT'
    ]"
sleep 10

# Check Metrics
log_header "Checking Metrics"

log_step "Querying service metrics..."
START_TIME=$(date -u +"%Y-%m-%dT%H:%M:%SZ" -d "1 hour ago")
END_TIME=$(date -u +"%Y-%m-%dT%H:%M:%SZ")

echo "Price Feed Metrics:"
./bin/cli metrics query price_updates --start "$START_TIME" --end "$END_TIME"

echo "Gas Bank Metrics:"
./bin/cli metrics query gas_allocation --start "$START_TIME" --end "$END_TIME"

echo "Bridge Metrics:"
./bin/cli metrics query bridge_transfer --start "$START_TIME" --end "$END_TIME"

# Check Logs
log_header "Checking Logs"

log_step "Retrieving recent logs..."
./bin/cli logs get --tail 50

# Final Status
log_header "Service Status"

log_step "Checking service health..."
./bin/cli health

log_step "Listing active functions..."
./bin/cli functions list

log_step "Listing active triggers..."
./bin/cli triggers list

# Summary
log_header "Demo Summary"

echo -e "${GREEN}The following components were demonstrated:${NC}"
echo "✓ Service Layer Startup"
echo "✓ Account Management"
echo "✓ Secret Management"
echo "✓ Contract Deployment"
echo "✓ Function Deployment"
echo "✓ Trigger Creation"
echo "✓ Price Feed Service"
echo "✓ Gas Bank Service"
echo "✓ Token Operations"
echo "✓ Bridge Monitoring"
echo "✓ Metrics Collection"
echo "✓ Logging System"

echo -e "\n${GREEN}Demo completed successfully!${NC}"