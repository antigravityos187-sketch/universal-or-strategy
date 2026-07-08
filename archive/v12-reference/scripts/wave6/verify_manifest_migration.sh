#!/bin/bash
# Verify manifest migration on VM
# Check that lamport_events exist in migrated manifests

cd ~/universal-or-strategy

echo "Checking EPIC-CCN-004 manifest..."
python3 << 'EOF'
import json
with open('docs/brain/EPIC-CCN-004/manifest.json') as f:
    m = json.load(f)
    print(f"lamport_clock: {m.get('lamport_clock')}")
    print(f"lamport_events: {len(m.get('lamport_events', []))} events")
    if m.get('lamport_events'):
        print("✓ Migration successful - Lamport events present")
    else:
        print("✗ Migration failed - No Lamport events")
EOF

# Made with Bob
