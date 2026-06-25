#!/bin/bash

# Final recovery for 17 remaining Wave 7 Phase 0 epics
# These either stalled again or never launched properly

FINAL_EPICS=(
    "005"
    "022"
    "039"
    "055"
    "073"
    "088"
    "089"
    "105"
    "106"
    "123"
    "124"
    "139"
    "140"
    "152"
    "156"
    "157"
    "159"
)

echo "================================================================================"
echo "WAVE 7 PHASE 0 - FINAL 17 EPIC RECOVERY"
echo "================================================================================"
echo ""
echo "Relaunching ${#FINAL_EPICS[@]} remaining epics"
echo "Pattern: 12-second stagger, parallel execution"
echo ""

# Create final recovery log directory
mkdir -p logs/phase0_final

# Launch each epic
for epic_num in "${FINAL_EPICS[@]}"; do
    EPIC_ID="EPIC-W7-${epic_num}"
    SCRIPT="_p0_${epic_num}.sh"
    
    if [ -f "$SCRIPT" ]; then
        echo "[Final Recovery] Launching $EPIC_ID (background)"
        /usr/bin/bash "$SCRIPT" > "logs/phase0_final/${EPIC_ID}.log" 2>&1 &
        PID=$!
        echo "$PID" >> logs/phase0_final/pids.txt
        echo "   PID: $PID"
        sleep 12
    else
        echo "[ERROR] Script not found: $SCRIPT"
    fi
done

echo ""
echo "================================================================================"
echo "✅ LAUNCHED ${#FINAL_EPICS[@]} FINAL EPICS"
echo "================================================================================"
echo ""
echo "Monitor progress:"
echo "  - tail -f logs/phase0_final/EPIC-W7-*.log"
echo "  - Check completion: python3 -c \"import os; print(len([f for f in os.listdir('docs/brain') if f.startswith('EPIC-W7-') and os.path.exists(f'docs/brain/{f}/00-hotspots.md')]))\""
echo ""

# Made with Bob
