/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  // swcMinify: true, // Removed unrecognized option
  poweredByHeader: false,
  images: {
    domains: ['assets.example.com'],
  },
  // Optimize build performance
  transpilePackages: ['@ant-design/icons'], // Restored package for transpilation
  // Only use source maps in development
  productionBrowserSourceMaps: false,
  // Experimental features for faster builds
  experimental: {
    // outputFileTracingRoot: process.env.NODE_ENV === 'production' ? undefined : process.cwd(), // Removed unrecognized option
    // Turbopack features
    turbo: {
      resolveAlias: {
        // Add any alias mappings here
      },
    },
  },
  // Disable type checking during build for speed
  typescript: {
    // Type checking happens in a separate process (CI)
    ignoreBuildErrors: process.env.CI ? false : true,
  },
  // Disable ESLint during build for speed
  eslint: {
    ignoreDuringBuilds: true,
  },
};

module.exports = nextConfig;
