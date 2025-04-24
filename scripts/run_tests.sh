#!/bin/bash

# Neo Service Layer Test Script
# This script runs all the tests in the Neo Service Layer project

# Exit on error
set -e

# Set the base directory to the current directory
BASE_DIR=$(pwd)

echo "Running Neo Service Layer tests..."

# Test configuration (Debug or Release)
TEST_CONFIG=${1:-Debug}
echo "Test configuration: $TEST_CONFIG"

# Test filter (optional)
TEST_FILTER=${2:-""}
if [ -n "$TEST_FILTER" ]; then
    echo "Test filter: $TEST_FILTER"
    FILTER_ARG="--filter $TEST_FILTER"
else
    FILTER_ARG=""
fi

# Run all tests in the tests directory
echo "Running all tests..."
cd "$BASE_DIR/tests"

# Find all test projects
TEST_PROJECTS=$(find . -name "*.csproj" | sort)

# Run each test project
for PROJECT in $TEST_PROJECTS; do
    echo "Running tests in $PROJECT..."
    dotnet test "$PROJECT" --configuration $TEST_CONFIG $FILTER_ARG
done

echo "All tests completed successfully!"
