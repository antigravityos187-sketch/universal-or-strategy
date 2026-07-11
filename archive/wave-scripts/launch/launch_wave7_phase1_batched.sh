#!/bin/bash
# Wave 7 Phase 1 Batched Launch Script
# Launches Phase 1 epics in small batches to prevent VM overload
# Batch size: 10 concurrent epics max
# Wait for batch completion before starting next batch

set -e

BATCH_SIZE=10
EXCLUDE_EPICS="100 024 017"

echo "=== Wave 7 Phase 1 Batched Launch ==="
echo "Batch size: $BATCH_SIZE concurrent epics"
echo "Start time: $(date -u +%Y-%m-%dT%H:%M:%SZ)"
echo ""

# Collect all eligible scripts
SCRIPTS=()
for script in _p1_[0-9][0-9][0-9].sh _p1_[0-9][0-9].sh _p1_[0-9].sh; do
    [ -f "$script" ] || continue
    
    EPIC_NUM=$(echo "$script" | sed 's/_p1_\([0-9]*\)\.sh/\1/' | sed 's/^0*//')
    
    # Skip pilots
    SKIP=0
    for EXCLUDE in $EXCLUDE_EPICS; do
        [ "$EPIC_NUM" = "$EXCLUDE" ] && SKIP=1 && break
    done
    [ $SKIP -eq 1 ] && continue
    
    # Check Phase 0 complete
    EPIC_DIR="docs/brain/EPIC-W7-$(printf '%03d' $EPIC_NUM)"
    [ ! -f "$EPIC_DIR/00-hotspots.md" ] && continue
    
    # Check Phase 1 not already complete
    [ -f "$EPIC_DIR/00-scope.md" ] && continue
    
    SCRIPTS+=("$script")
done

TOTAL=${#SCRIPTS[@]}
echo "Found $TOTAL epics to process"
echo ""

# Process in batches
BATCH_NUM=1
PROCESSED=0

while [ $PROCESSED -lt $TOTAL ]; do
    echo "=== Batch $BATCH_NUM (epics $((PROCESSED+1))-$((PROCESSED+BATCH_SIZE))) ==="
    
    BATCH_PIDS=()
    BATCH_COUNT=0
    
    # Launch batch
    for ((i=PROCESSED; i<TOTAL && BATCH_COUNT<BATCH_SIZE; i++)); do
        script="${SCRIPTS[$i]}"
        EPIC_NUM=$(echo "$script" | sed 's/_p1_\([0-9]*\)\.sh/\1/' | sed 's/^0*//')
        
        echo "🚀 Launching $script (EPIC-W7-$EPIC_NUM)"
        bash "$script" > "logs/phase1_epic_${EPIC_NUM}.log" 2>&1 &
        BATCH_PIDS+=($!)
        
        BATCH_COUNT=$((BATCH_COUNT+1))
        sleep 12  # 12-second delay between launches within batch
    done
    
    PROCESSED=$((PROCESSED+BATCH_COUNT))
    
    # Wait for batch to complete
    echo "⏳ Waiting for batch $BATCH_NUM to complete ($BATCH_COUNT epics)..."
    for pid in "${BATCH_PIDS[@]}"; do
        wait $pid 2>/dev/null || true
    done
    
    echo "✅ Batch $BATCH_NUM complete"
    echo ""
    
    BATCH_NUM=$((BATCH_NUM+1))
    
    # Brief pause between batches
    [ $PROCESSED -lt $TOTAL ] && sleep 30
done

echo "=== Launch Complete ==="
echo "Total processed: $PROCESSED epics"
echo "End time: $(date -u +%Y-%m-%dT%H:%M:%SZ)"
echo ""
echo "Check completion:"
echo "  find docs/brain/EPIC-W7-* -name '00-scope.md' | wc -l"

# Made with Bob
