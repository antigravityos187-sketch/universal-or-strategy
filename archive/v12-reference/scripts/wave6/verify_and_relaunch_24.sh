#!/bin/bash
# Verify manifest migration and relaunch 24 blocked epics
# Wave 6 Phase 1 Recovery

cd ~/universal-or-strategy

echo "=== Verifying Manifest Migration ==="
python3 << 'PYEOF'
import json
with open('docs/brain/EPIC-CCN-004/manifest.json') as f:
    m = json.load(f)
    clock = m.get('lamport_clock', 0)
    events = len(m.get('lamport_events', []))
    print(f"lamport_clock: {clock}")
    print(f"lamport_events: {events} events")
    if events > 0:
        print("✓ Migration successful - Lamport events present")
    else:
        print("✗ Migration failed - No Lamport events")
        exit(1)
PYEOF

if [ $? -ne 0 ]; then
    echo "ERROR: Manifest migration verification failed"
    exit 1
fi

echo ""
echo "=== Relaunching 24 Blocked Epics ==="

# List of 24 epics that were blocked
BLOCKED_EPICS=(001 004 016 020 021 028 050 051 052 053 054 055 056 057 058 059 060 061 070 073 076 077 078 079)

for epic in "${BLOCKED_EPICS[@]}"; do
    script="scripts/wave6/_p1_epic_ccn_${epic}.sh"
    if [ -f "$script" ]; then
        echo "Launching EPIC-CCN-${epic}..."
        nohup bash "$script" > "logs/wave6/phase1/EPIC-CCN-${epic}-relaunch.log" 2>&1 &
        sleep 0.5
    else
        echo "WARNING: Script not found: $script"
    fi
done

echo ""
echo "=== Launch Complete ==="
echo "24 epics relaunched in background"
echo "Monitor with: tail -f logs/wave6/phase1/EPIC-CCN-*-relaunch.log"

# Made with Bob
