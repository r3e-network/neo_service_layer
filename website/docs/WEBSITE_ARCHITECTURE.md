# Neo Service Layer Website Architecture

## Directory Structure Overview

### Core Directories

#### `/src`
- Primary source code directory for the Next.js application
- Contains reusable components, layouts, and core functionality

#### `/pages`
- Next.js pages directory
- Contains all route definitions and page components
- Follows Next.js file-based routing convention

#### `/components`
- Reusable React components
- UI building blocks used across multiple pages
- Should follow atomic design principles

#### `/services`
- Service layer implementations
- API integrations and data fetching logic
- Contains service-specific code for:
  - Price Feed Service
  - Gas Bank Service
  - Trigger Service
  - Metrics Service
  - Logging Service
  - Secrets Service
  - Functions Service
  - API Service

#### `/hooks`
- Custom React hooks
- Shared stateful logic
- Service integration hooks

#### `/utils`
- Utility functions and helpers
- Common constants
- Type definitions

#### `/content`
- Static content and documentation
- Markdown files
- Configuration files for content

#### `/docs`
- Project documentation
- Architecture documents
- API specifications
- Development guides

### Testing Directories

#### `/__tests__`
- Jest test configurations
- Unit tests
- Integration tests

#### `/tests`
- E2E tests
- Test utilities
- Test fixtures

### Configuration Files

- `package.json` - Project dependencies and scripts
- `tsconfig.json` - TypeScript configuration
- `next.config.js` - Next.js configuration
- `tailwind.config.js` - Tailwind CSS configuration
- `jest.config.js` - Jest testing configuration
- `netlify.toml` - Netlify deployment configuration

## Implementation Status

### ✅ Completed
1. Project structure setup
2. Basic Next.js configuration
3. Testing infrastructure
4. Documentation framework
5. Deployment configuration

### 🚧 In Progress
1. Service integration components
2. API integration services
3. User authentication flow
4. Error handling utilities

### ❌ Missing/Todo

#### Core Features
1. Price Feed Service UI
   - Real-time price display
   - Historical price charts
   - Price feed configuration interface

2. Gas Bank Service UI
   - Gas balance display
   - Gas management interface
   - Transaction history

3. Trigger Service UI
   - Event monitoring dashboard
   - Trigger configuration interface
   - Event history and logs

4. Metrics Service UI
   - Performance metrics dashboard
   - System health monitoring
   - Alert configuration

5. Logging Service UI
   - Log viewer interface
   - Log search and filtering
   - Log level configuration

6. Secrets Service UI
   - Secrets management interface
   - Permission configuration
   - Audit logs

7. Functions Service UI
   - Function creation interface
   - Function deployment workflow
   - Function monitoring dashboard

8. API Service UI
   - API documentation
   - API key management
   - Usage statistics

#### Authentication & Security
1. Neo N3 wallet integration
2. Message signing implementation
3. Signature verification flow
4. Permission management system

#### Documentation
1. API documentation
2. Service integration guides
3. User guides for each service
4. Development setup guide
5. Contribution guidelines update

#### Testing
1. Unit tests for all components
2. Integration tests for services
3. E2E tests for critical flows
4. Performance testing suite

## Next Steps

1. Implement core service UIs in priority order:
   - Price Feed Service (highest priority)
   - Gas Bank Service
   - Functions Service
   - Trigger Service

2. Develop authentication system:
   - Wallet integration
   - Message signing
   - Permission management

3. Create comprehensive documentation:
   - API specifications
   - Service integration guides
   - User documentation

4. Implement testing suite:
   - Unit tests
   - Integration tests
   - E2E tests

## Development Guidelines

1. Follow atomic design principles for components
2. Implement proper error boundaries
3. Use TypeScript for all new code
4. Write tests for all new features
5. Update documentation with code changes
6. Follow accessibility guidelines
7. Implement proper loading states
8. Add proper error handling
9. Include proper logging
10. Follow security best practices

## Performance Considerations

1. Implement proper caching strategies
2. Use proper code splitting
3. Optimize images and assets
4. Implement proper lazy loading
5. Monitor and optimize bundle size
6. Implement proper SEO practices
7. Use proper performance monitoring

## Security Considerations

1. Implement proper input validation
2. Use proper authentication
3. Implement proper authorization
4. Use proper encryption
5. Implement proper CORS policies
6. Use proper security headers
7. Implement proper rate limiting
8. Use proper error handling