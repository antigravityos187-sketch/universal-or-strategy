#!/bin/bash
# Quick status check for Wave 6 Phase 1

cd ~/universal-or-strategy

python3 << 'EOF'
import json
import glob

completed = 0
total = 78

for manifest_path in glob.glob('docs/brain/EPIC-CCN-*/manifest.json'):
    try:
        with open(manifest_path) as f:
            m = json.load(f)
            if m.get('phases', {}).get('1', {}).get('status') == 'completed':
                completed += 1
    except:
        pass

print(f"Phase 1 Status: {completed}/{total} complete")
if completed == total:
    print("✓ All epics completed!")
else:
    print(f"⏳ {total - completed} epics remaining")
EOF

# Made with Bob
