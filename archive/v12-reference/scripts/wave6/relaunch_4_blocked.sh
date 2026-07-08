#!/bin/bash
# Relaunch 4 blocked epics after Lamport clock fix

cd /home/malhitticrypto/universal-or-strategy

echo "=== Relaunching 4 Blocked Epics ==="
echo ""

for epic in 001 004 016 028; do
    echo "Launching EPIC-CCN-${epic}..."
    bash scripts/wave6/_p1_epic_ccn_${epic}.sh &
    sleep 2
done

echo ""
echo "✅ All 4 epics launched!"
echo ""
echo "Monitor progress with:"
echo "  python3 scripts/check_wave6_status.py"

# Made with Bob
