#!/bin/bash

# Wave 7 Phase 2 Pilot Launch (3 epics)
# Building-Blocks Method: Test before full wave
# Test epics: 001 (low), 050 (medium), 100 (high complexity)

set -e

echo "=== Wave 7 Phase 2 Pilot Launch ==="
echo "Started: $(date)"
echo ""

# Create logs directory
mkdir -p logs/wave7/phase2

# Pilot epics
PILOT_EPICS=("001" "050" "100")
TOTAL_PILOTS=${#PILOT_EPICS[@]}
LAUNCHED=0

echo "Pilot Configuration:"
echo "  - Epic 001: Low complexity"
echo "  - Epic 050: Medium complexity"
echo "  - Epic 100: High complexity"
echo "  - Delay between launches: 12 seconds"
echo ""

# Launch pilot epics
for i in "${PILOT_EPICS[@]}"; do
    EPIC_ID="EPIC-W7-${i}"
    SCRIPT="_p2_${i}.sh"
    LOG="logs/wave7/phase2/${EPIC_ID}.log"
    
    # Check if script exists
    if [ ! -f "$SCRIPT" ]; then
        echo "[$((LAUNCHED + 1))/$TOTAL_PILOTS] ERROR: Script not found: $SCRIPT"
        continue
    fi
    
    # Launch epic in background with nohup
    echo "[$((LAUNCHED + 1))/$TOTAL_PILOTS] Launching $EPIC_ID..."
    nohup ./"$SCRIPT" > "$LOG" 2>&1 &
    PID=$!
    echo "  PID: $PID, Log: $LOG"
    
    LAUNCHED=$((LAUNCHED + 1))
    
    # Delay before next launch (skip on last epic)
    if [ $LAUNCHED -lt $TOTAL_PILOTS ]; then
        sleep 12
    fi
done

echo ""
echo "=== Pilot Launch Complete ==="
echo "Finished: $(date)"
echo "Launched: $LAUNCHED pilot epics"
echo ""
echo "Monitor progress with:"
echo "  watch -n 60 'find docs/brain/EPIC-W7-{001,050,100}/02-architecture-plan.md 2>/dev/null | wc -l'"
echo ""
echo "Check logs:"
echo "  tail -f logs/wave7/phase2/EPIC-W7-{001,050,100}.log"
echo ""
echo "When all 3 pilots complete successfully, run:"
echo "  ./launch_wave7_phase2_master.sh"

# Made with Bob
