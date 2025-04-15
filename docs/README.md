# Neo Service Layer Documentation

*Last Updated: 2023-12-20*

This directory contains documentation for all services and components of the Neo Service Layer.

## Documentation Structure

Documentation is organized by service, with each service having its own directory:

```
docs/
├── README.md                       # This file
├── ARCHITECTURE_OVERVIEW.md        # Overall system architecture
├── SERVICE_INTEGRATION.md          # Service integration guidelines
├── SECURITY_MODEL.md               # Security architecture and model
├── DOCUMENTATION_IMPROVEMENT_PLAN.md # Plan for improving documentation
├── TEMPLATE_SERVICE_DOCS.md        # Template for service documentation
├── OPERATIONS.md                   # System operations procedures
├── DEPLOYMENT.md                   # Deployment configurations
├── API_SPECIFICATIONS.md           # API specifications and standards
├── tee/                            # TEE documentation
│   ├── GUIDE.md                    # Comprehensive guide to TEE
│   ├── ATTESTATION.md              # Attestation flows and security
│   └── PROVIDERS.md                # TEE provider implementations
├── security/                       # Security documentation
│   └── ENCRYPTION.md               # Encryption design and implementation
├── apiservice/                     # API Service documentation
│   ├── OVERVIEW.md                 # Service overview
│   ├── ARCHITECTURE.md             # Service architecture
│   ├── API_REFERENCE.md            # API specifications
│   ├── IMPLEMENTATION.md           # Implementation details
│   ├── CONFIGURATION.md            # Configuration parameters
│   └── OPERATIONS.md               # Operational procedures
└── ...                             # Other service documentation directories
```

## Documentation Standards

Each service documentation directory must include at minimum:

1. **OVERVIEW.md** - High-level overview of the service, its purpose, and key features
2. **ARCHITECTURE.md** - Detailed architectural design of the service
3. **API_REFERENCE.md** - API specifications and usage examples
4. **IMPLEMENTATION.md** - Implementation details and technology choices
5. **CONFIGURATION.md** - Configuration parameters and options

Optional service documentation:

6. **OPERATIONS.md** - Service-specific operational procedures (if they differ from system-wide operations)

See `TEMPLATE_SERVICE_DOCS.md` for detailed templates for each of these files.

## Directory Naming Convention

All service documentation directories must:

1. Use lowercase names (e.g., `apiservice`, not `ApiService` or `API_SERVICE`)
2. Not include underscores or special characters
3. Directly match the service name used in the codebase

## File Naming Convention

All documentation files MUST:

1. Use ALL_CAPS for filenames
2. Use underscores (_) to separate words
3. Have the .md extension (Markdown)

Examples: `OVERVIEW.md`, `API_REFERENCE.md`, `IMPLEMENTATION.md`

## Contributing Documentation

When adding or updating documentation:

1. Follow the established directory structure and naming conventions
2. Use the templates provided in `TEMPLATE_SERVICE_DOCS.md`
3. Include appropriate diagrams where helpful
4. Cross-reference related documentation
5. Update this README if necessary
6. Update the "Last Updated" field at the top of each file

## Documentation Ownership and Maintenance

Every documentation file has an assigned owner responsible for keeping it up to date. Documentation updates are required as part of the definition-of-done for feature/bugfix PRs that affect the documented components.

The entire documentation set undergoes a quarterly review for accuracy and consistency.

## Documentation Linting

Documentation is automatically linted to ensure consistency. The linter checks for:

1. Consistent headers and structure
2. Valid internal links
3. Proper formatting
4. Markdown style compliance

Run the linter with:

```bash
scripts/lint_docs.sh
```

## Core Documentation

- [Architecture Overview](ARCHITECTURE_OVERVIEW.md) - Complete system architecture and interservice communication
- [Security Model](SECURITY_MODEL.md) - Security architecture and considerations
- [Service Integration](SERVICE_INTEGRATION.md) - Patterns for secure service-to-service integration
- [API Specifications](API_SPECIFICATIONS.md) - Overall API specifications and standards
- [Operations Guide](OPERATIONS.md) - Operational procedures and maintenance
- [Deployment Guide](DEPLOYMENT.md) - Deployment configurations and procedures
- [Project Structure](PROJECT_STRUCTURE.md) - Organization of the codebase
- [Documentation Improvement Plan](DOCUMENTATION_IMPROVEMENT_PLAN.md) - Plan for improving documentation

## TEE Documentation

