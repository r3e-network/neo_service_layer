#!/bin/bash

# Build Optimization Script for Neo Service Layer Website
# This script applies all optimizations to improve build performance

# Set error handling
set -e

# Print banner
echo "🚀 Neo Service Layer Website Build Optimizer 🚀"
echo "==============================================="
echo "This script will optimize the build process for the Neo Service Layer website."
echo ""

# Start timer
START_TIME=$(date +%s)

# Step 1: Clean previous builds
echo "🧹 Step 1: Cleaning previous builds..."
npm run clean
echo "✅ Clean complete!"
echo ""

# Step 2: Clear node_modules cache if requested
if [ "$1" == "--clean-deps" ]; then
  echo "🧹 Step 2: Cleaning node_modules cache..."
  rm -rf node_modules/.cache
  echo "✅ Dependencies cache cleaned!"
  echo ""
else
  echo "🔄 Step 2: Skipping node_modules cache clean (use --clean-deps to clean)"
  echo ""
fi

# Step 3: Optimize TypeScript compilation
echo "⚙️ Step 3: Optimizing TypeScript compilation..."
# Temporarily disable type checking during build for faster builds
export DISABLE_TYPE_CHECK=true
echo "✅ TypeScript optimization configured!"
echo ""

# Step 4: Run optimized build
echo "🏗️ Step 4: Running optimized build..."
echo "🔧 Using Next.js production optimizations"
echo "🔧 Disabling telemetry"
echo "🔧 Skipping linting during build"
echo "🔧 Enabling SWC minification"

# Run the optimized build
NEXT_TELEMETRY_DISABLED=1 npm run build:fast

# End timer and calculate duration
END_TIME=$(date +%s)
DURATION=$((END_TIME - START_TIME))
MINUTES=$((DURATION / 60))
SECONDS=$((DURATION % 60))

echo ""
echo "✨ Build optimization complete! ✨"
echo "==============================================="
echo "⏱️ Total build time: ${MINUTES}m ${SECONDS}s"
echo ""

# Get build size information
NEXT_SIZE=$(du -sh .next | cut -f1)
echo "📊 Build Statistics:"
echo "   - .next directory size: ${NEXT_SIZE}"
echo ""

echo "🔍 Next Steps:"
echo "   1. Run 'npm run build:analyze' to identify large dependencies"
echo "   2. Apply code splitting for heavy components using the dynamic import utilities"
echo "   3. Consider implementing the optimization recommendations in the documentation"
echo ""

echo "🚀 To start the optimized build, run: npm run start"
