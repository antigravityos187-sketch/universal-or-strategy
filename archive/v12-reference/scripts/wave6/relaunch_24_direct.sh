#!/bin/bash
# Direct Launch 24 Epics (No Screen)
# Created: 2026-06-18T04:35:00Z
# Issue: Screen requires PTY, not available via SSH

set -e

EPICS=(
    "001" "004" "016" "020" "021" "028"
    "050" "051" "052" "053" "054" "055" "056" "057" "058" "059"
    "060" "061" "070" "073" "076" "077" "078" "079"
)

echo "=========================================="
echo "Direct Launch 24 Epics (No Screen)"
echo "Time: $(date -u +%Y-%m-%dT%H:%M:%SZ)"
echo "=========================================="
echo ""

LAUNCHED=0
FAILED=0

for EPIC_NUM in "${EPICS[@]}"; do
    EPIC_ID="EPIC-CCN-${EPIC_NUM}"
    SCRIPT="/home/malhitticrypto/universal-or-strategy/scripts/wave6/_p1_epic_ccn_${EPIC_NUM}.sh"
    LOG="/home/malhitticrypto/universal-or-strategy/logs/wave6_p1_epic_ccn_${EPIC_NUM}.log"
    
    echo "Launching $EPIC_ID..."
    
    # Check if script exists
    if [ ! -f "$SCRIPT" ]; then
        echo "  ❌ Script not found: $SCRIPT"
        ((FAILED++))
        continue
    fi
    
    # Launch directly in background with nohup
    cd /home/malhitticrypto/universal-or-strategy
    nohup bash "$SCRIPT" > "$LOG" 2>&1 &
    PID=$!
    
    # Verify process started
    sleep 0.5
    if ps -p $PID > /dev/null 2>&1; then
        echo "  ✅ Launched (PID: $PID)"
        ((LAUNCHED++))
    else
        echo "  ❌ Process died immediately"
        ((FAILED++))
    fi
done

echo ""
echo "=========================================="
echo "Launch Summary"
echo "=========================================="
echo "Launched: $LAUNCHED/24"
echo "Failed: $FAILED/24"
echo ""

if [ $LAUNCHED -eq 24 ]; then
    echo "✅ All 24 epics launched successfully"
    exit 0
else
    echo "⚠️  Some epics failed to launch"
    exit 1
fi

# Made with Bob
