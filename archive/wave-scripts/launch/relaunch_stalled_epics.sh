#!/bin/bash

# Relaunch stalled Wave 7 Phase 0 epics
# These epics have logs >10 minutes old without completion

STALLED_EPICS=(
    "005"
    "012"
    "022"
    "029"
    "039"
    "048"
    "055"
    "073"
    "078"
    "088"
    "089"
    "105"
    "106"
    "119"
    "123"
    "124"
    "127"
    "139"
    "140"
)

echo "================================================================================"
echo "WAVE 7 PHASE 0 - STALLED EPIC RECOVERY"
echo "================================================================================"
echo ""
echo "Relaunching ${#STALLED_EPICS[@]} stalled epics"
echo "Pattern: 12-second stagger, parallel execution"
echo ""

# Create recovery log directory
mkdir -p logs/phase0_recovery

# Launch each stalled epic
for epic_num in "${STALLED_EPICS[@]}"; do
    EPIC_ID="EPIC-W7-${epic_num}"
    SCRIPT="_p0_${epic_num}.sh"
    
    if [ -f "$SCRIPT" ]; then
        echo "[Recovery] Launching $EPIC_ID (background)"
        /usr/bin/bash "$SCRIPT" > "logs/phase0_recovery/${EPIC_ID}.log" 2>&1 &
        PID=$!
        echo "$PID" >> logs/phase0_recovery/pids.txt
        echo "   PID: $PID"
        sleep 12
    else
        echo "[ERROR] Script not found: $SCRIPT"
    fi
done

echo ""
echo "================================================================================"
echo "✅ LAUNCHED ${#STALLED_EPICS[@]} STALLED EPICS"
echo "================================================================================"
echo ""
echo "Monitor progress:"
echo "  - Check logs: tail -f logs/phase0_recovery/EPIC-W7-*.log"
echo "  - Count complete: ls -1d docs/brain/EPIC-W7-*/00-hotspots.md | wc -l"
echo ""

# Made with Bob
