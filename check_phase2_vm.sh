#!/bin/bash
cd ~/universal-or-strategy
echo "=== Wave 2 Phase 2 Status ==="
for epic in 107 108 109 110 111 112 113 114 115; do
  echo "EPIC-CCN-$epic:"
  if [ -f "docs/brain/EPIC-CCN-$epic/manifest.json" ]; then
    grep '"status":' "docs/brain/EPIC-CCN-$epic/manifest.json" | head -1
    grep -A 3 '"2":' "docs/brain/EPIC-CCN-$epic/manifest.json" | grep '"status"'
  else
    echo "  No manifest"
  fi
  echo ""
done

# Made with Bob
