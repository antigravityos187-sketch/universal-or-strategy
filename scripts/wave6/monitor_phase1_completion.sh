#!/bin/bash
# Monitor Wave 6 Phase 1 completion status
# Checks every 4 minutes (cost-optimized polling)

cd ~/universal-or-strategy

echo "=== Wave 6 Phase 1 Completion Monitor ==="
echo "Started: $(date)"
echo ""

while true; do
    # Count completed Phase 1 epics
    completed=$(python3 << 'PYEOF'
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

print(f"{completed}/{total}")
PYEOF
)
    
    echo "[$(date +%H:%M:%S)] Phase 1 Status: $completed"
    
    # Check if all 78 complete
    if [[ "$completed" == "78/78" ]]; then
        echo ""
        echo "=== SUCCESS: All 78 epics completed Phase 1 ==="
        echo "Completed at: $(date)"
        exit 0
    fi
    
    # Wait 4 minutes (cost-optimized polling per V12.32)
    sleep 240
done

# Made with Bob
