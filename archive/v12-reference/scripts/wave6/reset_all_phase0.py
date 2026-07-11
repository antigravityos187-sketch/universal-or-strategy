#!/usr/bin/env python3
"""Reset Phase 0 status to pending for all Wave 6 epics"""

import json
import os
import sys

# Load roadmap
with open('epic_roadmap_wave4_fresh.json', 'r', encoding='utf-8-sig') as f:
    roadmap = json.load(f)

epics = roadmap if isinstance(roadmap, list) else roadmap.get('epics', [])

reset_count = 0
skip_count = 0

for epic in epics:
    epic_num = epic['epic_number']
    # epic_number is already the full ID like "EPIC-CCN-001"
    epic_id = epic_num if isinstance(epic_num, str) and epic_num.startswith('EPIC-') else f"EPIC-CCN-{epic_num:03d}"
    
    manifest_path = f"docs/brain/{epic_id}/manifest.json"
    
    if not os.path.exists(manifest_path):
        print(f"⚠️  {epic_id}: No manifest found")
        skip_count += 1
        continue
    
    # Load manifest
    with open(manifest_path, 'r', encoding='utf-8') as f:
        manifest = json.load(f)
    
    # Check Phase 0 status
    if '0' not in manifest['phases']:
        print(f"⚠️  {epic_id}: No Phase 0 in manifest")
        skip_count += 1
        continue
    
    old_status = manifest['phases']['0']['status']
    
    # Reset Phase 0 to pending
    manifest['phases']['0']['status'] = 'pending'
    manifest['phases']['0']['started_at'] = None
    manifest['phases']['0']['completed_at'] = None
    
    # Save manifest
    with open(manifest_path, 'w', encoding='utf-8') as f:
        json.dump(manifest, f, indent=2)
    
    print(f"✅ {epic_id}: {old_status} → pending")
    reset_count += 1

print(f"\n=== Summary ===")
print(f"Reset: {reset_count}")
print(f"Skipped: {skip_count}")
print(f"Total: {reset_count + skip_count}")

exit(0)

# Made with Bob
