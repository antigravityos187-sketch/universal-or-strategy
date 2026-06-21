#!/bin/bash
# Wave 6 Phase 1 - Launch Remaining 24 Epics
# Cost-Optimized: 4-minute polling intervals
# Generated: 2026-06-18

set -euo pipefail

echo "=========================================="
echo "Wave 6 Phase 1 - Remaining 24 Epics"
echo "Cost-Optimized Polling: 4 minutes"
echo "=========================================="

# Create logs directory
mkdir -p logs/wave6/phase1

# Missing epics (24 total)
EPICS=(001 004 016 020 021 028 050 051 052 053 054 055 056 057 058 059 060 061 070 073 076 077 078 079)

echo "Launching ${#EPICS[@]} epics in background..."

# Launch all epics in background
for EPIC in "${EPICS[@]}"; do
    SCRIPT="scripts/wave6/_p1_epic_ccn_${EPIC}.sh"
    if [ -f "$SCRIPT" ]; then
        echo "  Launching EPIC-CCN-${EPIC}..."
        bash "$SCRIPT" > "logs/wave6/phase1/epic_ccn_${EPIC}.log" 2>&1 &
    else
        echo "  [WARN] Script not found: $SCRIPT"
    fi
done

echo ""
echo "All epics launched. Monitoring progress..."
echo "Polling interval: 4 minutes (cost-optimized)"
echo ""

# Monitor progress with 4-minute intervals
COMPLETED=0
TOTAL=${#EPICS[@]}

while [ $COMPLETED -lt $TOTAL ]; do
    # Count completed epics
    COMPLETED=0
    for EPIC in "${EPICS[@]}"; do
        SCOPE_FILE="docs/brain/EPIC-CCN-${EPIC}/00-scope.md"
        if [ -f "$SCOPE_FILE" ]; then
            ((COMPLETED++))
        fi
    done
    
    PERCENT=$((COMPLETED * 100 / TOTAL))
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] Progress: $COMPLETED/$TOTAL ($PERCENT%)"
    
    if [ $COMPLETED -lt $TOTAL ]; then
        echo "  Waiting 4 minutes before next check..."
        sleep 240  # 4 minutes
    fi
done

echo ""
echo "=========================================="
echo "Wave 6 Phase 1 - Remaining 24 COMPLETE"
echo "Final: $COMPLETED/$TOTAL (100%)"
echo "=========================================="

# Made with Bob
