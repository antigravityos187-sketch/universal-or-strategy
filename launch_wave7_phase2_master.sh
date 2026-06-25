#!/bin/bash

# Wave 7 Phase 2 Sequential Launch with Resource Management
# Building-Blocks Method: Copied from launch_wave7_phase1_with_delays.sh
# Changes: phase1 -> phase2, 00-scope.md -> 02-architecture-plan.md

set -e

echo "=== Wave 7 Phase 2 Sequential Launch ==="
echo "Started: $(date)"
echo ""

# Create logs directory
mkdir -p logs/wave7/phase2

# Track progress
TOTAL_EPICS=161
COMPLETED_EPICS=0  # No pilots yet for Phase 2
REMAINING_EPICS=$((TOTAL_EPICS - COMPLETED_EPICS))
LAUNCHED=0

echo "Status:"
echo "  - Total epics: $TOTAL_EPICS"
echo "  - Already complete: $COMPLETED_EPICS"
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
    SCRIPT="_p2_${i}.sh"
    LOG="logs/wave7/phase2/${EPIC_ID}.log"
    
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
        CURRENT_COMPLETE=$(find docs/brain/EPIC-W7-*/02-architecture-plan.md 2>/dev/null | wc -l)
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
echo "  watch -n 240 'find docs/brain/EPIC-W7-*/02-architecture-plan.md 2>/dev/null | wc -l'"
echo ""
echo "Check logs:"
echo "  tail -f logs/wave7/phase2/EPIC-W7-*.log"

# Made with Bob
