#!/bin/bash

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Get the directory where the script is located
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$( cd "$SCRIPT_DIR/.." && pwd )"

echo -e "${GREEN}Starting Neo Service Layer in production mode...${NC}"

# Create necessary directories
mkdir -p "$PROJECT_ROOT/config/prometheus"
mkdir -p "$PROJECT_ROOT/logs"

# Check if docker-compose.yml exists
if [ ! -f "$PROJECT_ROOT/docker-compose.yml" ]; then
    echo -e "${RED}Error: docker-compose.yml not found in project root${NC}"
    echo -e "${YELLOW}Please ensure docker-compose.yml is present in: $PROJECT_ROOT${NC}"
    exit 1
fi

# Check if production .env file exists
if [ ! -f "$PROJECT_ROOT/.env.production" ]; then
    echo -e "${RED}Error: .env.production file not found in project root${NC}"
    echo -e "${YELLOW}Please create .env.production with proper production credentials in: $PROJECT_ROOT${NC}"
    exit 1
fi

# Function to check if Docker is running
check_docker() {
    if ! docker info > /dev/null 2>&1; then
        echo -e "${RED}Error: Docker is not running${NC}"
        exit 1
    fi
}

# Function to check container health
check_health() {
    local service=$1
    local max_attempts=60
    local attempt=1

    echo -n "Waiting for $service to be ready..."
    while [ $attempt -le $max_attempts ]; do
        if docker-compose -f "$PROJECT_ROOT/docker-compose.yml" ps | grep "$service" | grep "Up" > /dev/null; then
            echo -e "${GREEN}ready${NC}"
            return 0
        fi
        echo -n "."
        sleep 2
        attempt=$((attempt + 1))
    done
    echo -e "${RED}failed${NC}"
    return 1
}

# Function to check system requirements
check_system_requirements() {
    # Check available memory
    total_memory=$(free -m | awk '/^Mem:/{print $2}')
    if [ $total_memory -lt 8192 ]; then
        echo -e "${YELLOW}Warning: Less than 8GB RAM available. This might affect performance.${NC}"
    fi

    # Check available disk space
    free_space=$(df -m / | awk 'NR==2 {print $4}')
    if [ $free_space -lt 20480 ]; then
        echo -e "${YELLOW}Warning: Less than 20GB free disk space. This might affect performance.${NC}"
    fi

    # Check Docker version
    docker_version=$(docker version --format '{{.Server.Version}}' 2>/dev/null)
    if [ -z "$docker_version" ]; then
        echo -e "${RED}Error: Unable to get Docker version${NC}"
        exit 1
    fi
    
    # Check if running Docker version is at least 20.10
    if [ "$(printf '%s\n' "20.10" "$docker_version" | sort -V | head -n1)" != "20.10" ]; then
        echo -e "${YELLOW}Warning: Docker version $docker_version might be too old. Recommended version is 20.10 or higher.${NC}"
    fi
}

# Check if running as root in production
if [ "$EUID" -ne 0 ]; then
    echo -e "${RED}Error: Please run as root in production${NC}"
    exit 1
fi

# Change to project root directory
cd "$PROJECT_ROOT"

# Check system requirements
check_system_requirements

# Check if Docker is running
check_docker

# Pull latest images
echo "Pulling latest Docker images..."
docker-compose pull

# Stop any existing containers
echo "Stopping any existing containers..."
docker-compose down

# Start services with production environment
echo "Starting services..."
NODE_ENV=production docker-compose --env-file .env.production up -d

# Check health of core services
check_health "postgres" || exit 1
check_health "redis" || exit 1
check_health "neo-node" || exit 1
check_health "elasticsearch" || exit 1

echo -e "${GREEN}Services are running in production mode!${NC}"
echo "
Access points:
- API: http://localhost:3000
- Metrics: http://localhost:9090
- Grafana: http://localhost:3001
- Prometheus: http://localhost:9091
- Jaeger UI: http://localhost:16686
- Elasticsearch: http://localhost:9200

Commands:
- View logs: docker-compose logs -f
- Stop services: docker-compose down
- Restart services: $SCRIPT_DIR/run-prod.sh

Project root: $PROJECT_ROOT

Monitor the services and check the logs for any issues.
"

# Set up log rotation
if ! crontab -l | grep -q "docker-compose logs"; then
    (crontab -l 2>/dev/null; echo "0 0 * * * docker-compose -f $PROJECT_ROOT/docker-compose.yml logs > $PROJECT_ROOT/logs/docker-\$(date +\%Y\%m\%d).log 2>&1") | crontab -
    echo "Log rotation has been set up"
fi 