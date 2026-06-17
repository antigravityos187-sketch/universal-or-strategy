#!/usr/bin/env python3
"""Add missing dependencies dict to V12.52 manifests"""

import json
import os
import sys

# Standard V12 phase dependencies
STANDARD_DEPENDENCIES = {
    "0": [],  # Phase 0 has no dependencies
    "1": ["0"],  # Phase 1 depends on Phase 0
    "1.5": ["1"],  # Phase 1.5 depends on Phase 1
    "2": ["1.5"],  # Phase 2 depends on Phase 1.5
    "3": ["2"],  # Phase 3 depends on Phase 2
    "4": ["3"]  # Phase 4 depends on Phase 3
}

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
    
    # Check if dependencies dict exists and is populated
    if manifest.get('dependencies') and len(manifest['dependencies']) > 0:
        skip_count += 1
        continue
    
    # Add dependencies dict
    dependencies = {}
    
    # Add standard dependencies for phases that exist
    for phase_id in manifest.get('phases', {}):
        if phase_id in STANDARD_DEPENDENCIES:
            dependencies[phase_id] = STANDARD_DEPENDENCIES[phase_id]
        elif phase_id.startswith('5.') and not phase_id.endswith('.V'):
            # Ticket execution phases depend on Phase 4
            dependencies[phase_id] = ["4"]
        elif phase_id.endswith('.V'):
            # Verification phases depend on their ticket
            ticket_id = phase_id[:-2]  # Remove '.V'
            dependencies[phase_id] = [ticket_id]
        elif phase_id == '6':
            # Phase 6 depends on all verification phases
            verify_phases = [p for p in manifest['phases'] if p.endswith('.V')]
            dependencies[phase_id] = verify_phases if verify_phases else ["4"]
    
    # Update manifest
    manifest['dependencies'] = dependencies
    
    # Save manifest
    with open(manifest_path, 'w', encoding='utf-8') as f:
        json.dump(manifest, f, indent=2)
    
    print(f"[OK] {epic_id}: Added dependencies dict ({len(dependencies)} phases)")
    fixed_count += 1

print(f"\n=== Summary ===")
print(f"Fixed: {fixed_count}")
print(f"Skipped: {skip_count}")
print(f"Total: {fixed_count + skip_count}")

exit(0)

# Made with Bob