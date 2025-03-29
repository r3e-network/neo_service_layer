#!/bin/bash

# This script removes framer-motion imports and replaces motion components with standard HTML elements
# in all React components in the website directory

# Find all .tsx files in the src directory
FILES=$(find ./src -name "*.tsx")

for file in $FILES; do
  echo "Processing $file..."
  
  # Remove framer-motion import lines
  sed -i '' '/import.*framer-motion/d' "$file"
  
  # Replace motion.div with div
  sed -i '' 's/motion\.div/div/g' "$file"
  
  # Replace motion.section with section
  sed -i '' 's/motion\.section/section/g' "$file"
  
  # Replace motion.span with span
  sed -i '' 's/motion\.span/span/g' "$file"
  
  # Replace motion.p with p
  sed -i '' 's/motion\.p/p/g' "$file"
  
  # Replace motion.h1 with h1
  sed -i '' 's/motion\.h1/h1/g' "$file"
  
  # Replace motion.h2 with h2
  sed -i '' 's/motion\.h2/h2/g' "$file"
  
  # Replace motion.h3 with h3
  sed -i '' 's/motion\.h3/h3/g' "$file"
  
  # Replace motion.a with a
  sed -i '' 's/motion\.a/a/g' "$file"
  
  # Replace motion.button with button
  sed -i '' 's/motion\.button/button/g' "$file"
  
  # Replace motion.ul with ul
  sed -i '' 's/motion\.ul/ul/g' "$file"
  
  # Replace motion.li with li
  sed -i '' 's/motion\.li/li/g' "$file"
  
  # Replace motion.img with img
  sed -i '' 's/motion\.img/img/g' "$file"
  
  # Replace AnimatePresence component with div
  sed -i '' 's/<AnimatePresence>/<div>/g' "$file"
  sed -i '' 's/<\/AnimatePresence>/<\/div>/g' "$file"
  
  # Remove framer-motion animation props
  sed -i '' 's/initial={[^}]*}//g' "$file"
  sed -i '' 's/animate={[^}]*}//g' "$file"
  sed -i '' 's/exit={[^}]*}//g' "$file"
  sed -i '' 's/transition={[^}]*}//g' "$file"
  sed -i '' 's/variants={[^}]*}//g' "$file"
  sed -i '' 's/whileHover={[^}]*}//g' "$file"
  sed -i '' 's/whileTap={[^}]*}//g' "$file"
  
  echo "Finished processing $file"
done

echo "All files processed. Now updating package.json to remove framer-motion dependency..."

# Remove framer-motion from package.json
sed -i '' '/"framer-motion": /d' package.json

echo "Script completed successfully!"
