#!/bin/bash
# Relaunch 18 pending epics - Wave 6 Phase 1
# Building blocks method: Copied from relaunch_24_sequential.sh (working pattern)
# Generated: 2026-06-18

cd ~/universal-or-strategy

echo "=== Relaunching 18 Pending Epics ==="
echo "2-second delay between each epic"
echo ""

# 18 pending epics (excluding the 6 already completed: 070, 073, 076, 077, 078, 079)
PENDING_EPICS=(001 004 016 020 021 028 050 051 052 053 054 055 056 057 058 059 060 061)

count=1
total=${#PENDING_EPICS[@]}

for epic in "${PENDING_EPICS[@]}"; do
    script="scripts/wave6/_p1_epic_ccn_${epic}.sh"
    if [ -f "$script" ]; then
        echo "[$count/$total] Launching EPIC-CCN-${epic}..."
        bash "$script" &
        
        # Wait 2 seconds before next launch (except for last one)
        if [ $count -lt $total ]; then
            sleep 2
        fi
        
        count=$((count + 1))
    else
        echo "  ⚠️  Script not found: $script"
    fi
done

echo ""
echo "=== All 18 epics launched ==="
echo "Monitor with: bash /tmp/monitor_24_completion.sh"

# Made with Bob
