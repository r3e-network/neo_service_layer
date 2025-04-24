# Manual Docker Setup Instructions

If you're unable to run Docker Desktop or the provided scripts, you can manually set up the Neo Service Layer using the following commands.

## Prerequisites

- Docker
- Docker Compose

## Manual Setup Steps

### 1. Start MongoDB

```bash
docker run -d --name neo-mongodb -p 27017:27017 -v mongodb-data:/data/db mongo:latest
```

### 2. Start Redis

```bash
docker run -d --name neo-redis -p 6379:6379 -v redis-data:/data redis:latest
```

### 3. Verify Services are Running

```bash
docker ps
```

You should see both MongoDB and Redis containers running.

### 4. Test MongoDB Connection

```bash
docker exec -it neo-mongodb mongosh --eval "db.runCommand({ping:1})"
```

You should see output containing `{ ok: 1 }`.

### 5. Test Redis Connection

```bash
docker exec -it neo-redis redis-cli ping
```

You should see output containing `PONG`.

## Next Steps

Once the basic services are running, you can proceed with building and running the API and Enclave services:

### 1. Build the API Image

```bash
docker build -t neo-api -f Dockerfile.api .
```

### 2. Run the API Container

```bash
docker run -d --name neo-api -p 8080:80 -p 8443:443 \
  --link neo-mongodb:mongodb --link neo-redis:redis \
  -v api-storage:/app/Storage \
  -v api-database:/app/Database \
  -v api-backups:/app/Backups \
  -v $(pwd)/docker-appsettings.json:/app/appsettings.json \
  -e ASPNETCORE_ENVIRONMENT=Production \
  neo-api
```

### 3. Build the Enclave Image

```bash
docker build -t neo-enclave -f Dockerfile.enclave .
```

### 4. Run the Enclave Container

```bash
docker run -d --name neo-enclave \
  --link neo-mongodb:mongodb --link neo-redis:redis \
  -v enclave-data:/app/data \
  neo-enclave
```

### 5. Start MailHog

```bash
docker run -d --name neo-mailhog -p 1025:1025 -p 8025:8025 mailhog/mailhog
```

## Stopping the Services

To stop and remove all containers:

```bash
docker stop neo-mongodb neo-redis neo-api neo-enclave neo-mailhog
docker rm neo-mongodb neo-redis neo-api neo-enclave neo-mailhog
```

## Cleaning Up Volumes

To remove all volumes (this will delete all data):

```bash
docker volume rm mongodb-data redis-data api-storage api-database api-backups enclave-data
```
