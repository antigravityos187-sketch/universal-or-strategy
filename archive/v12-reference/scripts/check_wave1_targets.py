#!/usr/bin/env python3
"""Check Wave 1 epic targets and recommendations."""

import json
import os

epics = [
    'EPIC-CCN-21', 'EPIC-CCN-22', 'EPIC-CCN-23', 'EPIC-CCN-24', 'EPIC-CCN-25',
    'EPIC-CCN-26', 'EPIC-CCN-27', 'EPIC-CCN-28', 'EPIC-CCN-29', 'EPIC-CCN-30'
]

print("Wave 1 Epic Status Check:")
print("=" * 80)

for epic_id in epics:
    manifest_path = f"docs/brain/{epic_id}/manifest.json"
    if os.path.exists(manifest_path):
        with open(manifest_path) as f:
            data = json.load(f)
        
        current_cyc = data.get('current_cyc', '?')
        target_cyc = data.get('target_cyc', '?')
        recommendation = data.get('recommendation', '?')
        status = data.get('status', '?')
        
        needs_update = target_cyc != 8 and current_cyc > 8
        marker = "[NEEDS UPDATE]" if needs_update else "[OK]"
        
        print(f"{epic_id}: CYC={current_cyc}, Target={target_cyc}, Rec={recommendation}, Status={status} {marker}")
    else:
        print(f"{epic_id}: No manifest found")

print("=" * 80)

# Made with Bob
