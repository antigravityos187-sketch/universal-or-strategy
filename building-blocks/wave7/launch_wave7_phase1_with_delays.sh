#!/bin/bash

# Wave 7 Phase 1 Sequential Launch with Resource Management
# Prevents VM exhaustion by launching epics with 12-second delays

set -e

echo "=== Wave 7 Phase 1 Sequential Launch ==="
echo "Started: $(date)"
echo ""

# Create logs directory
mkdir -p logs/wave7/phase1

# Track progress
TOTAL_EPICS=161
COMPLETED_EPICS=3  # Pilots already done: 003, 051, 101
REMAINING_EPICS=$((TOTAL_EPICS - COMPLETED_EPICS))
LAUNCHED=0

echo "Status:"
echo "  - Total epics: $TOTAL_EPICS"
echo "  - Already complete: $COMPLETED_EPICS (pilots: 003, 051, 101)"
echo "  - Remaining to launch: $REMAINING_EPICS"
echo "  - Delay between launches: 12 seconds"
echo ""

# Calculate estimated time
TOTAL_SECONDS=$((REMAINING_EPICS * 12))
HOURS=$((TOTAL_SECONDS / 3600))
MINUTES=$(((TOTAL_SECONDS % 3600) / 60))
echo "Estimated launch time: ${HOURS}h ${MINUTES}m"
echo ""

# Launch all epics sequentially with delays
for i in {001..161}; do
    EPIC_ID="EPIC-W7-${i}"
    SCRIPT="_p1_${i}.sh"
    LOG="logs/wave7/phase1/${EPIC_ID}.log"
    
    # Skip pilot epics (already complete)
    if [ "$i" = "003" ] || [ "$i" = "051" ] || [ "$i" = "101" ]; then
        echo "[$((LAUNCHED + COMPLETED_EPICS))/$TOTAL_EPICS] Skipping $EPIC_ID (pilot - already complete)"
        continue
    fi
    
    # Check if script exists
    if [ ! -f "$SCRIPT" ]; then
        echo "[$((LAUNCHED + COMPLETED_EPICS))/$TOTAL_EPICS] ERROR: Script not found: $SCRIPT"
        continue
    fi
    
    # Launch epic in background with nohup
    echo "[$((LAUNCHED + COMPLETED_EPICS + 1))/$TOTAL_EPICS] Launching $EPIC_ID..."
    nohup ./"$SCRIPT" > "$LOG" 2>&1 &
    PID=$!
    echo "  PID: $PID, Log: $LOG"
    
    LAUNCHED=$((LAUNCHED + 1))
    
    # Progress checkpoint every 10 epics
    if [ $((LAUNCHED % 10)) -eq 0 ]; then
        CURRENT_COMPLETE=$(find docs/brain/EPIC-W7-*/00-scope.md 2>/dev/null | wc -l)
        echo ""
        echo "=== Checkpoint: $LAUNCHED/$REMAINING_EPICS launched, $CURRENT_COMPLETE/$TOTAL_EPICS complete ==="
        echo ""
    fi
    
    # Delay before next launch (skip on last epic)
    if [ $LAUNCHED -lt $REMAINING_EPICS ]; then
        sleep 12
    fi
done

echo ""
echo "=== Launch Complete ==="
echo "Finished: $(date)"
echo "Launched: $LAUNCHED epics"
echo ""
echo "Monitor progress with:"
echo "  watch -n 60 'find docs/brain/EPIC-W7-*/00-scope.md 2>/dev/null | wc -l'"
echo ""
echo "Check logs:"
echo "  tail -f logs/wave7/phase1/EPIC-W7-*.log"
