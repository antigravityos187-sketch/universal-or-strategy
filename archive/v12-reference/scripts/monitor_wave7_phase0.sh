#!/bin/bash

# Wave 7 Phase 0 Progress Monitor
# Checks completion status every 4 minutes (cost-optimized polling)

TOTAL_EPICS=161
LOG_DIR="logs/phase0"
BRAIN_DIR="docs/brain"

echo "================================================================================"
echo "WAVE 7 PHASE 0 - PROGRESS MONITOR"
echo "================================================================================"
echo ""
echo "Target: $TOTAL_EPICS epics"
echo "Polling interval: 4 minutes (cost-optimized)"
echo "Press Ctrl+C to stop monitoring"
echo ""

while true; do
    # Count completed epics
    COMPLETE=$(/usr/bin/python3 -c "import os; print(len([f for f in os.listdir('$BRAIN_DIR') if f.startswith('EPIC-W7-') and os.path.exists(f'$BRAIN_DIR/{f}/00-hotspots.md')]))")
    
    # Calculate percentage
    PERCENT=$(/usr/bin/python3 -c "print(f'{($COMPLETE/$TOTAL_EPICS)*100:.1f}')")
    
    # Get timestamp
    TIMESTAMP=$(date '+%Y-%m-%d %H:%M:%S')
    
    # Display status
    echo "[$TIMESTAMP] Progress: $COMPLETE/$TOTAL_EPICS ($PERCENT%)"
    
    # Check if complete
    if [ "$COMPLETE" -eq "$TOTAL_EPICS" ]; then
        echo ""
        echo "================================================================================"
        echo "✅ WAVE 7 PHASE 0 COMPLETE!"
        echo "================================================================================"
        echo ""
        echo "All $TOTAL_EPICS epics completed successfully."
        echo "Ready for Phase 1 (Scope Definition)."
        echo ""
        break
    fi
    
    # Check for errors in recent logs
    ERRORS=$(grep -l "ERROR\|FAILED\|Exception" $LOG_DIR/*.log 2>/dev/null | wc -l)
    if [ "$ERRORS" -gt 0 ]; then
        echo "   ⚠️  Warning: $ERRORS log files contain errors"
    fi
    
    # Wait 4 minutes before next check
    sleep 240
done

# Made with Bob
