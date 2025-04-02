#!/usr/bin/env node

/**
 * Build optimization script for Neo Service Layer website
 * 
 * This script helps identify and fix build performance issues
 */

const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');

// Configuration
const TEMP_CACHE_DIR = path.join(__dirname, '..', '.next', 'cache');

function executeCommand(command) {
  console.log(`\n🚀 Executing: ${command}\n`);
  try {
    execSync(command, { stdio: 'inherit' });
    return true;
  } catch (error) {
    console.error(`❌ Command failed: ${command}`);
    return false;
  }
}

function clearCache() {
  console.log('\n🧹 Clearing Next.js cache...');
  try {
    if (fs.existsSync(TEMP_CACHE_DIR)) {
      executeCommand(`rimraf ${TEMP_CACHE_DIR}`);
    }
    return true;
  } catch (error) {
    console.error('❌ Failed to clear cache:', error);
    return false;
  }
}

async function main() {
  console.log('🔍 Optimizing Neo Service Layer website build...');

  // Clear cache to start fresh
  clearCache();

  // Run fast build with minimum checks
  console.log('\n⚡ Running optimized fast build...');
  const buildSuccess = executeCommand('NEXT_TELEMETRY_DISABLED=1 next build --no-lint');

  if (!buildSuccess) {
    console.error('\n❌ Build failed. Trying to diagnose the issue...');
    
    // Run with more verbose output for debugging
    console.log('\n🔎 Running diagnostic build...');
    executeCommand('NEXT_TELEMETRY_DISABLED=1 NODE_OPTIONS="--max-old-space-size=4096" next build --debug');
  } else {
    console.log('\n✅ Build completed successfully!');
  }
}

main().catch(console.error);