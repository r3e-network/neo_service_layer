#!/bin/bash

# Neo Service Layer Enclave Setup Script
# This script sets up the AWS Nitro Enclave environment for the Neo Service Layer project

# Exit on error
set -e

# Set the base directory to the current directory
BASE_DIR=$(pwd)

echo "Setting up AWS Nitro Enclave environment for Neo Service Layer..."

# Check if running on AWS
if ! curl -s http://169.254.169.254/latest/meta-data/ &> /dev/null; then
    echo "Warning: This script is designed to run on an AWS EC2 instance with Nitro Enclave support."
    echo "You may be running this script on a non-AWS environment."
    echo "Continuing with setup, but some steps may fail."
fi

# Check for AWS CLI
if ! command -v aws &> /dev/null; then
    echo "Error: AWS CLI is not installed. Please install AWS CLI from https://aws.amazon.com/cli/"
    exit 1
fi

# Check for Nitro CLI
if ! command -v nitro-cli &> /dev/null; then
    echo "Installing Nitro CLI..."
    sudo amazon-linux-extras install aws-nitro-enclaves-cli -y
    sudo yum install aws-nitro-enclaves-cli-devel -y
    sudo usermod -aG ne $USER
    echo "Nitro CLI installed successfully. Please log out and log back in for group changes to take effect."
    exit 0
fi

# Check Nitro Enclave allocator configuration
echo "Checking Nitro Enclave allocator configuration..."
if ! grep -q "^memory_mib: 4096" /etc/nitro_enclaves/allocator.yaml; then
    echo "Configuring Nitro Enclave allocator..."
    sudo sed -i 's/^memory_mib: .*/memory_mib: 4096/' /etc/nitro_enclaves/allocator.yaml
    sudo sed -i 's/^cpu_count: .*/cpu_count: 2/' /etc/nitro_enclaves/allocator.yaml
    sudo systemctl restart nitro-enclaves-allocator.service
    echo "Nitro Enclave allocator configured successfully."
fi

# Create the Enclave EIF (Enclave Image File)
echo "Building Enclave EIF..."
mkdir -p "$BASE_DIR/dist/eif"

# Create a Dockerfile for the enclave
cat > "$BASE_DIR/dist/eif/Dockerfile.enclave" << EOF
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
COPY ./enclave/ .
ENTRYPOINT ["dotnet", "NeoServiceLayer.Enclave.dll"]
EOF

# Build the enclave image
cd "$BASE_DIR/dist/eif"
docker build -t neo-service-layer-enclave -f Dockerfile.enclave ..

# Create the EIF file
nitro-cli build-enclave --docker-uri neo-service-layer-enclave --output-file neo-service-layer-enclave.eif

echo "Enclave EIF built successfully: $BASE_DIR/dist/eif/neo-service-layer-enclave.eif"

# Create a script to run the enclave
cat > "$BASE_DIR/scripts/run_enclave.sh" << EOF
#!/bin/bash

# Neo Service Layer Enclave Run Script
# This script runs the Neo Service Layer enclave

# Exit on error
set -e

# Set the base directory to the current directory
BASE_DIR=\$(pwd)

echo "Running Neo Service Layer enclave..."

# Check if the EIF file exists
if [ ! -f "\$BASE_DIR/dist/eif/neo-service-layer-enclave.eif" ]; then
    echo "Error: Enclave EIF file not found. Please run the setup_enclave.sh script first."
    exit 1
fi

# Run the enclave
echo "Starting enclave..."
ENCLAVE_ID=\$(nitro-cli run-enclave --eif-path "\$BASE_DIR/dist/eif/neo-service-layer-enclave.eif" --memory 4096 --cpu-count 2 --debug-mode)

echo "Enclave started with ID: \$ENCLAVE_ID"
echo "You can check the enclave console output with:"
echo "  nitro-cli console --enclave-id \$ENCLAVE_ID"
echo ""
echo "To stop the enclave, run:"
echo "  nitro-cli terminate-enclave --enclave-id \$ENCLAVE_ID"
EOF

chmod +x "$BASE_DIR/scripts/run_enclave.sh"

echo "Enclave setup completed successfully!"
echo "You can now run the enclave using the following command:"
echo "  ./scripts/run_enclave.sh"
