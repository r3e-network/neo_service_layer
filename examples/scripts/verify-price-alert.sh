#!/bin/bash

# Verification script for price alert function
# This script triggers and verifies the price alert function

set -e # Exit on error

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}Verifying price alert function...${NC}"

# Get function ID
FUNC_ID=$(./bin/cli functions list | grep price-alert | awk '{print $1}')

# Get webhook URL
WEBHOOK_URL=$(./bin/cli secrets get WEBHOOK_URL)

# Create a webhook.site collector
COLLECTOR_ID=$(curl -s -X POST "https://webhook.site/token" | jq -r '.uuid')

echo "Using webhook collector: $COLLECTOR_ID"

# Update webhook URL to our collector
./bin/cli secrets set WEBHOOK_URL "https://webhook.site/$COLLECTOR_ID"

# Manually trigger the function
echo -e "\n${YELLOW}Triggering price alert function...${NC}"
./bin/cli functions invoke $FUNC_ID

# Wait for webhook to receive data
echo -e "\n${YELLOW}Waiting for webhook response...${NC}"
sleep 5

# Check webhook.site for received data
RESPONSE=$(curl -s "https://webhook.site/token/$COLLECTOR_ID/requests")
REQUEST_COUNT=$(echo $RESPONSE | jq '.data | length')

if [ "$REQUEST_COUNT" -gt 0 ]; then
    echo -e "${GREEN}Success! Received webhook data:${NC}"
    echo $RESPONSE | jq '.data[0].content'
else
    echo -e "${RED}Error: No webhook data received${NC}"
    exit 1
fi

# Restore original webhook URL
./bin/cli secrets set WEBHOOK_URL "$WEBHOOK_URL"

echo -e "\n${GREEN}Price alert function verification complete!${NC}"