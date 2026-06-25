#!/bin/bash
# Launch remaining Phase 2 epics (skip completed ones)
# Building-Blocks Method: Same pattern as master launch

set -e

echo "=== Wave 7 Phase 2 Recovery Launch ==="
echo "Started: $(date)"
echo ""

# Create logs directory
mkdir -p logs/wave7/phase2

# Count completed
COMPLETED=$(find docs/brain/EPIC-W7-*/02-architecture-plan.md 2>/dev/null | wc -l)
echo "Already complete: $COMPLETED/161"
echo "Remaining to launch: $((161 - COMPLETED))"
echo "Delay between launches: 12 seconds"
echo ""

LAUNCHED=0

# Launch all epics, skip completed ones
for i in {001..161}; do
    EPIC_ID="EPIC-W7-${i}"
    SCRIPT="_p2_${i}.sh"
    LOG="logs/wave7/phase2/${EPIC_ID}.log"
    OUTPUT="docs/brain/${EPIC_ID}/02-architecture-plan.md"
    
    # Skip if already complete
    if [ -f "$OUTPUT" ]; then
        continue
    fi
    
    # Check if script exists
    if [ ! -f "$SCRIPT" ]; then
        echo "ERROR: Script not found: $SCRIPT"
        continue
    fi
    
    # Launch epic in background with nohup
    echo "[$((LAUNCHED + 1))] Launching $EPIC_ID..."
    nohup ./"$SCRIPT" > "$LOG" 2>&1 &
    PID=$!
    echo "  PID: $PID"
    
    LAUNCHED=$((LAUNCHED + 1))
    
    # Progress checkpoint every 10 epics
    if [ $((LAUNCHED % 10)) -eq 0 ]; then
        CURRENT_COMPLETE=$(find docs/brain/EPIC-W7-*/02-architecture-plan.md 2>/dev/null | wc -l)
        echo ""
        echo "=== Checkpoint: $LAUNCHED launched, $CURRENT_COMPLETE/161 complete ==="
        echo ""
    fi
    
    # Delay before next launch
    sleep 12
done

echo ""
echo "=== Recovery Launch Complete ==="
echo "Finished: $(date)"
echo "Launched: $LAUNCHED epics"
echo ""
echo "Monitor progress with:"
echo "  watch -n 240 'find docs/brain/EPIC-W7-*/02-architecture-plan.md 2>/dev/null | wc -l'"

# Made with Bob
