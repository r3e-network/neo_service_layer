#!/bin/bash

# Script to create a new release
# Usage: ./scripts/create_release.sh <version>
# Example: ./scripts/create_release.sh 1.0.0

# Check if version is provided
if [ -z "$1" ]; then
    echo "Error: Version number is required"
    echo "Usage: ./scripts/create_release.sh <version>"
    echo "Example: ./scripts/create_release.sh 1.0.0"
    exit 1
fi

VERSION=$1
RELEASE_DATE=$(date +%Y-%m-%d)

# Navigate to project root
cd "$(dirname "$0")/.."
PROJECT_ROOT=$(pwd)

echo "Project root: $PROJECT_ROOT"

# Update CHANGELOG.md
echo "Updating CHANGELOG.md..."
sed -i.bak "s/## \[Unreleased\]/## [Unreleased]\n\n### Added\n\n### Changed\n\n### Fixed\n\n## [$VERSION] - $RELEASE_DATE/" CHANGELOG.md
rm CHANGELOG.md.bak

# Create a git tag
echo "Creating git tag v$VERSION..."
git add CHANGELOG.md
git commit -m "Release v$VERSION"
git tag -a "v$VERSION" -m "Release v$VERSION"

echo "Release v$VERSION created!"
echo "Don't forget to push the changes and the tag:"
echo "git push origin main"
echo "git push origin v$VERSION"
