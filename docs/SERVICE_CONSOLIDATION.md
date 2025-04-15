# Service Documentation Consolidation

*Last Updated: 2023-12-05*

## Overview

This document records consolidation of duplicate service documentation in the Neo Service Layer.

## Secrets Service and Secrets Service Consolidation

The Secrets Service and Secrets Service documentations have been consolidated as they refer to the same service. The service is officially known as the **Secrets Service**, with its primary responsibility being to manage user-set secrets and make them available for function execution in Trusted Execution Environments (TEEs).

### Reasons for Consolidation

1. Both services described essentially the same functionality:
   - Management of sensitive data (secrets/keys)
   - Integration with TEEs for secure access
   - Access control for sensitive information

2. The Secrets Service documentation was more recent (December 2023) but shared core concepts with the older Secrets Service documentation (July 2023).

### Consolidation Actions

1. Removed Secrets Service section from the main README.md
2. Retained Secrets Service as the canonical name
3. Merged content from Secrets Service documentation into Secrets Service files where relevant
4. Updated cross-references in other documentation

### Directory Status

- `/docs/secretservice/` - Active and maintained
- `/docs/secretservice/` - Deprecated, to be removed

## Next Steps

Any references to "Secrets Service" in code, configuration, or documentation should be updated to reference "Secrets Service" for consistency.
