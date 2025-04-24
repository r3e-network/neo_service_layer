# Neo Service Layer Setup and Run Guide

## Overview

This document provides step-by-step instructions for setting up and running the Neo Service Layer. It covers development environment setup, building the project, and deploying it to AWS with Nitro Enclave support.

## Prerequisites

Before you begin, ensure you have the following installed:

1. **.NET 7.0 SDK or later**: Required for building and running the C# projects
2. **Docker and Docker Compose**: Required for containerization and local development
3. **AWS CLI**: Required for AWS integration
4. **AWS Nitro CLI**: Required for Nitro Enclave management
5. **SQL Server**: Required for database operations
6. **Redis**: Required for caching
7. **Git**: Required for version control

## Development Environment Setup

### 1. Install .NET SDK

#### Windows
1. Download the .NET SDK from [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)
2. Run the installer and follow the instructions

#### macOS
```bash
brew install --cask dotnet-sdk
```

#### Linux (Ubuntu/Debian)
```bash
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y apt-transport-https
sudo apt-get install -y dotnet-sdk-7.0
```

### 2. Install Docker and Docker Compose

#### Windows
1. Download Docker Desktop from [https://www.docker.com/products/docker-desktop](https://www.docker.com/products/docker-desktop)
2. Run the installer and follow the instructions

#### macOS
```bash
brew install --cask docker
```

#### Linux (Ubuntu/Debian)
```bash
sudo apt-get update
sudo apt-get install -y docker.io docker-compose
sudo systemctl enable docker
sudo systemctl start docker
sudo usermod -aG docker $USER
```

### 3. Install AWS CLI

#### Windows
1. Download the AWS CLI installer from [https://aws.amazon.com/cli/](https://aws.amazon.com/cli/)
2. Run the installer and follow the instructions

#### macOS
```bash
brew install awscli
```

#### Linux (Ubuntu/Debian)
```bash
sudo apt-get update
sudo apt-get install -y awscli
```

### 4. Install AWS Nitro CLI

#### Amazon Linux 2
```bash
sudo amazon-linux-extras install aws-nitro-enclaves-cli
sudo yum install aws-nitro-enclaves-cli-devel
sudo usermod -aG ne $USER
sudo usermod -aG docker $USER
```

### 5. Configure AWS Credentials

```bash
aws configure
```

Enter your AWS Access Key ID, Secret Access Key, default region, and output format when prompted.

## Project Setup

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/neo_service_layer.git
cd neo_service_layer
```

### 2. Create Project Structure

```bash
chmod +x scripts/create_project_structure.sh
./scripts/create_project_structure.sh
```

### 3. Build the Solution

```bash
cd src
dotnet build
```

## Local Development

### 1. Start Required Services

```bash
docker-compose up -d
```

This will start SQL Server and Redis containers for local development.

### 2. Run the API

```bash
cd src/NeoServiceLayer.Api
dotnet run
```

The API will be available at `http://localhost:5000`.

### 3. Run Tests

```bash
cd src/NeoServiceLayer.Tests
dotnet test
```

## AWS Deployment

### 1. Create EC2 Instance with Nitro Enclaves Support

1. Log in to the AWS Management Console
2. Navigate to EC2
3. Click "Launch Instance"
4. Select an Amazon Linux 2 AMI
5. Choose an instance type that supports Nitro Enclaves (e.g., c5.xlarge)
6. Configure instance details
   - Enable Nitro Enclaves
   - Allocate at least 2GB of memory for enclaves
7. Add storage (at least 20GB)
8. Configure security groups
   - Allow SSH (port 22)
   - Allow HTTP (port 80)
   - Allow HTTPS (port 443)
9. Launch the instance

### 2. Connect to the Instance

```bash
ssh -i your-key.pem ec2-user@your-instance-ip
```

### 3. Install Required Software

```bash
# Update system
sudo yum update -y

# Install .NET SDK
sudo rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm
sudo yum install -y dotnet-sdk-7.0

# Install Docker
sudo yum install -y docker
sudo systemctl enable docker
sudo systemctl start docker
sudo usermod -aG docker $USER

# Install AWS Nitro Enclaves CLI
sudo amazon-linux-extras install aws-nitro-enclaves-cli
sudo yum install -y aws-nitro-enclaves-cli-devel
sudo usermod -aG ne $USER
sudo usermod -aG docker $USER

# Install Git
sudo yum install -y git

# Log out and log back in for group changes to take effect
exit
```

Log back in:

```bash
ssh -i your-key.pem ec2-user@your-instance-ip
```

### 4. Clone and Build the Project

```bash
git clone https://github.com/yourusername/neo_service_layer.git
cd neo_service_layer
chmod +x scripts/create_project_structure.sh
./scripts/create_project_structure.sh
cd src
dotnet build
```

### 5. Build the Enclave Image

```bash
cd src/NeoServiceLayer.Enclave/Enclave
dotnet publish -c Release -o ./bin/publish

# Create Dockerfile for the enclave
cat > Dockerfile << EOF
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY ./bin/publish .
ENTRYPOINT ["dotnet", "NeoServiceLayer.Enclave.dll"]
EOF

# Build the Docker image
docker build -t neo-service-layer-enclave .

# Create the enclave image file (EIF)
nitro-cli build-enclave --docker-uri neo-service-layer-enclave --output-file neo-service-layer-enclave.eif
```

### 6. Configure the Application

Create or update the configuration files:

```bash
cd ~/neo_service_layer/src/NeoServiceLayer.Api
mkdir -p appsettings
```

Create `appsettings.json`:

```bash
cat > appsettings.json << EOF
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=NeoServiceLayer;User Id=sa;Password=YourStrong!Passw0rd;"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "Enclave": {
    "Path": "/home/ec2-user/neo_service_layer/src/NeoServiceLayer.Enclave/Enclave/neo-service-layer-enclave.eif",
    "Memory": "2048",
    "Cpus": "2"
  },
  "Vsock": {
    "Port": 5000
  }
}
EOF
```

### 7. Run the Application

```bash
cd ~/neo_service_layer/src/NeoServiceLayer.Api
dotnet run
```

### 8. Set Up as a Service

Create a systemd service file:

```bash
sudo cat > /etc/systemd/system/neo-service-layer.service << EOF
[Unit]
Description=Neo Service Layer
After=network.target

[Service]
WorkingDirectory=/home/ec2-user/neo_service_layer/src/NeoServiceLayer.Api
ExecStart=/usr/bin/dotnet run
Restart=always
RestartSec=10
SyslogIdentifier=neo-service-layer
User=ec2-user
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
EOF
```

Enable and start the service:

```bash
sudo systemctl enable neo-service-layer
sudo systemctl start neo-service-layer
```

## Docker Deployment

### 1. Build Docker Images

```bash
cd ~/neo_service_layer
docker-compose build
```

### 2. Run Docker Containers

```bash
docker-compose up -d
```

## Monitoring and Maintenance

### 1. View Logs

```bash
# View API logs
sudo journalctl -u neo-service-layer -f

# View Docker logs
docker-compose logs -f
```

### 2. Check Enclave Status

```bash
nitro-cli describe-enclaves
```

### 3. Restart Services

```bash
# Restart API service
sudo systemctl restart neo-service-layer

# Restart Docker containers
docker-compose restart
```

## Troubleshooting

### 1. Enclave Fails to Start

Check the enclave logs:

```bash
nitro-cli console --enclave-id $(nitro-cli describe-enclaves | jq -r '.[0].EnclaveID')
```

Ensure the enclave has enough memory and CPU resources:

```bash
# Check allocated resources
nitro-cli describe-enclaves

# Terminate the enclave
nitro-cli terminate-enclave --enclave-id $(nitro-cli describe-enclaves | jq -r '.[0].EnclaveID')

# Start with more resources
nitro-cli run-enclave --eif-path /path/to/enclave.eif --memory 4096 --cpu-count 4
```

### 2. API Cannot Connect to Enclave

Check VSOCK communication:

```bash
# On the parent instance
nc -U /tmp/vsock-test.sock

# In another terminal
nitro-cli console --enclave-id $(nitro-cli describe-enclaves | jq -r '.[0].EnclaveID')
nc -l -U /tmp/vsock-test.sock
```

### 3. Database Connection Issues

Check SQL Server connection:

```bash
# Check if SQL Server is running
docker ps | grep sql

# Check connection string in appsettings.json
cat ~/neo_service_layer/src/NeoServiceLayer.Api/appsettings.json
```

## Security Best Practices

### 1. Keep Software Updated

Regularly update the system and dependencies:

```bash
sudo yum update -y
dotnet restore
```

### 2. Secure Configuration

- Store sensitive configuration in AWS Secrets Manager
- Use environment variables for sensitive values
- Encrypt configuration files

### 3. Monitor for Security Events

- Enable AWS CloudTrail
- Set up alerts for suspicious activities
- Regularly review logs

### 4. Implement Proper Access Control

- Use IAM roles with least privilege
- Implement proper authentication and authorization
- Regularly rotate credentials

## Conclusion

You have successfully set up and deployed the Neo Service Layer. The system is now ready to provide serverless functions and blockchain services for Neo N3. For more information on using the system, refer to the other documentation files in the `docs` directory.
