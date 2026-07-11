#!/usr/bin/env python3
"""Find first truly pending epic for pilot test"""

import json
import os

# Load roadmap (handle UTF-8 BOM)
with open('epic_roadmap_wave4_fresh.json', 'r', encoding='utf-8-sig') as f:
    roadmap = json.load(f)

# Find first pending epic
# Roadmap is a list of epics, not a dict with 'epics' key
epics = roadmap if isinstance(roadmap, list) else roadmap.get('epics', [])

for epic in epics:
    epic_num = epic['epic_number']
    # epic_number might be string or int
    if isinstance(epic_num, str):
        epic_id = f"EPIC-CCN-{epic_num}"
    else:
        epic_id = f"EPIC-CCN-{epic_num:03d}"
    manifest_path = f"docs/brain/{epic_id}/manifest.json"
    
    # Check if manifest exists and Phase 0 is pending
    if os.path.exists(manifest_path):
        with open(manifest_path, 'r') as f:
            manifest = json.load(f)
        
        phase0_status = manifest.get('phases', {}).get('0', {}).get('status', 'unknown')
        
        if phase0_status == 'pending':
            print(f"Found pending epic: {epic_id}")
            print(f"Method: {epic['method']}")
            print(f"File: {epic['file']}")
            print(f"CYC: {epic['cyclomatic']}")
            exit(0)

print("No pending epics found")
exit(1)

# Made with Bob
