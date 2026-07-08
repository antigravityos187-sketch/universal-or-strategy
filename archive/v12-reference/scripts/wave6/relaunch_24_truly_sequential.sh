#!/bin/bash
# Relaunch 24 blocked epics ONE AT A TIME (wait for completion)
# Wave 6 Phase 1 Recovery - True Sequential Execution

cd ~/universal-or-strategy

echo "=== Relaunching 24 Blocked Epics (True Sequential) ==="
echo "Each epic completes before next one starts"
echo ""

# List of 24 epics that were blocked
BLOCKED_EPICS=(001 004 016 020 021 028 050 051 052 053 054 055 056 057 058 059 060 061 070 073 076 077 078 079)

count=1
total=${#BLOCKED_EPICS[@]}

for epic in "${BLOCKED_EPICS[@]}"; do
    script="scripts/wave6/_p1_epic_ccn_${epic}.sh"
    if [ -f "$script" ]; then
        echo "[$count/$total] Executing EPIC-CCN-${epic}..."
        echo "----------------------------------------"
        
        # Run in FOREGROUND (wait for completion)
        bash "$script"
        exit_code=$?
        
        if [ $exit_code -eq 0 ]; then
            echo "✓ EPIC-CCN-${epic} completed successfully"
        else
            echo "✗ EPIC-CCN-${epic} failed with exit code $exit_code"
        fi
        
        echo "----------------------------------------"
        echo ""
        
        count=$((count + 1))
    else
        echo "[$count/$total] WARNING: Script not found: $script"
        count=$((count + 1))
    fi
done

echo ""
echo "=== All 24 Epics Processed ==="
echo "Check final status:"
echo "  bash /tmp/check_phase1_status.sh"

# Made with Bob
