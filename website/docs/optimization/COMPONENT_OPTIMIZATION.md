# Neo Service Layer Component Optimization Guide

This document provides guidelines for optimizing React components in the Neo Service Layer website to improve build and runtime performance.

## Dynamic Imports and Code Splitting

### When to Use Dynamic Imports

Use dynamic imports for:

1. **Heavy Components**: Components with large dependencies (charts, editors, etc.)
2. **Infrequently Used Components**: Components that aren't needed on initial page load
3. **Route-Specific Components**: Components only used on certain pages
4. **Components with Browser-Only APIs**: Components that use browser-specific features

### Implementation Guide

We've created utility functions in `src/utils/optimization/dynamicImport.ts` to simplify dynamic imports:

```tsx
import { createDynamicComponent, createLazyComponent } from '@/utils/optimization/dynamicImport';

// Basic dynamic import
const DynamicChart = createDynamicComponent(() => import('@/components/charts/Chart'), {
  ssr: false, // Don't render on server if it uses browser APIs
});

// Lazy-loaded component (loads when visible in viewport)
const LazyImage = createLazyComponent(() => import('@/components/media/HeavyImage'));

// Usage in your component
function MyPage() {
  return (
    <div>
      <h1>My Page</h1>
      <DynamicChart data={chartData} />
      <LazyImage src="/large-image.jpg" alt="Large image" />
    </div>
  );
}
```

### Best Practices

1. **Chunk Naming**: Name your chunks for better debugging:

```tsx
const DynamicChart = createDynamicComponent(
  () => import(/* webpackChunkName: "chart" */ '@/components/charts/Chart')
);
```

2. **Preloading**: Preload important components that will be needed soon:

```tsx
// In your component
useEffect(() => {
  // Preload the component when the user hovers over a button
  const preloadChart = () => {
    import('@/components/charts/Chart');
  };
  
  button.addEventListener('mouseover', preloadChart);
  return () => button.removeEventListener('mouseover', preloadChart);
}, []);
```

3. **Route-Based Splitting**: Split components based on routes:

```tsx
// In your app/page.tsx
import dynamic from 'next/dynamic';

const DynamicHomePage = dynamic(() => import('@/components/pages/HomePage'));

export default function Home() {
  return <DynamicHomePage />;
}
```

## Image Optimization

Use Next.js Image component for all images to enable automatic optimization:

```tsx
import Image from 'next/image';

function OptimizedImage() {
  return (
    <Image
      src="/profile.jpg"
      alt="Profile"
      width={500}
      height={300}
      placeholder="blur"
      blurDataURL="data:image/jpeg;base64,..."
      priority={false}
    />
  );
}
```

### Image Best Practices

1. **Use `priority` for LCP images**: Mark above-the-fold images as priority
2. **Provide `width` and `height`**: Prevents layout shifts
3. **Use WebP/AVIF formats**: Next.js automatically serves optimized formats
4. **Use responsive sizes**: Adjust image size based on viewport

```tsx
<Image
  src="/hero.jpg"
  alt="Hero"
  sizes="(max-width: 768px) 100vw, (max-width: 1200px) 50vw, 33vw"
  fill
  className="object-cover"
/>
```

## React Component Optimization

### Memoization

Use memoization for expensive components:

```tsx
import { memo, useMemo, useCallback } from 'react';

// Memoize a component
const MemoizedComponent = memo(function ExpensiveComponent({ data }) {
  // Component logic
});

// In your component
function ParentComponent() {
  // Memoize expensive calculations
  const processedData = useMemo(() => {
    return expensiveCalculation(data);
  }, [data]);
  
  // Memoize callbacks
  const handleClick = useCallback(() => {
    // Handle click
  }, []);
  
  return <MemoizedComponent data={processedData} onClick={handleClick} />;
}
```

### Virtual Lists

For long lists, use virtualization:

```tsx
import { useVirtualizer } from '@tanstack/react-virtual';

function VirtualList({ items }) {
  const parentRef = useRef(null);
  
  const virtualizer = useVirtualizer({
    count: items.length,
    getScrollElement: () => parentRef.current,
    estimateSize: () => 50,
  });
  
  return (
    <div ref={parentRef} style={{ height: '500px', overflow: 'auto' }}>
      <div
        style={{
          height: `${virtualizer.getTotalSize()}px`,
          width: '100%',
          position: 'relative',
        }}
      >
        {virtualizer.getVirtualItems().map((virtualItem) => (
          <div
            key={virtualItem.key}
            style={{
              position: 'absolute',
              top: 0,
              left: 0,
              width: '100%',
              height: `${virtualItem.size}px`,
              transform: `translateY(${virtualItem.start}px)`,
            }}
          >
            {items[virtualItem.index]}
          </div>
        ))}
      </div>
    </div>
  );
}
```

## CSS Optimization

### Tailwind Optimization

1. **Purge Unused Styles**: Ensure the Tailwind config properly purges unused styles
2. **Use JIT Mode**: Just-in-time compilation for faster builds
3. **Group Related Styles**: Use Tailwind's `@apply` directive for repeated patterns

```css
/* In your CSS */
.neo-button {
  @apply px-4 py-2 bg-neo-green text-white rounded-md hover:bg-neo-green/90 transition-colors;
}
```

### CSS-in-JS Optimization

If using CSS-in-JS libraries:

1. **Use Static Extraction**: Extract CSS at build time when possible
2. **Avoid Runtime Styles**: Minimize dynamic styles that can't be extracted
3. **Use Theme Constants**: Define colors, spacing, etc. as constants

## Conclusion

By following these optimization techniques, we can significantly improve both build time and runtime performance of the Neo Service Layer website. Always measure the impact of optimizations using tools like Lighthouse and the Next.js build analyzer to ensure they're providing the expected benefits.
