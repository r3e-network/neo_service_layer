#!/bin/bash

# Neo Service Layer Environment Setup Script
# This script sets up the development environment for the Neo Service Layer project

# Exit on error
set -e

# Set the base directory to the current directory
BASE_DIR=$(pwd)

echo "Setting up Neo Service Layer development environment..."

# Check for .NET SDK
if ! command -v dotnet &> /dev/null; then
    echo "Error: .NET SDK is not installed. Please install .NET 7.0 SDK or later."
    exit 1
fi

# Check .NET version
DOTNET_VERSION=$(dotnet --version)
echo "Detected .NET version: $DOTNET_VERSION"

# Check for Docker
if ! command -v docker &> /dev/null; then
    echo "Warning: Docker is not installed. Docker is required for running the services in containers."
    echo "Please install Docker from https://www.docker.com/get-started"
fi

# Check for Docker Compose
if ! command -v docker-compose &> /dev/null; then
    echo "Warning: Docker Compose is not installed. Docker Compose is required for running the services in containers."
    echo "Please install Docker Compose from https://docs.docker.com/compose/install/"
fi

# Check for AWS CLI (for Nitro Enclave support)
if ! command -v aws &> /dev/null; then
    echo "Warning: AWS CLI is not installed. AWS CLI is required for Nitro Enclave support."
    echo "Please install AWS CLI from https://aws.amazon.com/cli/"
fi

# Create necessary directories if they don't exist
echo "Creating necessary directories..."
mkdir -p logs
mkdir -p data
mkdir -p config

# Create default configuration files
echo "Creating default configuration files..."
if [ ! -f "config/appsettings.Development.json" ]; then
    cat > config/appsettings.Development.json << EOF
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=NeoServiceLayer;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "JWT": {
    "Secret": "your-development-secret-key-at-least-32-characters",
    "Issuer": "neo-service-layer",
    "Audience": "neo-service-layer-clients",
    "ExpiryMinutes": 60
  },
  "Enclave": {
    "Enabled": false,
    "CID": 16
  }
}
EOF
    echo "Created appsettings.Development.json"
fi

# Install required .NET tools
echo "Installing required .NET tools..."
dotnet tool install --global dotnet-ef || echo "dotnet-ef is already installed"
dotnet tool install --global dotnet-format || echo "dotnet-format is already installed"

# Restore NuGet packages
echo "Restoring NuGet packages..."
dotnet restore "$BASE_DIR/src/NeoServiceLayer.sln"

# Set up Git hooks
echo "Setting up Git hooks..."
if [ -d ".git" ]; then
    cat > .git/hooks/pre-commit << EOF
#!/bin/bash
set -e

# Run dotnet format
echo "Running dotnet format..."
dotnet format --verify-no-changes

# Run tests
echo "Running tests..."
dotnet test src/NeoServiceLayer.Tests/NeoServiceLayer.Tests.csproj --configuration Debug --no-restore
EOF
    chmod +x .git/hooks/pre-commit
    echo "Git hooks set up successfully"
fi

# Set up database (if Docker is available)
if command -v docker &> /dev/null && command -v docker-compose &> /dev/null; then
    echo "Setting up database and Redis..."
    docker-compose up -d db redis
    
    # Wait for SQL Server to be ready
    echo "Waiting for SQL Server to be ready..."
    sleep 10
    
    # Create database if it doesn't exist
    echo "Creating database if it doesn't exist..."
    docker exec -it $(docker ps -q -f name=db) /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong!Passw0rd -Q "IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'NeoServiceLayer') CREATE DATABASE NeoServiceLayer"
    
    echo "Database and Redis set up successfully"
else
    echo "Skipping database and Redis setup as Docker or Docker Compose is not installed"
fi

echo "Environment setup completed successfully!"
echo "You can now build and run the Neo Service Layer services using the following commands:"
echo "  ./scripts/build_services.sh"
echo "  ./scripts/run_services.sh"
