#!/bin/bash
set -e

echo "=== Starting EPIC-CCN-164 Test ==="
echo "Time: $(date)"
echo ""

# Change to repository directory
cd ~/universal-or-strategy

echo "Testing Bob Shell authentication..."
bob --accept-license --auth-method api-key -p "Test authentication" --max-coins 1

echo ""
echo "Running epic-intake for EPIC-CCN-164..."
bob --accept-license --auth-method api-key -p "Run epic-intake for EPIC-CCN-164" --max-coins 20

echo ""
echo "=== Test Complete ==="
echo "Time: $(date)"

# Made with Bob
