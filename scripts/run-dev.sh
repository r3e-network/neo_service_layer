#!/bin/bash

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Get the directory where the script is located
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$( cd "$SCRIPT_DIR/.." && pwd )"

echo -e "${GREEN}Starting Neo Service Layer in development mode...${NC}"

# Detect OS type
OS_TYPE=$(uname)

# Create necessary directories
mkdir -p "$PROJECT_ROOT/config/prometheus"
mkdir -p "$PROJECT_ROOT/logs"

# Check if docker-compose.yml exists
if [ ! -f "$PROJECT_ROOT/docker-compose.yml" ]; then
    echo -e "${RED}Error: docker-compose.yml not found in project root${NC}"
    echo -e "${YELLOW}Please ensure docker-compose.yml is present in: $PROJECT_ROOT${NC}"
    exit 1
fi

# Check if .env file exists
if [ ! -f "$PROJECT_ROOT/.env" ]; then
    echo -e "${YELLOW}Warning: .env file not found in project root${NC}"
    echo "Creating default .env file..."
    cat > "$PROJECT_ROOT/.env" << EOL
NODE_ENV=development
DB_HOST=postgres
DB_PORT=5432
DB_NAME=neo_service
DB_USER=neo_user
DB_PASSWORD=neo_password
REDIS_HOST=redis
REDIS_PORT=6379
NEO_RPC_ENDPOINT=http://neo-node:10332
JWT_SECRET=dev_jwt_secret
TEE_ATTESTATION_URL=https://attestation.example.com
EOL
fi

# Function to check if Docker is running
check_docker() {
    if ! docker info > /dev/null 2>&1; then
        echo -e "${RED}Error: Docker is not running${NC}"
        echo -e "${YELLOW}Please start Docker Desktop or Docker daemon${NC}"
        exit 1
    fi
}

# Function to check Docker login status and registry accessibility
check_registry_access() {
    local registry=$1
    echo -n "Checking access to $registry... "
    if curl -s -f "https://$registry/v2/" &>/dev/null; then
        echo -e "${GREEN}success${NC}"
        return 0
    else
        echo -e "${RED}failed${NC}"
        echo -e "${YELLOW}Warning: Cannot access $registry. You may need to:${NC}"
        echo "1. Check your internet connection"
        echo "2. Run: docker login $registry"
        echo "3. Check if the registry is accessible from your network"
        return 1
    fi
}

# Function to check container health
check_health() {
    local service=$1
    local max_attempts=30
    local attempt=1

    echo -n "Waiting for $service to be healthy..."
    while [ $attempt -le $max_attempts ]; do
        if docker-compose ps "$service" 2>/dev/null | grep -q "Up"; then
            local container_id=$(docker-compose ps -q "$service")
            if docker inspect "$container_id" --format "{{.State.Health.Status}}" 2>/dev/null | grep -q "healthy"; then
                echo -e "${GREEN}ready${NC}"
                return 0
            fi
        fi
        echo -n "."
        sleep 2
        attempt=$((attempt + 1))
    done
    echo -e "${RED}failed${NC}"
    
    # Show logs of failed service
    echo -e "${YELLOW}Last logs from $service:${NC}"
    docker-compose logs --tail=50 "$service"
    return 1
}

# Function to check system requirements
check_system_requirements() {
    # Check Docker Compose version
    if ! docker-compose version > /dev/null 2>&1; then
        echo -e "${RED}Error: Docker Compose not found${NC}"
        exit 1
    fi

    # Check system resources based on OS
    if [ "$OS_TYPE" = "Darwin" ]; then
        # macOS memory check
        total_memory=$(sysctl -n hw.memsize | awk '{print $1/1024/1024}')
        if [ "${total_memory%.*}" -lt 4096 ]; then
            echo -e "${YELLOW}Warning: Less than 4GB RAM available. This might affect performance.${NC}"
        fi
        
        # macOS disk space check
        free_space=$(df -m / | awk 'NR==2 {print $4}')
        if [ -n "$free_space" ] && [ "$free_space" -lt 10240 ]; then
            echo -e "${YELLOW}Warning: Less than 10GB free disk space. This might affect performance.${NC}"
        fi
    elif [ "$OS_TYPE" = "Linux" ]; then
        # Linux memory check
        if command -v free >/dev/null 2>&1; then
            total_memory=$(free -m | awk '/^Mem:/{print $2}')
            if [ "$total_memory" -lt 4096 ]; then
                echo -e "${YELLOW}Warning: Less than 4GB RAM available. This might affect performance.${NC}"
            fi
        fi
        
        # Linux disk space check
        free_space=$(df -m / | awk 'NR==2 {print $4}')
        if [ "$free_space" -lt 10240 ]; then
            echo -e "${YELLOW}Warning: Less than 10GB free disk space. This might affect performance.${NC}"
        fi
    fi

    # Check Docker version
    docker_version=$(docker version --format '{{.Server.Version}}' 2>/dev/null)
    if [ -z "$docker_version" ]; then
        echo -e "${RED}Error: Unable to get Docker version${NC}"
        exit 1
    fi
}

