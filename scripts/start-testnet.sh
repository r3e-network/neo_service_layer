#!/bin/bash

# Exit on error
set -e

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Default environment variables
export NEO_NETWORK="testnet"
export API_PORT=${API_PORT:-3000}
export METRICS_PORT=${METRICS_PORT:-9090}
export NEO_NODE_URL=${NEO_NODE_URL:-"http://localhost:10332"}
export REDIS_URL=${REDIS_URL:-"redis://localhost:6379"}

# Function to check if a service is running
check_service() {
    local service_name=$1
    local port=$2
    
    # If no port is specified, just check if the process is running
    if [ -z "$port" ]; then
        if [ -f "./pids/${service_name}.pid" ]; then
            local pid=$(cat "./pids/${service_name}.pid")
            if ps -p $pid > /dev/null; then
                echo -e "${GREEN}✓ $service_name is running (PID: $pid)${NC}"
                return 0
            fi
        fi
        echo -e "${RED}✗ $service_name is not running${NC}"
        return 1
    fi
    
    # If port is specified, check if it's listening
    if nc -z localhost $port; then
        echo -e "${GREEN}✓ $service_name is running on port $port${NC}"
        return 0
    else
        echo -e "${RED}✗ $service_name failed to start on port $port${NC}"
        return 1
    fi
}

# Function to start a service
start_service() {
    local service_name=$1
    local start_cmd=$2
    local port=$3
    
    echo -e "${YELLOW}Starting $service_name...${NC}"
    eval "$start_cmd" &
    local pid=$!
    sleep 5
    
    if ps -p $pid > /dev/null; then
        echo "$pid" > "./pids/${service_name}.pid"
        if [ -n "$port" ]; then
            check_service "$service_name" "$port"
            return $?
        fi
        return 0
    else
        return 1
    fi
}

# Create directory for PID files
mkdir -p ./pids

# Build the services
echo -e "${GREEN}Building services...${NC}"
go build -o bin/server ./cmd/server
go build -o bin/worker ./cmd/worker
go build -o bin/cli ./cmd/cli

# Check dependencies
echo -e "${GREEN}Checking dependencies...${NC}"

# Check Redis
echo -e "${YELLOW}Checking Redis connection...${NC}"
if ! redis-cli ping &> /dev/null; then
    echo -e "${RED}Redis is not running. Please start Redis first.${NC}"
    exit 1
fi

# Start services
echo -e "${GREEN}Starting Neo N3 Service Layer (Testnet Mode)...${NC}"

# Start API Server
start_service "api" "./bin/server" "$API_PORT"

# Start Workers (handles price feed, gas bank, trigger, functions)
start_service "upkeep-worker" "./bin/worker --type upkeep"
start_service "trigger-worker" "./bin/worker --type trigger"
start_service "executor-worker" "./bin/worker --type executor"

# Health check
echo -e "${GREEN}Performing health check...${NC}"
for service in api upkeep-worker trigger-worker executor-worker; do
    if [ -f "./pids/${service}.pid" ]; then
        pid=$(cat "./pids/${service}.pid")
        if ps -p $pid > /dev/null; then
            echo -e "${GREEN}✓ $service is running (PID: $pid)${NC}"
        else
            echo -e "${RED}✗ $service is not running${NC}"
        fi
    else
        echo -e "${RED}✗ $service failed to start${NC}"
    fi
done

echo -e "${GREEN}Neo N3 Service Layer is running in testnet mode${NC}"
echo -e "${GREEN}API endpoint: http://localhost:${API_PORT}${NC}"
echo -e "${GREEN}Metrics endpoint: http://localhost:${METRICS_PORT}${NC}"

# Trap SIGINT and SIGTERM
cleanup() {
    echo -e "${YELLOW}Shutting down services...${NC}"
    for pid_file in ./pids/*.pid; do
        if [ -f "$pid_file" ]; then
            pid=$(cat "$pid_file")
            service_name=$(basename "$pid_file" .pid)
            echo -e "${YELLOW}Stopping $service_name (PID: $pid)...${NC}"
            kill $pid 2>/dev/null || true
            rm "$pid_file"
        fi
    done
    echo -e "${GREEN}Shutdown complete${NC}"
    exit 0
}

trap cleanup SIGINT SIGTERM

# Keep script running
echo -e "${YELLOW}Press Ctrl+C to stop all services${NC}"
while true; do sleep 1; done