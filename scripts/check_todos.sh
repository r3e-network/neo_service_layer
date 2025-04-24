#!/bin/bash

# Script to check for TODOs and other placeholders
# Usage: ./scripts/check_todos.sh

# Navigate to project root
cd "$(dirname "$0")/.."
PROJECT_ROOT=$(pwd)

echo "Project root: $PROJECT_ROOT"

echo "Checking for TODOs..."
TODO_COUNT=$(grep -r "TODO" --include="*.cs" --include="*.md" . | wc -l)
echo "Found $TODO_COUNT TODOs"

echo "Checking for FIXMEs..."
FIXME_COUNT=$(grep -r "FIXME" --include="*.cs" --include="*.md" . | wc -l)
echo "Found $FIXME_COUNT FIXMEs"

echo "Checking for 'Not implemented'..."
NOT_IMPLEMENTED_COUNT=$(grep -r "Not implemented" --include="*.cs" . | wc -l)
echo "Found $NOT_IMPLEMENTED_COUNT 'Not implemented' instances"

echo "Checking for 'throw new NotImplementedException'..."
NOT_IMPLEMENTED_EXCEPTION_COUNT=$(grep -r "throw new NotImplementedException" --include="*.cs" . | wc -l)
echo "Found $NOT_IMPLEMENTED_EXCEPTION_COUNT 'throw new NotImplementedException' instances"

TOTAL_COUNT=$((TODO_COUNT + FIXME_COUNT + NOT_IMPLEMENTED_COUNT + NOT_IMPLEMENTED_EXCEPTION_COUNT))
echo "Total: $TOTAL_COUNT items to address"

if [ $TOTAL_COUNT -gt 0 ]; then
    echo "Consider addressing these items before release"
    
    # Show details if there are items to address
    echo "Details:"
    
    if [ $TODO_COUNT -gt 0 ]; then
        echo "TODOs:"
        grep -r "TODO" --include="*.cs" --include="*.md" . | sed 's/^/  /'
        echo ""
    fi
    
    if [ $FIXME_COUNT -gt 0 ]; then
        echo "FIXMEs:"
        grep -r "FIXME" --include="*.cs" --include="*.md" . | sed 's/^/  /'
        echo ""
    fi
    
    if [ $NOT_IMPLEMENTED_COUNT -gt 0 ]; then
        echo "'Not implemented' instances:"
        grep -r "Not implemented" --include="*.cs" . | sed 's/^/  /'
        echo ""
    fi
    
    if [ $NOT_IMPLEMENTED_EXCEPTION_COUNT -gt 0 ]; then
        echo "'throw new NotImplementedException' instances:"
        grep -r "throw new NotImplementedException" --include="*.cs" . | sed 's/^/  /'
        echo ""
    fi
fi
