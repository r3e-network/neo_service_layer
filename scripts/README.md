# Neo Service Layer Scripts

This directory contains scripts for setting up, building, running, and deploying the Neo Service Layer services.

## Available Scripts

### Setup Scripts

- **setup_all.sh**: Master script that runs all the setup scripts in the correct order
- **setup_environment.sh**: Sets up the development environment for the Neo Service Layer project
- **setup_js_runtime.sh**: Sets up the JavaScript runtime environment for the function service
- **setup_enclave.sh**: Sets up the AWS Nitro Enclave environment for the Neo Service Layer project
- **create_project_structure.sh**: Creates the directory structure and empty files for the Neo Service Layer project

### Build and Run Scripts

- **build_services.sh**: Builds all the services in the Neo Service Layer project
- **run_services.sh**: Runs the Neo Service Layer services
- **run_tests.sh**: Runs all the tests in the Neo Service Layer project
- **run_enclave.sh**: Runs the Neo Service Layer enclave (created by setup_enclave.sh)

### Deployment Scripts

- **deploy_services.sh**: Deploys the Neo Service Layer services to production

## Usage

### Setting Up the Project

To set up the entire project, run:

```bash
./scripts/setup_all.sh
```

This will:
1. Create the project structure (if needed)
2. Set up the development environment
3. Set up the JavaScript runtime
4. Build the services
5. Run the tests

### Building and Running

To build the services:

```bash
./scripts/build_services.sh [Debug|Release]
```

To run the services:

```bash
./scripts/run_services.sh [api|enclave|all]
```

To run the tests:

```bash
./scripts/run_tests.sh [Debug|Release] [TestFilter]
```

### Enclave Support

To set up the AWS Nitro Enclave environment:

```bash
./scripts/setup_enclave.sh
```

To run the enclave:

```bash
./scripts/run_enclave.sh
```

### Deployment

To deploy the services to production:

```bash
./scripts/deploy_services.sh [dev|staging|prod] [aws-region]
```

## Notes

- All scripts are designed to be run from the root directory of the project
- The scripts assume a Unix-like environment (Linux or macOS)
- For Windows, consider using Windows Subsystem for Linux (WSL) or Git Bash
