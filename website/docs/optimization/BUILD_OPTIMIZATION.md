# Neo Service Layer Website Build Optimization

This document outlines the strategy for optimizing the build performance of the Neo Service Layer website. It identifies the current issues causing slow build times and provides solutions to address them.

## Current Issues

1. **Mixing of Routing Systems**
   - The project uses both Pages Router (`/pages`) and App Router (`/src/app`)
   - This duplication forces Next.js to process both routing systems

2. **Heavy Dependencies**
   - Multiple UI frameworks (Tailwind CSS alongside other libraries)
   - Large blockchain-related libraries (`@cityofzion/neon-js`)
   - Charting libraries (`recharts`)
   - Multiple testing frameworks

3. **Suboptimal Next.js Configuration**
   - Experimental features causing additional overhead
   - Preact replacement in production builds requiring additional processing
   - Deprecated experimental flags

4. **Multiple Documentation Systems**
   - Both Storybook and VitePress for documentation
   - Separate build processes required

5. **Inefficient Asset Handling**
   - Unoptimized images and static assets
   - Potential duplicate assets

## Optimization Strategy

### 1. Consolidate Routing System

```
- Move all pages from /pages to /src/app
- Update routing patterns to match App Router conventions
- Remove legacy Pages Router code
```

### 2. Optimize Dependencies

```json
// Remove unused dependencies
// Consolidate UI frameworks to Tailwind CSS only
// Use dynamic imports for heavy components
```

### 3. Update Next.js Configuration

```javascript
// Remove experimental features
// Optimize chunks and module splitting
// Enable proper caching mechanisms
```

### 4. Implement Build Caching

```bash
# Use persistent build cache
NEXT_TELEMETRY_DISABLED=1 npx next build --no-lint
```

### 5. Separate Documentation Builds

```json
// Split documentation builds from main website
"scripts": {
  "build": "next build",
  "build:docs": "vitepress build docs",
  "build:all": "npm run build && npm run build:docs"
}
```

### 6. Implement Code Splitting

```javascript
// Use dynamic imports for heavy components
import dynamic from 'next/dynamic';

const HeavyComponent = dynamic(() => import('../components/HeavyComponent'), {
  loading: () => <p>Loading...</p>,
  ssr: false // Disable server-side rendering for heavy components
});
```

## Implementation Plan

### Phase 1: Immediate Optimizations

1. **Analyze Build Performance**
   - Run build analyzer to identify largest dependencies
   - Profile build process to identify bottlenecks

2. **Update Next.js Configuration**
   - Remove experimental features
   - Optimize webpack configuration
   - Enable proper caching

3. **Remove Unused Dependencies**
   - Audit and remove unused packages
   - Consolidate UI frameworks to Tailwind CSS only

### Phase 2: Structural Improvements

1. **Complete Migration to App Router**
   - Move all pages to App Router format
   - Update routing patterns
   - Remove Pages Router code

2. **Implement Code Splitting**
   - Add dynamic imports for heavy components
   - Optimize page loading

3. **Optimize Asset Handling**
   - Implement Next.js Image component for all images
   - Optimize static assets

### Phase 3: Long-term Strategy

1. **Consider Turbopack Migration**
   - Evaluate benefits of Turbopack for build performance
   - Plan migration if beneficial

2. **Implement Incremental Static Regeneration**
   - Use ISR for documentation pages
   - Reduce build times for content-heavy pages

3. **Set Up Performance Monitoring**
   - Track build times over time
   - Identify ongoing optimization opportunities

## Success Metrics

- **Build Time**: Reduce build time by at least 50%
- **Bundle Size**: Reduce total bundle size by at least 30%
- **Page Load Performance**: Improve Lighthouse performance scores to 90+
- **Developer Experience**: Reduce time from code change to preview

## Conclusion

By implementing these optimizations, we expect to significantly improve the build performance of the Neo Service Layer website, resulting in faster deployments, better developer experience, and improved user experience through faster page loads.
