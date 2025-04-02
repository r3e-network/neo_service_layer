'use client';

import React from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
// @ts-ignore - Using this approach to avoid TypeScript errors with next-themes
import { ThemeProvider } from 'next-themes';
import { ThemeProvider as MuiThemeProvider } from '@mui/material/styles';
import { lightTheme } from '../theme/theme';

// Create a client for React Query
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
    },
  },
});

// Update next-themes wrapper to force light theme
const NextThemesWrapper = ({ children }: { children: React.ReactNode }) => (
  // @ts-ignore - Ignoring type issues with next-themes
  <ThemeProvider
    attribute="class"
    // Force light theme and disable system preference
    forcedTheme="light"
    defaultTheme="light"
    enableSystem={false} 
    enableColorScheme={false}
  >
    {children}
  </ThemeProvider>
);

export function Providers({ children }: { children: React.ReactNode }) {
  return (
    <NextThemesWrapper>
      <QueryClientProvider client={queryClient}>
        {/* Use MuiThemeProvider directly with lightTheme */}
        <MuiThemeProvider theme={lightTheme}>
          {children}
        </MuiThemeProvider>
      </QueryClientProvider>
    </NextThemesWrapper>
  );
}