- [TEE Guide](tee/GUIDE.md) - Comprehensive guide to Trusted Execution Environments
- [TEE Attestation](tee/ATTESTATION.md) - Attestation flows and security guarantees
- [TEE Providers](tee/PROVIDERS.md) - Supported TEE provider implementations

## Security Documentation

- [Encryption](security/ENCRYPTION.md) - Encryption design and implementation

## Service Documentation

### API Service

- [Overview](apiservice/OVERVIEW.md) - API Service overview
- [Architecture](apiservice/ARCHITECTURE.md) - API Service architecture
- [API Reference](apiservice/API_REFERENCE.md) - API Service API reference
- [Implementation](apiservice/IMPLEMENTATION.md) - API Service implementation details
- [Configuration](apiservice/CONFIGURATION.md) - API Service configuration parameters

### Automation Service

- [Overview](automationservice/OVERVIEW.md) - Automation Service overview
- [Architecture](automationservice/ARCHITECTURE.md) - Automation Service architecture
- [API Reference](automationservice/API_REFERENCE.md) - Automation Service API reference
- [Implementation](automationservice/IMPLEMENTATION.md) - Automation Service implementation details
- [Configuration](automationservice/CONFIGURATION.md) - Automation Service configuration

### Functions Service

- [Overview](functionservice/OVERVIEW.md) - Functions Service overview
- [Architecture](functionservice/ARCHITECTURE.md) - Functions Service architecture
- [API Reference](functionservice/API_REFERENCE.md) - Functions Service API reference
- [Implementation](functionservice/IMPLEMENTATION.md) - Functions Service implementation details
- [TEE Integration](functionservice/TEE_INTEGRATION.md) - TEE integration in Functions Service
- [Runtimes](functionservice/RUNTIMES.md) - Supported runtimes and environments
- [Configuration](functionservice/CONFIGURATION.md) - Functions Service configuration

### Gas Bank Service

- [Overview](gasbankservice/OVERVIEW.md) - Gas Bank Service overview
- [Architecture](gasbankservice/ARCHITECTURE.md) - Gas Bank Service architecture
- [API Reference](gasbankservice/API_REFERENCE.md) - Gas Bank Service API reference
- [Implementation](gasbankservice/IMPLEMENTATION.md) - Gas Bank Service implementation details
- [Configuration](gasbankservice/CONFIGURATION.md) - Gas Bank Service configuration

### Price Feed Service

- [Overview](pricefeedservice/OVERVIEW.md) - Price Feed Service overview
- [Architecture](pricefeedservice/ARCHITECTURE.md) - Price Feed Service architecture
- [API Reference](pricefeedservice/API_REFERENCE.md) - Price Feed Service API reference
- [Implementation](pricefeedservice/IMPLEMENTATION.md) - Price Feed Service implementation details
- [Data Sources](pricefeedservice/DATA_SOURCES.md) - Supported price data sources
- [Configuration](pricefeedservice/CONFIGURATION.md) - Price Feed Service configuration

### Secrets Service

- [Overview](secretservice/OVERVIEW.md) - Secrets Service overview
- [Architecture](secretservice/ARCHITECTURE.md) - Secrets Service architecture
- [API Reference](secretservice/API_REFERENCE.md) - Secrets Service API reference
- [Implementation](secretservice/IMPLEMENTATION.md) - Secrets Service implementation details
- [Configuration](secretservice/CONFIGURATION.md) - Secrets Service configuration
- [Security Model](secretservice/SECURITY_MODEL.md) - Secrets Service security model

## Documentation Conventions

All documentation follows these conventions:

- Each document begins with a clear title and Last Updated date
- Each service directory contains the required standard files
- Cross-references between documents use direct links
- Diagrams use consistent styling (ASCII art in code blocks or Mermaid)
- All filenames follow UPPERCASE_WITH_UNDERSCORES.md format

## Documentation Review Cycle

Documentation is regularly reviewed according to this schedule:

- Quarterly full review of all documentation
- Monthly consistency checks
- Updates with each software release
- Verification after any service architecture changes

## Changelog

- 2023-07-12: Initial documentation index created
- 2023-07-31: Documentation cleanup and standardization
- 2023-08-15: Updated documentation structure to follow new standards
- 2023-12-05: Consolidated Secrets Service documentation into Secrets Service
- 2023-12-20: Updated documentation standards and structure according to DOCUMENTATION_IMPROVEMENT_PLAN.md
