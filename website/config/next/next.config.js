/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  swcMinify: true,
  pageExtensions: ['tsx', 'ts'],
  experimental: {
    appDir: true,
  },
  webpack: (config, { isServer }) => {
    // Add any custom webpack configurations here
    return config;
  },
  images: {
    domains: ['neo.org'],
  },
  // Redirect /api/* to our API routes
  async rewrites() {
    return [
      {
        source: '/api/:path*',
        destination: '/api/:path*',
      },
    ];
  },
  // Add headers for security
  async headers() {
    return [
      {
        source: '/:path*',
        headers: [
          {
            key: 'X-Frame-Options',
            value: 'DENY',
          },
          {
            key: 'X-Content-Type-Options',
            value: 'nosniff',
          },
          {
            key: 'X-XSS-Protection',
            value: '1; mode=block',
          },
        ],
      },
    ];
  },
  // Enable TypeScript path aliases
  webpack: (config) => {
    config.resolve.fallback = {
      ...config.resolve.fallback,
      fs: false,
      net: false,
      tls: false,
    };
    return config;
  },
  // Add environment variables
  env: {
    NEO_NETWORK: process.env.NEO_NETWORK || 'testnet',
    NEO_RPC_URL: process.env.NEO_RPC_URL,
  },
  // Add redirects
  async redirects() {
    return [
      {
        source: '/docs',
        destination: '/docs/introduction',
        permanent: true,
      },
    ];
  },
}

module.exports = nextConfig; 