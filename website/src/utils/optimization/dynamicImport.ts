/**
 * Utility functions for dynamic imports and code splitting
 * 
 * This module provides helper functions for optimizing the loading of heavy components
 * through dynamic imports, which improves initial page load performance by reducing
 * the main bundle size.
 */

import React from 'react';
import dynamic from 'next/dynamic';
import type { DynamicOptionsLoadingProps } from 'next/dynamic';

/**
 * Options for dynamic import
 */
interface DynamicImportOptions {
  /**
   * Custom loading component to show while the main component is loading
   */
  LoadingComponent?: React.ComponentType;
  
  /**
   * Fallback content to show if the component fails to load
   */
  fallback?: React.ReactNode;
  
  /**
   * Whether to prefetch the component
   */
  prefetch?: boolean;
}

/**
 * Default loading component
 */
const DefaultLoading = (): JSX.Element => {
  return React.createElement('div', { 
    className: "animate-pulse p-4 bg-gray-100 dark:bg-gray-800 rounded-md" 
  }, "Loading...");
};

/**
 * Creates a dynamically imported component with optimized loading
 * 
 * @param importFunc - Function that imports the component
 * @param options - Options for dynamic import
 */
export function createDynamicComponent<T extends React.ComponentType<unknown>>(
  importFunc: () => Promise<{ default: T }>,
  options: DynamicImportOptions = {}
) {
  const {
    LoadingComponent = DefaultLoading,
    fallback,
    prefetch = true
  } = options;

  return dynamic(importFunc, {
    loading: LoadingComponent,
    ssr: true,
    suspense: true
  });
}

/**
 * Creates a lazy-loaded component that only loads when it becomes visible in the viewport
 * 
 * @param importFunc - Function that imports the component
 * @param options - Options for lazy loading
 */
export function createLazyComponent<T extends React.ComponentType<unknown>>(
  importFunc: () => Promise<{ default: T }>,
  options: Omit<DynamicImportOptions, 'prefetch'> = {}
) {
  return createDynamicComponent(importFunc, {
    ...options,
    prefetch: false
  });
}

/**
 * Example usage:
 * 
 * ```tsx
 * // Import a heavy chart component dynamically
 * const DynamicChart = createDynamicComponent(() => import('../components/Chart'), {
 *   prefetch: false, // Don't prefetch if it's not needed immediately
 * });
 * 
 * // Import a component that should only load when visible
 * const LazyImage = createLazyComponent(() => import('../components/HeavyImage'));
 * ```
 */
