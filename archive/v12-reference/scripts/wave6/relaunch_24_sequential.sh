#!/bin/bash
# Relaunch 24 blocked epics SEQUENTIALLY with 9-second delays
# Wave 6 Phase 1 Recovery - Sequential Foreground Execution

cd ~/universal-or-strategy

echo "=== Relaunching 24 Blocked Epics Sequentially ==="
echo "9-second delay between each epic"
echo ""

# List of 24 epics that were blocked
BLOCKED_EPICS=(001 004 016 020 021 028 050 051 052 053 054 055 056 057 058 059 060 061 070 073 076 077 078 079)

count=1
total=${#BLOCKED_EPICS[@]}

for epic in "${BLOCKED_EPICS[@]}"; do
    script="scripts/wave6/_p1_epic_ccn_${epic}.sh"
    if [ -f "$script" ]; then
        echo "[$count/$total] Launching EPIC-CCN-${epic}..."
        bash "$script" &
        
        # Wait 9 seconds before next launch (except for last one)
        if [ $count -lt $total ]; then
            echo "  Waiting 9 seconds..."
            sleep 9
        fi
        
        count=$((count + 1))
    else
        echo "[$count/$total] WARNING: Script not found: $script"
        count=$((count + 1))
    fi
done

echo ""
echo "=== Launch Complete ==="
echo "All 24 epics launched in background"
echo ""
echo "Monitor progress:"
echo "  bash /tmp/check_phase1_status.sh"

# Made with Bob
