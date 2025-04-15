#!/bin/bash

# Exit immediately if a command exits with a non-zero status.
set -e

# --- Configuration ---
# Assumes the script is run from the root of the project.
MODULE_PATH="github.com/r3e-network/neo_service_layer"
SOURCE_SUBDIR="services"
TARGET_BASE="internal"
SOURCE_DIR="$TARGET_BASE/$SOURCE_SUBDIR"

# --- Safety Check ---
if [ ! -d "$SOURCE_DIR" ]; then
  echo "Error: Source directory '$SOURCE_DIR' not found."
  echo "Please run this script from the root of your project: $MODULE_PATH"
  exit 1
fi

echo "Starting restructure process..."
echo "Module Path: $MODULE_PATH"
echo "Source Directory: $SOURCE_DIR"
echo "Target Base Directory: $TARGET_BASE"
echo "------------------------------------"

# --- Main Logic ---
# Find all directories directly under internal/services
find "$SOURCE_DIR" -mindepth 1 -maxdepth 1 -type d | while read -r service_path; do
  service_name=$(basename "$service_path")

  # Skip potentially problematic names or files that might appear as dirs
  if [[ -z "$service_name" || "$service_name" == ".DS_Store" ]]; then
      echo "Skipping invalid entry: $service_path"
      continue
  fi

  # Construct new name and path
  new_service_name="${service_name}service"
  new_service_path="$TARGET_BASE/$new_service_name"

  # Construct import paths
  old_import_path="$MODULE_PATH/$SOURCE_DIR/$service_name"
  new_import_path="$MODULE_PATH/$TARGET_BASE/$new_service_name" # The new path under internal

  echo ""
  echo "Processing '$service_name':"
  echo "  Old Path: $service_path"
  echo "  New Path: $new_service_path"
  echo "  Old Import: $old_import_path"
  echo "  New Import: $new_import_path"

  # 1. Move the directory
  echo "  Moving directory..."
  mv "$service_path" "$new_service_path"
  if [ $? -ne 0 ]; then
      echo "  Error moving directory $service_path. Aborting."
      exit 1
  fi
  echo "  Moved '$service_path' to '$new_service_path'."

  # 2. Update import references across the entire project
  echo "  Updating import paths in .go files..."
  # Use grep -rl --include='*.go' to find Go files containing the old path
  # Use xargs and sed to replace the path in those files
  # Using "|" as sed delimiter because paths contain "/"
  # The '-i ''' is for macOS sed compatibility (no backup file). Linux might just need '-i'.
  grep -rl --include='*.go' "$old_import_path" . | xargs sed -i '' "s|$old_import_path|$new_import_path|g"
  echo "  Finished updating imports for '$service_name'."

done

# --- Cleanup ---
echo ""
echo "Removing the now potentially empty source directory '$SOURCE_DIR'..."
# Use rmdir for safety - it only removes empty directories
rmdir "$SOURCE_DIR" || echo "Warning: '$SOURCE_DIR' was not empty or could not be removed."

echo "------------------------------------"
echo "Restructuring process completed!"
echo "IMPORTANT: Please review the changes carefully using 'git status' and 'git diff'."
echo "Build and test your project thoroughly before committing."

exit 0
