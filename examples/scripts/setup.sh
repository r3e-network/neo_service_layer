#!/bin/bash

# Setup script for Neo N3 Service Layer examples
# This script sets up the environment and deploys example functions

set -e # Exit on error

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}Setting up Neo N3 Service Layer examples...${NC}"

# Check if neo service layer is running
if ! nc -z localhost 3000 >/dev/null 2>&1; then
    echo "Starting Neo N3 Service Layer..."
    ./scripts/start-testnet.sh &
    sleep 10 # Wait for services to start
fi

# Create example secrets
echo -e "\n${YELLOW}Setting up example secrets...${NC}"
./bin/cli secrets set WEBHOOK_URL "https://webhook.site/$(openssl rand -hex 16)" # Generate random webhook for testing
./bin/cli secrets set ALERT_EMAIL "test@example.com"
./bin/cli secrets set ORACLE_PRIVATE_KEY "$(openssl rand -hex 32)" # Generate random private key for testing

# Deploy example functions
echo -e "\n${YELLOW}Deploying example functions...${NC}"
for func in examples/functions/*.js; do
    echo "Deploying $(basename $func)..."
    ./bin/cli functions deploy -f "$func"
done

# Create triggers
echo -e "\n${YELLOW}Creating triggers...${NC}"
for trigger in examples/functions/triggers/*.json; do
    echo "Creating trigger from $(basename $trigger)..."
    ./bin/cli triggers create -f "$trigger"
done

echo -e "\n${GREEN}Setup complete! You can now run the verification scripts.${NC}"