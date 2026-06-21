#!/bin/bash
# Relaunch 24 epics that failed due to ImportError
# Now that imports are fixed, these should work

set -euo pipefail

echo "=========================================="
echo "Relaunching 24 Fixed Epics"
echo "Started: $(date)"
echo "=========================================="

# The 24 epics that were stuck at 0% (from launch_phase1_remaining_24.sh)
EPICS=(001 004 016 020 021 028 050 051 052 053 054 055 056 057 058 059 060 061 070 073 076 077 078 079)

echo "Launching ${#EPICS[@]} epics in background..."

# Launch all epics
for EPIC in "${EPICS[@]}"; do
    SCRIPT="scripts/wave6/_p1_epic_ccn_${EPIC}.sh"
    LOG="logs/wave6/phase1/epic_ccn_${EPIC}.log"
    
    if [ -f "$SCRIPT" ]; then
        echo "  Launching EPIC-CCN-${EPIC}..."
        nohup bash "$SCRIPT" > "$LOG" 2>&1 &
    else
        echo "  [WARN] Script not found: $SCRIPT"
    fi
done

echo ""
echo "=========================================="
echo "All 24 epics launched"
echo "Monitor progress: tail -f logs/wave6_phase1_monitor.log"
echo "=========================================="

# Made with Bob
