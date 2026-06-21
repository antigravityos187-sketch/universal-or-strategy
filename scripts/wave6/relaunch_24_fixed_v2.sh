#!/bin/bash
# Relaunch 24 Fixed Epics - V2 (Actually Launch Them)
# Created: 2026-06-18T04:32:00Z
# Issue: V1 reported success but epics never launched

set -e

EPICS=(
    "001" "004" "016" "020" "021" "028"
    "050" "051" "052" "053" "054" "055" "056" "057" "058" "059"
    "060" "061" "070" "073" "076" "077" "078" "079"
)

echo "=========================================="
echo "Relaunching 24 Fixed Epics - V2"
echo "Time: $(date -u +%Y-%m-%dT%H:%M:%SZ)"
echo "=========================================="
echo ""

LAUNCHED=0
FAILED=0

for EPIC_NUM in "${EPICS[@]}"; do
    EPIC_ID="EPIC-CCN-${EPIC_NUM}"
    SCRIPT="/home/malhitticrypto/universal-or-strategy/scripts/wave6/_p1_epic_ccn_${EPIC_NUM}.sh"
    
    echo "Launching $EPIC_ID..."
    
    # Check if script exists
    if [ ! -f "$SCRIPT" ]; then
        echo "  ❌ Script not found: $SCRIPT"
        ((FAILED++))
        continue
    fi
    
    # Launch in screen session
    screen -dmS "epic_ccn_${EPIC_NUM}" bash -c "cd /home/malhitticrypto/universal-or-strategy && bash $SCRIPT"
    
    # Verify screen session created
    sleep 1
    if screen -ls | grep -q "epic_ccn_${EPIC_NUM}"; then
        echo "  ✅ Launched in screen session: epic_ccn_${EPIC_NUM}"
        ((LAUNCHED++))
    else
        echo "  ❌ Failed to create screen session"
        ((FAILED++))
    fi
done

echo ""
echo "=========================================="
echo "Relaunch Summary"
echo "=========================================="
echo "Launched: $LAUNCHED/24"
echo "Failed: $FAILED/24"
echo ""

if [ $LAUNCHED -eq 24 ]; then
    echo "✅ All 24 epics relaunched successfully"
    exit 0
else
    echo "⚠️  Some epics failed to launch"
    exit 1
fi

# Made with Bob
