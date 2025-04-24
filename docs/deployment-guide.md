# Neo Service Layer Deployment Guide

This guide outlines the steps to deploy the Neo Service Layer to a production environment.

## Prerequisites

- AWS account with permissions to create and manage Nitro Enclaves
- EC2 instance with Nitro Enclaves support (e.g., c5.xlarge or larger)
- Docker installed on the EC2 instance
- AWS CLI installed and configured
- .NET 6.0 SDK or later

## Building the Enclave Image

1. Clone the repository:
   ```bash
   git clone https://github.com/your-org/neo-service-layer.git
   cd neo-service-layer
   ```

2. Build the enclave application:
   ```bash
   dotnet publish -c Release -o ./publish src/NeoServiceLayer.Enclave/NeoServiceLayer.Enclave.csproj
   ```

3. Build the Docker image:
   ```bash
   docker build -t neo-service-layer-enclave -f Dockerfile.enclave .
   ```

4. Convert the Docker image to an enclave image:
   ```bash
   nitro-cli build-enclave --docker-uri neo-service-layer-enclave:latest --output-file neo-service-layer-enclave.eif
   ```

## Building the Parent Application

1. Build the parent application:
   ```bash
   dotnet publish -c Release -o ./publish src/NeoServiceLayer.Parent/NeoServiceLayer.Parent.csproj
   ```

2. Build the Docker image:
   ```bash
   docker build -t neo-service-layer-parent -f Dockerfile.parent .
   ```

## Deploying to AWS

### Option 1: Manual Deployment

1. Copy the enclave image to the EC2 instance:
   ```bash
   scp neo-service-layer-enclave.eif ec2-user@your-ec2-instance:/path/to/enclave/
   ```

2. SSH into the EC2 instance:
   ```bash
   ssh ec2-user@your-ec2-instance
   ```

3. Run the enclave:
   ```bash
   nitro-cli run-enclave --eif-path /path/to/enclave/neo-service-layer-enclave.eif --memory 4096 --cpu-count 2 --debug-mode
   ```

4. Run the parent application:
   ```bash
   docker run -d --name neo-service-layer-parent -p 8080:80 neo-service-layer-parent
   ```

### Option 2: Automated Deployment with AWS CDK

1. Install the AWS CDK:
   ```bash
   npm install -g aws-cdk
   ```

2. Navigate to the CDK directory:
   ```bash
   cd deployment/cdk
   ```

3. Install dependencies:
   ```bash
   npm install
   ```

4. Deploy the stack:
   ```bash
   cdk deploy
   ```

## Configuration

### Environment Variables

The Neo Service Layer can be configured using environment variables:

#### Parent Application

| Variable | Description | Default |
|----------|-------------|---------|
| `NSL_API_PORT` | Port for the API server | 8080 |
| `NSL_LOG_LEVEL` | Log level (Debug, Info, Warning, Error) | Info |
| `NSL_JWT_SECRET` | Secret for JWT token signing | (required) |
| `NSL_ENCLAVE_CID` | CID of the enclave | 16 |
| `NSL_ENCLAVE_PORT` | Port of the enclave | 5000 |

#### Enclave Application

| Variable | Description | Default |
|----------|-------------|---------|
| `NSL_ENCLAVE_PORT` | Port for the enclave server | 5000 |
| `NSL_LOG_LEVEL` | Log level (Debug, Info, Warning, Error) | Info |
| `NSL_NEO_RPC_URL` | URL of the Neo N3 RPC server | http://seed1.neo.org:10332 |
| `NSL_NEO_NETWORK_MAGIC` | Network magic number for Neo N3 | 860833102 (MainNet) |

### Configuration Files

Configuration can also be provided using JSON configuration files:

#### Parent Application

```json
{
  "Api": {
    "Port": 8080
  },
  "Logging": {
    "LogLevel": "Info"
  },
  "Jwt": {
    "Secret": "your-secret-key",
    "Issuer": "NeoServiceLayer",
    "Audience": "NeoServiceLayerApi",
    "ExpirationMinutes": 60
  },
  "Enclave": {
    "Cid": 16,
    "Port": 5000
  }
}
```

#### Enclave Application

```json
{
  "Enclave": {
    "Port": 5000
  },
  "Logging": {
    "LogLevel": "Info"
  },
  "Neo": {
    "RpcUrl": "http://seed1.neo.org:10332",
    "NetworkMagic": 860833102
  }
}
```

## Monitoring

### Health Checks

The Neo Service Layer provides health check endpoints:

- Parent application: `GET /health`
- Enclave application: Not directly accessible, but monitored by the parent application

### Metrics

Metrics are exposed via Prometheus endpoints:

- Parent application: `GET /metrics`
- Enclave application: Not directly accessible, but metrics are forwarded to the parent application

### Logging

Logs are written to stdout/stderr and can be collected using standard Docker/AWS logging mechanisms.

## Security Considerations

### Secrets Management

- Use AWS Secrets Manager or similar service to store sensitive configuration values
- Do not hardcode secrets in the application code or configuration files
- Rotate secrets regularly

### Network Security

- Use a private subnet for the EC2 instance
- Use security groups to restrict access to the EC2 instance
- Use HTTPS for all external communication

### Enclave Security

- Keep the enclave image up to date
- Use the minimum required memory and CPU for the enclave
- Use debug mode only for development and testing

## Troubleshooting

### Common Issues

#### Enclave Fails to Start

- Check the enclave logs: `nitro-cli console --enclave-id <enclave-id>`
- Ensure the enclave has enough memory and CPU
- Verify the enclave image is built correctly

#### Parent Application Cannot Connect to Enclave

- Verify the enclave is running: `nitro-cli describe-enclaves`
- Check the CID and port configuration
- Ensure the parent application has permission to connect to the enclave

#### API Requests Fail

- Check the parent application logs
- Verify the JWT configuration
- Ensure the API server is running and accessible

## Conclusion

This guide provides the basic steps to deploy the Neo Service Layer to a production environment. For more detailed information, refer to the documentation in the `docs/` directory.
