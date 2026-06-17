#!/usr/bin/env python3
"""Fix manifest phase numbering inconsistency (1.0 → 1)"""

import json
import os
import sys

# Load roadmap
with open('epic_roadmap_wave4_fresh.json', 'r', encoding='utf-8-sig') as f:
    roadmap = json.load(f)

epics = roadmap if isinstance(roadmap, list) else roadmap.get('epics', [])

fixed_count = 0
skip_count = 0

for epic in epics:
    epic_num = epic['epic_number']
    epic_id = epic_num if isinstance(epic_num, str) and epic_num.startswith('EPIC-') else f"EPIC-CCN-{epic_num:03d}"
    
    manifest_path = f"docs/brain/{epic_id}/manifest.json"
    
    if not os.path.exists(manifest_path):
        skip_count += 1
        continue
    
    # Load manifest
    with open(manifest_path, 'r', encoding='utf-8') as f:
        manifest = json.load(f)
    
    # Check if Phase 1.0 exists
    if '1.0' not in manifest.get('phases', {}):
        skip_count += 1
        continue
    
    # Fix phase numbering
    phases = manifest['phases']
    dependencies = manifest.get('dependencies', {})
    
    # Rename 1.0 → 1
    if '1.0' in phases:
        phases['1'] = phases.pop('1.0')
    
    # Update dependency references
    if '1.0' in dependencies:
        dependencies['1'] = dependencies.pop('1.0')
    
    # Update Phase 1.5 dependencies (if exists)
    if '1.5' in phases and phases['1.5'].get('dependencies'):
        phases['1.5']['dependencies'] = ['1']
    
    if '1.5' in dependencies:
        dependencies['1.5'] = ['1']
    
    # Save manifest
    with open(manifest_path, 'w', encoding='utf-8') as f:
        json.dump(manifest, f, indent=2)
    
    print(f"[OK] {epic_id}: Fixed phase numbering (1.0 -> 1)")
    fixed_count += 1

print(f"\n=== Summary ===")
print(f"Fixed: {fixed_count}")
print(f"Skipped: {skip_count}")
print(f"Total: {fixed_count + skip_count}")

exit(0)

# Made with Bob
