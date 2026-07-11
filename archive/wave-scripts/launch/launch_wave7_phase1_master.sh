#!/bin/bash
# Wave 7 Phase 1 Master Launch Script
# Launches all remaining Phase 1 epics (excluding 3 completed pilots)
# Uses 12-second delays between launches for parallel execution

set -e

echo "=== Wave 7 Phase 1 Master Launch ==="
echo "Start time: $(date -u +%Y-%m-%dT%H:%M:%SZ)"
echo ""

# Exclude completed pilots
EXCLUDE_EPICS="100 024 017"

# Counter
LAUNCHED=0
SKIPPED=0

# Launch only standard Phase 1 scripts (not Phase 1.5, not _corrected variants)
for script in _p1_[0-9][0-9][0-9].sh _p1_[0-9][0-9].sh _p1_[0-9].sh; do
    # Skip if glob didn't match
    [ -f "$script" ] || continue
    
    # Extract epic number from script name
    EPIC_NUM=$(echo "$script" | sed 's/_p1_\([0-9]*\)\.sh/\1/' | sed 's/^0*//')
    
    # Check if this epic should be skipped (pilots)
    SKIP=0
    for EXCLUDE in $EXCLUDE_EPICS; do
        if [ "$EPIC_NUM" = "$EXCLUDE" ]; then
            SKIP=1
            break
        fi
    done
    
    if [ $SKIP -eq 1 ]; then
        echo "⏭️  Skipping $script (pilot already complete)"
        SKIPPED=$((SKIPPED + 1))
        continue
    fi
    
    # Check if epic has Phase 0 complete
    EPIC_DIR="docs/brain/EPIC-W7-$(printf '%03d' $EPIC_NUM)"
    if [ ! -f "$EPIC_DIR/00-hotspots.md" ]; then
        echo "⚠️  Skipping $script (no Phase 0 hotspots)"
        SKIPPED=$((SKIPPED + 1))
        continue
    fi
    
    # Check if Phase 1 already complete
    if [ -f "$EPIC_DIR/00-scope.md" ]; then
        echo "✅ Skipping $script (Phase 1 already complete)"
        SKIPPED=$((SKIPPED + 1))
        continue
    fi
    
    # Launch epic in background
    echo "🚀 Launching $script (EPIC-W7-$EPIC_NUM)"
    bash "$script" > "logs/phase1_epic_${EPIC_NUM}.log" 2>&1 &
    LAUNCHED=$((LAUNCHED + 1))
    
    # 12-second delay between launches
    sleep 12
done

echo ""
echo "=== Launch Summary ==="
echo "Launched: $LAUNCHED epics"
echo "Skipped: $SKIPPED epics"
echo "End time: $(date -u +%Y-%m-%dT%H:%M:%SZ)"
echo ""
echo "Monitor progress with:"
echo "  watch -n 60 'find docs/brain/EPIC-W7-* -name \"00-scope.md\" | wc -l'"

# Made with Bob
