# TODOs and Implementation Notes

This document lists the current TODOs and implementation notes for the Neo Service Layer project. These items should be addressed in future releases.

## Core Services

### Metrics Service
- Implement metric recording
- Implement metric retrieval
- Implement dashboard creation and retrieval
- Implement alert creation and retrieval

### Enclave Service
- Implement attestation document verification
- Implement actual enclave communication

### Function Service
- Implement template functionality
- Implement upload functionality
- Implement ZIP functionality
- Implement YAML parsing and serialization
- Implement more sophisticated test generation and coverage analysis

### Function Dependency Service
- Implement actual installation logic
- Implement update checking and update logic
- Implement validation logic

### Function Composition Service
- Implement output mappings
- Implement condition evaluation

### Wallet Service
- Implement Neo RPC client to get balance

## Common Utilities
- Implement string extensions
- Implement encryption utilities
- Implement JSON serialization utilities

## Tests
- Implement API integration tests

## External Dependencies
- Most TODOs in the external/neo directory are related to the Neo blockchain implementation and should be addressed by the Neo team.

## Next Steps

1. Prioritize these TODOs based on their importance for the core functionality
2. Create issues in the GitHub repository for each TODO
3. Assign issues to team members
4. Track progress in the project board

This list was generated using the `./scripts/check_todos.sh` script, which can be run at any time to get an updated list of TODOs in the codebase.