# Function to pull Neo node image with fallbacks
pull_neo_node_image() {
    local neo_images=(
        "cityofzion/neo-go:latest"
        "cityofzion/neo-go:0.103.0"
        "neo-project/neo-node:latest"
        "neo-project/neo-node:3.5.0"
    )
    
    echo "Attempting to pull Neo node image..."
    for image in "${neo_images[@]}"; do
        echo -n "Trying $image... "
        if docker pull "$image" &>/dev/null; then
            echo -e "${GREEN}success${NC}"
            # Update docker-compose.yml to use the successful image
            if [ "$(uname)" = "Darwin" ]; then
                # macOS sed requires backup extension
                sed -i '' "s|image: cityofzion/neo-go:latest|image: $image|g" "$PROJECT_ROOT/docker-compose.yml"
            else
                # Linux sed
                sed -i "s|image: cityofzion/neo-go:latest|image: $image|g" "$PROJECT_ROOT/docker-compose.yml"
            fi
            return 0
        else
            echo -e "${RED}failed${NC}"
        fi
    done
    
    echo -e "${RED}Error: Failed to pull any Neo node image${NC}"
    echo -e "${YELLOW}Please check:${NC}"
    echo "1. Your internet connection"
    echo "2. Docker registry access (try 'docker login')"
    echo "3. VPN settings if you're behind a corporate network"
    echo "4. Try manually pulling the image: docker pull cityofzion/neo-go:latest"
    return 1
}

# Function to check and pull images
pull_images() {
    local registries=(
        "docker.elastic.co"
        "registry.hub.docker.com"
    )

    # Check registry access first
    for registry in "${registries[@]}"; do
        check_registry_access "$registry"
    done

    # First try to pull Neo node image with fallbacks
    if ! pull_neo_node_image; then
        echo -e "${YELLOW}Would you like to continue without the Neo node image? [y/N]${NC}"
        read -r response
        if [[ ! "$response" =~ ^[Yy]$ ]]; then
            echo "Aborting..."
            exit 1
        fi
    fi

    local images=(
        "docker.elastic.co/elasticsearch/elasticsearch:8.9.0"
        "postgres:15-alpine"
        "redis:7-alpine"
        "prom/prometheus:v2.45.0"
        "grafana/grafana:9.5.2"
        "jaegertracing/all-in-one:1.47"
    )

    echo "Pulling remaining Docker images..."
    local pull_failed=0
    for image in "${images[@]}"; do
        echo -n "Pulling $image... "
        if docker pull "$image" &>/dev/null; then
            echo -e "${GREEN}success${NC}"
        else
            echo -e "${RED}failed${NC}"
            pull_failed=1
        fi
    done

    if [ $pull_failed -eq 1 ]; then
        echo -e "${YELLOW}Some images failed to pull. Would you like to continue anyway? [y/N]${NC}"
        read -r response
        if [[ ! "$response" =~ ^[Yy]$ ]]; then
            echo "Aborting..."
            exit 1
        fi
    fi
}

# Change to project root directory
cd "$PROJECT_ROOT"

# Check system requirements
check_system_requirements

# Check if Docker is running
check_docker

# Pull required images
pull_images

# Stop any existing containers
echo "Stopping any existing containers..."
docker-compose down

# Start services
echo "Starting services..."
docker-compose up -d

# Check health of core services
check_health "postgres" || exit 1
check_health "redis" || exit 1
check_health "elasticsearch" || exit 1
check_health "neo-node" || exit 1
check_health "prometheus" || exit 1
check_health "grafana" || exit 1
check_health "api" || exit 1

echo -e "${GREEN}All services are running and healthy!${NC}"
echo "
Access points:
- API: http://localhost:3000
- Metrics: http://localhost:9090
- Grafana: http://localhost:3001 (admin/admin)
- Prometheus: http://localhost:9091
- Jaeger UI: http://localhost:16686
- Elasticsearch: http://localhost:9200

Commands:
- View logs: docker-compose logs -f
- Stop services: docker-compose down
- Restart services: $SCRIPT_DIR/run-dev.sh
- View service status: docker-compose ps

Project root: $PROJECT_ROOT

Development Tips:
- Use 'docker-compose logs -f service_name' to follow specific service logs
- Use 'docker-compose restart service_name' to restart a specific service
- Check the API health at: http://localhost:3000/health
" 