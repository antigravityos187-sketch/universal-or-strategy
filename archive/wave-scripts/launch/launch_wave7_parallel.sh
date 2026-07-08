#!/bin/bash
# Wave 7 Phase 0 - Parallel Launcher with 12-second stagger
# Protocol: Launch epics every 12 seconds in background
set -e

echo "================================================================================"
echo "WAVE 7 PHASE 0 - PARALLEL LAUNCHER (12-SECOND STAGGER)"
echo "================================================================================"
echo ""

# Read remaining epics
if [ ! -f "wave7_remaining_epics.txt" ]; then
    echo "❌ Error: wave7_remaining_epics.txt not found"
    echo "Run cleanup_and_relaunch_wave7.py first"
    exit 1
fi

mapfile -t INCOMPLETE < wave7_remaining_epics.txt
TOTAL=${#INCOMPLETE[@]}

echo "Found $TOTAL incomplete epics"
echo "Launch pattern: 12-second stagger, parallel execution"
echo ""

# Create logs directory
mkdir -p logs/phase0

# Launch counter
LAUNCHED=0

for epic_id in "${INCOMPLETE[@]}"; do
    # Extract epic number from EPIC-W7-XXX format
    EPIC_NUM=$(echo "$epic_id" | sed 's/EPIC-W7-//')
    SCRIPT="_p0_${EPIC_NUM}.sh"
    
    if [ ! -f "$SCRIPT" ]; then
        echo "⚠️  Script $SCRIPT not found - skipping"
        continue
    fi
    
    LAUNCHED=$((LAUNCHED + 1))
    echo "[$LAUNCHED/$TOTAL] Launching $epic_id (background)"
    
    # Launch in background with log redirection
    /usr/bin/bash "$SCRIPT" > "logs/phase0/${epic_id}.log" 2>&1 &
    
    # Store PID for monitoring
    echo $! >> logs/phase0/pids.txt
    
    # 12-second stagger (protocol requirement)
    if [ $LAUNCHED -lt $TOTAL ]; then
        echo "   Waiting 12 seconds before next launch..."
        sleep 12
    fi
done

echo ""
echo "================================================================================"
echo "✅ LAUNCHED $LAUNCHED EPICS IN PARALLEL"
echo "================================================================================"
echo ""
echo "Monitor progress:"
echo "  - Check logs: tail -f logs/phase0/EPIC-W7-*.log"
echo "  - Count complete: ls -1d docs/brain/EPIC-W7-*/00-hotspots.md | wc -l"
echo "  - Check PIDs: cat logs/phase0/pids.txt"
echo ""
echo "Expected completion: ~15 hours (151 epics × 6 min each, parallel)"
echo ""

# Made with Bob