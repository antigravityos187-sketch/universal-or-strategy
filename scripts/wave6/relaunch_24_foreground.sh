#!/bin/bash
# Relaunch 24 blocked epics in FOREGROUND using screen sessions
# Wave 6 Phase 1 Recovery - Foreground Execution

cd ~/universal-or-strategy

echo "=== Relaunching 24 Blocked Epics in Foreground ==="
echo "Using screen sessions for visibility"
echo ""

# List of 24 epics that were blocked
BLOCKED_EPICS=(001 004 016 020 021 028 050 051 052 053 054 055 056 057 058 059 060 061 070 073 076 077 078 079)

# Create a master screen session
screen -dmS wave6_phase1_master

# Launch each epic in its own screen window
for epic in "${BLOCKED_EPICS[@]}"; do
    script="scripts/wave6/_p1_epic_ccn_${epic}.sh"
    if [ -f "$script" ]; then
        echo "Launching EPIC-CCN-${epic} in screen window..."
        screen -S wave6_phase1_master -X screen -t "epic_${epic}" bash "$script"
        sleep 0.5
    else
        echo "WARNING: Script not found: $script"
    fi
done

echo ""
echo "=== Launch Complete ==="
echo "24 epics running in screen session 'wave6_phase1_master'"
echo ""
echo "To view:"
echo "  screen -r wave6_phase1_master"
echo ""
echo "To switch windows:"
echo "  Ctrl+A then N (next) or P (previous)"
echo ""
echo "To detach:"
echo "  Ctrl+A then D"
echo ""
echo "To list windows:"
echo "  screen -S wave6_phase1_master -X windows"

# Made with Bob
