# Contributing to Neo Service Layer

We love your input! We want to make contributing to Neo Service Layer as easy and transparent as possible, whether it's:

- Reporting a bug
- Discussing the current state of the code
- Submitting a fix
- Proposing new features
- Becoming a maintainer

## Development Process

We use GitHub to host code, to track issues and feature requests, as well as accept pull requests.

1. Fork the repo and create your branch from `main`
2. If you've added code that should be tested, add tests
3. If you've changed APIs, update the documentation
4. Ensure the test suite passes
5. Make sure your code lints
6. Issue that pull request!

## Code Quality Standards

### TypeScript Best Practices

```typescript
// Use explicit types
interface ConfigOptions {
  timeout: number;
  retries: number;
}

// Use type guards
function isValidResponse(response: unknown): response is ApiResponse {
  return typeof response === 'object' && response !== null;
}

// Use async/await with proper error handling
async function fetchData(): Promise<Data> {
  try {
    const response = await api.get('/endpoint');
    return response.data;
  } catch (error) {
    logger.error('Failed to fetch data', { error });
    throw new ServiceError('Data fetch failed');
  }
}
```

### Documentation Requirements

1. File Headers:
```typescript
/**
 * @file service-name.ts
 * @description Brief description of the service's purpose
 * @module ServiceName
 */
```

2. Interface/Type Documentation:
```typescript
/**
 * Configuration options for the service
 * @interface ServiceConfig
 */
interface ServiceConfig {
  /** Maximum retry attempts for failed operations */
  maxRetries: number;
  /** Timeout in milliseconds */
  timeout: number;
}
```

3. Method Documentation:
```typescript
/**
 * Processes incoming data with validation and transformation
 * @param data - Raw input data
 * @param options - Processing options
 * @returns Processed data object
 * @throws {ValidationError} If data is invalid
 */
function processData(data: RawData, options: ProcessOptions): ProcessedData
```

### Error Handling

1. Use custom error classes:
```typescript
export class ServiceError extends Error {
  constructor(
    message: string,
    public readonly code: string,
    public readonly details?: Record<string, unknown>
  ) {
    super(message);
    this.name = 'ServiceError';
  }
}
```

2. Implement proper error chains:
```typescript
try {
  await operation();
} catch (error) {
  throw new ServiceError(
    'Operation failed',
    'ERR_OPERATION',
    { cause: error }
  );
}
```

### Testing Standards

1. Unit Tests:
```typescript
describe('PriceFeedService', () => {
  describe('getAggregatedPrice', () => {
    it('should return weighted average price', async () => {
      // Arrange
      const service = new PriceFeedService(config);
      const mockData = generateMockPriceData();

      // Act
      const result = await service.getAggregatedPrice('NEO/USD');

      // Assert
      expect(result.price).toBeCloseTo(expectedPrice, 2);
      expect(result.confidence).toBeGreaterThan(0.8);
    });

    it('should handle invalid data gracefully', async () => {
      // Test error cases
    });
  });
});
```

2. Integration Tests:
```typescript
describe('Service Integration', () => {
  beforeAll(async () => {
    await setupTestEnvironment();
  });

  afterAll(async () => {
    await cleanupTestEnvironment();
  });

  it('should process end-to-end workflow', async () => {
    // Test complete workflow
  });
});
```

### Performance Considerations

1. Implement caching where appropriate:
```typescript
class CacheableService {
  private cache = new Map<string, CachedData>();

  async getData(key: string): Promise<Data> {
    const cached = this.cache.get(key);
    if (cached && !this.isExpired(cached)) {
      return cached.data;
    }
    
    const data = await this.fetchFreshData(key);
    this.cache.set(key, {
      data,
      timestamp: Date.now()
    });
    return data;
  }
}
```

2. Use batch operations:
```typescript
async function batchProcess(items: Item[]): Promise<Result[]> {
  const batchSize = 100;
  const results: Result[] = [];
  
  for (let i = 0; i < items.length; i += batchSize) {
    const batch = items.slice(i, i + batchSize);
    const batchResults = await Promise.all(
      batch.map(item => processItem(item))
    );
    results.push(...batchResults);
  }
  
  return results;
}
```

## Pull Request Process

1. Update the README.md with details of changes to the interface
2. Update the documentation with any new information
3. The PR must pass all tests and lint checks
4. The PR must be reviewed by at least one maintainer
5. Follow the PR template:

```markdown
## Description
Brief description of the changes

## Type of change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## How Has This Been Tested?
Describe the tests you ran

## Checklist:
- [ ] My code follows the style guidelines
- [ ] I have performed a self-review
- [ ] I have commented my code
- [ ] I have updated the documentation
- [ ] My changes generate no new warnings
- [ ] I have added tests
- [ ] All new and existing tests pass
```

## Community and Behavioral Expectations

1. Be welcoming and inclusive
2. Be respectful of differing viewpoints
3. Accept constructive criticism
4. Focus on what is best for the community
5. Show empathy towards other community members

## Issue Reporting Guidelines

1. Use the issue template
2. Include reproduction steps
3. Include relevant logs and screenshots
4. Describe expected vs actual behavior
5. List environment details

## License

By contributing, you agree that your contributions will be licensed under its MIT License.

## References

* [Neo Documentation](https://docs.neo.org/)
* [TypeScript Guidelines](https://www.typescriptlang.org/docs/handbook/declaration-files/do-and-dont.html)
* [Conventional Commits](https://www.conventionalcommits.org/) 