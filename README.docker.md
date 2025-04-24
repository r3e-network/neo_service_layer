# Neo Service Layer Docker Setup

This document provides detailed instructions for setting up and using the Neo Service Layer with Docker Compose.

## Prerequisites

- Docker
- Docker Compose

## Setup Instructions

### 1. Install Docker

If you don't have Docker installed, download and install it from the official website:
- [Docker Desktop for Mac](https://www.docker.com/products/docker-desktop)
- [Docker Desktop for Windows](https://www.docker.com/products/docker-desktop)
- [Docker for Linux](https://docs.docker.com/engine/install/)

### 2. Start Docker

Make sure Docker is running on your system:
- On macOS: Open Docker Desktop from the Applications folder
- On Windows: Open Docker Desktop from the Start menu
- On Linux: Run `sudo systemctl start docker`

You can also run our helper script:
```bash
./start-docker.sh
```

### 3. Run the Docker Compose Setup

We've created a script that handles building and starting the Docker Compose services:
```bash
./run-docker.sh
```

This script will:
1. Check if Docker is running
2. Stop any running containers
3. Build the containers
4. Start the containers
5. Wait for services to start
6. Check if services are running
7. Show logs

### 4. Test the Setup

To verify that the Docker Compose setup is working correctly, run:
```bash
./test-docker.sh
```

This script will check if all the required services are accessible.

## Phased Deployment

We've set up a phased deployment approach to make it easier to troubleshoot issues:

### Phase 1: Basic Services (Current)

The current docker-compose.yml file only includes the basic services:
- MongoDB
- Redis

### Phase 2: Full Stack

Once the basic services are working, you can uncomment the additional services in docker-compose.yml:
- API
- Enclave
- MailHog

To enable the full stack:
1. Edit docker-compose.yml and uncomment the api, enclave, and mailhog services
2. Run `./run-docker.sh` again to rebuild and restart the services
3. Run `./test-docker.sh` to verify that all services are working correctly

## Troubleshooting

### Docker is not running

If you see the error "Docker is not running", make sure Docker Desktop is started on your system.

### Services are not accessible

If the test script reports that services are not accessible:
1. Check the logs: `docker-compose logs`
2. Make sure the ports are not being used by other applications
3. Try restarting the services: `docker-compose restart`

### MongoDB connection issues

If you're having trouble connecting to MongoDB:
1. Check the MongoDB logs: `docker-compose logs mongodb`
2. Try connecting manually: `docker-compose exec mongodb mongosh`

### Redis connection issues

If you're having trouble connecting to Redis:
1. Check the Redis logs: `docker-compose logs redis`
2. Try connecting manually: `docker-compose exec redis redis-cli`

## Customization

### Changing Ports

If you need to change the ports used by the services, edit the docker-compose.yml file and modify the port mappings.

### Changing Configuration

The API service uses a custom configuration file (docker-appsettings.json) that is mounted as a volume. You can edit this file to change the configuration of the API service.

## Cleanup

To stop and remove all containers, networks, and volumes created by Docker Compose:
```bash
docker-compose down -v
```

To remove only the containers and networks, but keep the volumes:
```bash
docker-compose down
```
