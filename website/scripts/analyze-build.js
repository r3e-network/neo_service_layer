#!/usr/bin/env node

/**
 * Build Analysis Script
 * 
 * This script runs a build with the bundle analyzer and captures performance metrics
 * to help identify optimization opportunities in the Neo Service Layer website.
 */

const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');

// Create directory for reports if it doesn't exist
const reportsDir = path.join(__dirname, '../reports');
if (!fs.existsSync(reportsDir)) {
  fs.mkdirSync(reportsDir, { recursive: true });
}

// Timestamp for the report
const timestamp = new Date().toISOString().replace(/:/g, '-').replace(/\..+/, '');
const reportFile = path.join(reportsDir, `build-analysis-${timestamp}.json`);

console.log('🔍 Starting build analysis...');
console.log('📊 This will run a production build with bundle analyzer enabled');
console.log('⏱️  Measuring build performance metrics');

// Capture start time
const startTime = Date.now();

try {
  // Run the build with analyzer
  console.log('\n📦 Running production build with analyzer...');
  execSync('npm run build:analyze', { stdio: 'inherit' });
  
  // Capture end time and calculate duration
  const endTime = Date.now();
  const buildDuration = (endTime - startTime) / 1000; // in seconds
  
  // Get .next directory size
  const nextDirSize = getDirSizeInMB(path.join(__dirname, '../.next'));
  
  // Get bundle stats from Next.js build output
  const buildStats = getBuildStats();
  
  // Create report
  const report = {
    timestamp: new Date().toISOString(),
    buildDuration: `${buildDuration.toFixed(2)} seconds`,
    nextDirSize: `${nextDirSize.toFixed(2)} MB`,
    buildStats,
    recommendations: generateRecommendations(buildStats, buildDuration)
  };
  
  // Write report to file
  fs.writeFileSync(reportFile, JSON.stringify(report, null, 2));
  
  console.log(`\n✅ Build analysis complete!`);
  console.log(`📝 Report saved to: ${reportFile}`);
  console.log(`\n📊 Build Performance Summary:`);
  console.log(`⏱️  Build Duration: ${buildDuration.toFixed(2)} seconds`);
  console.log(`📦 .next Directory Size: ${nextDirSize.toFixed(2)} MB`);
  
  // Print top 5 largest chunks
  if (buildStats.chunks && buildStats.chunks.length > 0) {
    console.log('\n🔍 Top 5 Largest Chunks:');
    buildStats.chunks
      .sort((a, b) => b.size - a.size)
      .slice(0, 5)
      .forEach((chunk, i) => {
        console.log(`   ${i + 1}. ${chunk.name}: ${(chunk.size / 1024).toFixed(2)} KB`);
      });
  }
  
  // Print recommendations
  console.log('\n💡 Recommendations:');
  report.recommendations.forEach((rec, i) => {
    console.log(`   ${i + 1}. ${rec}`);
  });
  
} catch (error) {
  console.error('\n❌ Build analysis failed:', error);
  process.exit(1);
}

/**
 * Get directory size in MB
 */
function getDirSizeInMB(dirPath) {
  let size = 0;
  
  if (!fs.existsSync(dirPath)) {
    return 0;
  }
  
  const files = fs.readdirSync(dirPath);
  
  for (const file of files) {
    const filePath = path.join(dirPath, file);
    const stats = fs.statSync(filePath);
    
    if (stats.isDirectory()) {
      size += getDirSizeInMB(filePath) * 1024 * 1024; // Convert MB back to bytes for accumulation
    } else {
      size += stats.size;
    }
  }
  
  return size / (1024 * 1024); // Convert bytes to MB
}

/**
 * Get build stats from Next.js build output
 */
function getBuildStats() {
  try {
    // Try to read the build manifest
    const manifestPath = path.join(__dirname, '../.next/build-manifest.json');
    if (!fs.existsSync(manifestPath)) {
      return { error: 'Build manifest not found' };
    }
    
    const manifest = JSON.parse(fs.readFileSync(manifestPath, 'utf8'));
    
    // Get all JS files from the manifest
    const jsFiles = new Set();
    Object.values(manifest.pages).forEach(files => {
      files.forEach(file => {
        if (file.endsWith('.js')) {
          jsFiles.add(file);
        }
      });
    });
    
    // Get file sizes
    const chunks = Array.from(jsFiles).map(file => {
      const filePath = path.join(__dirname, '../.next', file);
      const size = fs.existsSync(filePath) ? fs.statSync(filePath).size : 0;
      return {
        name: file,
        size
      };
    });
    
    return {
      totalPages: Object.keys(manifest.pages).length,
      chunks
    };
  } catch (error) {
    return { error: error.message };
  }
}

/**
 * Generate optimization recommendations based on build stats
 */
function generateRecommendations(buildStats, buildDuration) {
  const recommendations = [];
  
  // Basic recommendations
  recommendations.push('Run builds with the new optimized configuration: npm run build:fast');
  recommendations.push('Consider implementing the dynamic import utilities for heavy components');
  
  // Build duration recommendations
  if (buildDuration > 60) {
    recommendations.push('Build time is excessive. Consider splitting the project into smaller packages or implementing incremental builds');
  }
  
  // Chunk size recommendations
  if (buildStats.chunks && buildStats.chunks.length > 0) {
    const largeChunks = buildStats.chunks.filter(chunk => chunk.size > 500 * 1024); // > 500KB
    
    if (largeChunks.length > 0) {
      recommendations.push(`Found ${largeChunks.length} large chunks (>500KB). Consider code splitting these components`);
      
      // Check for specific large dependencies
      const reactChunks = buildStats.chunks.filter(chunk => chunk.name.includes('react'));
      if (reactChunks.some(chunk => chunk.size > 200 * 1024)) {
        recommendations.push('React chunks are large. Consider using preact in production');
      }
      
      const neonJsChunks = buildStats.chunks.filter(chunk => chunk.name.includes('neon-js'));
      if (neonJsChunks.some(chunk => chunk.size > 300 * 1024)) {
        recommendations.push('neon-js chunks are large. Consider dynamic importing these only when needed');
      }
    }
  }
  
  return recommendations;
}
