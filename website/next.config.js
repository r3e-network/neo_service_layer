/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  transpilePackages: ['@ant-design/icons'],
  images: {
    domains: ['localhost'],
  },
  // Simplified configuration to avoid conflicts
  swcMinify: true,
  // Ignore ESLint errors during build
  eslint: {
    // Warning instead of error during build
    ignoreDuringBuilds: true,
  },
};

module.exports = nextConfig;
