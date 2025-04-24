#!/bin/bash

# Neo Service Layer Deployment Script
# This script deploys the Neo Service Layer services to production

# Exit on error
set -e

# Set the base directory to the current directory
BASE_DIR=$(pwd)

echo "Deploying Neo Service Layer services to production..."

# Check if AWS CLI is installed
if ! command -v aws &> /dev/null; then
    echo "Error: AWS CLI is not installed. Please install AWS CLI from https://aws.amazon.com/cli/"
    exit 1
fi

# Check if AWS CLI is configured
if ! aws sts get-caller-identity &> /dev/null; then
    echo "Error: AWS CLI is not configured. Please configure AWS CLI with 'aws configure'"
    exit 1
fi

# Deployment environment (dev, staging, or prod)
DEPLOY_ENV=${1:-dev}
echo "Deployment environment: $DEPLOY_ENV"

# AWS region
AWS_REGION=${2:-us-east-1}
echo "AWS region: $AWS_REGION"

# Build the services for production
echo "Building services for production..."
./scripts/build_services.sh Release

# Create deployment package
echo "Creating deployment package..."
TIMESTAMP=$(date +%Y%m%d%H%M%S)
DEPLOY_PACKAGE="neo-service-layer-$DEPLOY_ENV-$TIMESTAMP.zip"

mkdir -p "$BASE_DIR/dist/deploy"
cd "$BASE_DIR/dist"
zip -r "deploy/$DEPLOY_PACKAGE" api enclave

echo "Deployment package created: $BASE_DIR/dist/deploy/$DEPLOY_PACKAGE"

# Upload deployment package to S3
echo "Uploading deployment package to S3..."
S3_BUCKET="neo-service-layer-$DEPLOY_ENV-deployments"
S3_KEY="$DEPLOY_PACKAGE"

# Create S3 bucket if it doesn't exist
if ! aws s3api head-bucket --bucket "$S3_BUCKET" --region "$AWS_REGION" 2>/dev/null; then
    echo "Creating S3 bucket: $S3_BUCKET..."
    aws s3api create-bucket --bucket "$S3_BUCKET" --region "$AWS_REGION" --create-bucket-configuration LocationConstraint="$AWS_REGION"
fi

# Upload deployment package
aws s3 cp "$BASE_DIR/dist/deploy/$DEPLOY_PACKAGE" "s3://$S3_BUCKET/$S3_KEY" --region "$AWS_REGION"

echo "Deployment package uploaded to S3: s3://$S3_BUCKET/$S3_KEY"

# Deploy to EC2 instances (if applicable)
if [ "$DEPLOY_ENV" == "prod" ] || [ "$DEPLOY_ENV" == "staging" ]; then
    echo "Deploying to EC2 instances..."
    
    # Get EC2 instance IDs
    INSTANCE_IDS=$(aws ec2 describe-instances \
        --filters "Name=tag:Environment,Values=$DEPLOY_ENV" "Name=tag:Service,Values=neo-service-layer" "Name=instance-state-name,Values=running" \
        --query "Reservations[].Instances[].InstanceId" \
        --output text \
        --region "$AWS_REGION")
    
    if [ -z "$INSTANCE_IDS" ]; then
        echo "No EC2 instances found for environment: $DEPLOY_ENV"
    else
        echo "Found EC2 instances: $INSTANCE_IDS"
        
        # Deploy to each instance using SSM Run Command
        for INSTANCE_ID in $INSTANCE_IDS; do
            echo "Deploying to instance: $INSTANCE_ID..."
            
            # Create SSM document for deployment
            COMMAND_ID=$(aws ssm send-command \
                --instance-ids "$INSTANCE_ID" \
                --document-name "AWS-RunShellScript" \
                --parameters "commands=[
                    \"cd /opt/neo-service-layer\",
                    \"aws s3 cp s3://$S3_BUCKET/$S3_KEY .\",
                    \"unzip -o $DEPLOY_PACKAGE -d .\",
                    \"sudo systemctl restart neo-service-layer-api\",
                    \"sudo systemctl restart neo-service-layer-enclave\"
                ]" \
                --output text \
                --query "Command.CommandId" \
                --region "$AWS_REGION")
            
            echo "Deployment command sent to instance $INSTANCE_ID with command ID: $COMMAND_ID"
            
            # Wait for command to complete
            echo "Waiting for deployment to complete..."
            aws ssm wait command-executed --command-id "$COMMAND_ID" --instance-id "$INSTANCE_ID" --region "$AWS_REGION"
            
            # Check command status
            STATUS=$(aws ssm get-command-invocation \
                --command-id "$COMMAND_ID" \
                --instance-id "$INSTANCE_ID" \
                --query "Status" \
                --output text \
                --region "$AWS_REGION")
            
            if [ "$STATUS" == "Success" ]; then
                echo "Deployment to instance $INSTANCE_ID completed successfully."
            else
                echo "Error: Deployment to instance $INSTANCE_ID failed with status: $STATUS"
                echo "Check the command output for details:"
                aws ssm get-command-invocation \
                    --command-id "$COMMAND_ID" \
                    --instance-id "$INSTANCE_ID" \
                    --query "StandardOutputContent" \
                    --output text \
                    --region "$AWS_REGION"
                exit 1
            fi
        done
    fi
fi

echo "Deployment completed successfully!"
