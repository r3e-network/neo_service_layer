# Documentation Cleanup Plan

*Last Updated: 2025-04-14*

This document defines the systematic cleanup process for Neo Service Layer documentation.

## Identified Issues

1. **Duplication of Documentation**
   - Service duplication (secretservice vs secretservice)
   - Inconsistent naming conventions
   - Different file structures across services

2. **Inconsistent File Naming**
   - Mixed use of lowercase-with-hyphens and ALL_CAPS_WITH_UNDERSCORES
   - Inconsistent file naming patterns

3. **Missing Standard Files**
   - Incomplete standard file sets in service directories
   - Inconsistent file presence across services

4. **Missing Last Updated Fields**
   - Many files missing Last Updated dates
   - Outdated dates in critical files

5. **Broken Internal Links**
   - Links to non-existent or renamed files
   - References to outdated structures

## Implementation Timeline

### Phase 1: Foundational Cleanup (Due: 2025-04-30)

1. **Define and Document Standards**
   - [x] Verify TEMPLATE_SERVICE_DOCS.md is accurate
   - [x] Decide on OpenAPI/Swagger documentation approach
   - [ ] Determine standard approach for extra files (QUICKSTART.md, DIAGRAMS.md)

2. **Complete Key/Secrets Service Consolidation**
   - [ ] Move unique content from secretservice into secretservice
   - [ ] Delete secretservice directory
   - [ ] Update all references to secretservice

3. **Enforce Naming Conventions**
   - [ ] Rename all files to match ALL_CAPS_WITH_UNDERSCORES.md
   - [ ] Update internal links after renaming
   - [ ] Run link checking to verify no broken links

4. **Establish Standard File Structure**
   - [ ] Create missing standard files for all services
   - [ ] Use templates for consistent structure

### Phase 2: Content Harmonization (Due: 2025-05-15)

1. **Refactor API Documentation**
   - [ ] Restructure API_SPECIFICATIONS.md to focus on standards only
   - [ ] Ensure each service has complete API_REFERENCE.md
   - [ ] Standardize OpenAPI/Swagger documentation

2. **Populate Standard Files**
   - [ ] Move content from non-standard files into standard locations
   - [ ] Ensure all standard sections are present

3. **Update Last Updated Dates**
   - [ ] Add or update dates in all documentation files
   - [ ] Establish system for date maintenance

4. **Review Cross-Cutting Documents**
   - [ ] Update ARCHITECTURE_OVERVIEW.md for accuracy
   - [ ] Update PROJECT_STRUCTURE.md to match actual structure

### Phase 3: Polish & Process Improvement (Due: 2025-05-31)

1. **Fix Markdown Linting Issues**
   - [ ] Address formatting errors from lint_results.md
   - [ ] Standardize code blocks and lists

2. **Standardize Extra Files**
   - [ ] Make decisions on non-standard files
   - [ ] Apply consistent naming to extras

3. **Update Meta-Documentation**
   - [ ] Ensure README.md is accurate
   - [ ] Update TEMPLATE_SERVICE_DOCS.md if needed

4. **Improve Documentation Process**
   - [ ] Integrate documentation linting into CI/CD
   - [ ] Define documentation ownership
   - [ ] Add documentation to Definition of Done

## Ownership and Tracking

Each service team is responsible for implementing these changes for their service documentation. The Documentation Team will coordinate and track progress.

Progress will be tracked in the weekly team meetings and through the documentation cleanup project board.

## Verification Process

After each phase, a documentation review will be conducted to verify:

1. All standards are correctly applied
2. No broken links exist
3. Content is accurate and complete
4. Last Updated dates are current

A final review will be conducted at the end of Phase 3 to ensure all issues have been addressed.
