#!/bin/bash

# Update imports in src/components
find ./src/components -type f -name "*.tsx" -exec sed -i '' \
  -e 's|from "../../hooks/|from "../app/hooks/|g' \
  -e "s|from '../../hooks/|from '../app/hooks/|g" {} +

# Update imports in src/app/services
find ./src/app/services -type f -name "*.ts" -name "*.tsx" -exec sed -i '' \
  -e 's|from "@/hooks/|from "../../hooks/|g' \
  -e "s|from '@/hooks/|from '../../hooks/|g" {} +

# Update imports in docs
find ./docs -type f -name "*.md" -exec sed -i '' \
  -e 's|from "../hooks/|from "../src/app/hooks/|g' \
  -e "s|from '../hooks/|from '../src/app/hooks/|g" {} +

# Update imports in tests
find ./__tests__ -type f -name "*.tsx" -name "*.ts" -exec sed -i '' \
  -e 's|from "../../hooks/|from "../src/app/hooks/|g' \
  -e "s|from '../../hooks/|from '../src/app/hooks/|g" {} +