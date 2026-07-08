#!/bin/bash
# Check Phase 2 status for all Wave 2 epics

cd ~/universal-or-strategy

echo "=== Wave 2 Phase 2 Status Check ==="
echo ""

for epic in 107 108 109 110 111 112 113 114 115; do
    echo "EPIC-CCN-$epic:"
    if [ -f "docs/brain/EPIC-CCN-$epic/manifest.json" ]; then
        status=$(grep -o '"status": "[^"]*"' "docs/brain/EPIC-CCN-$epic/manifest.json" | head -1 | cut -d'"' -f4)
        phase2=$(grep -A 3 '"2":' "docs/brain/EPIC-CCN-$epic/manifest.json" | grep '"status"' | cut -d'"' -f4)
        echo "  Overall: $status"
        echo "  Phase 2: ${phase2:-not started}"
    else
        echo "  Manifest not found"
    fi
    echo ""
done

echo "=== Summary ==="
completed=$(find docs/brain/EPIC-CCN-*/manifest.json -exec grep -l '"2":.*"completed"' {} \; 2>/dev/null | wc -l)
echo "Phase 2 completed: $completed/9 epics"

# Made with Bob
