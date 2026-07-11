#!/usr/bin/env python3
"""Fix Wave 1 epic targets to CYC=8 and prepare for re-run."""

import json
import os
import shutil
from datetime import datetime

# Wave 1 epic IDs
wave1_epics = [
    'EPIC-CCN-21', 'EPIC-CCN-22', 'EPIC-CCN-23', 'EPIC-CCN-24', 'EPIC-CCN-25',
    'EPIC-CCN-26', 'EPIC-CCN-27', 'EPIC-CCN-28', 'EPIC-CCN-29', 'EPIC-CCN-30'
]

print("Fixing Wave 1 Epic Targets")
print("=" * 80)

for epic_id in wave1_epics:
    manifest_path = f"docs/brain/{epic_id}/manifest.json"
    
    if os.path.exists(manifest_path):
        # Backup existing manifest
        backup_path = f"{manifest_path}.backup-{datetime.now().strftime('%Y%m%d-%H%M%S')}"
        shutil.copy2(manifest_path, backup_path)
        print(f"\n{epic_id}:")
        print(f"  - Backed up to: {backup_path}")
        
        # Update target_cyc to 8
        with open(manifest_path, 'r') as f:
            data = json.load(f)
        
        old_target = data.get('target_cyc', '?')
        data['target_cyc'] = 8
        data['status'] = 'pending'  # Reset status
        data['recommendation'] = 'pending_analysis'  # Reset recommendation
        
        # Reset Phase 0 status to pending
        if 'phases' in data and '0' in data['phases']:
            data['phases']['0']['status'] = 'pending'
        
        with open(manifest_path, 'w') as f:
            json.dump(data, f, indent=2)
        
        print(f"  - Updated target_cyc: {old_target} -> 8")
        print(f"  - Reset status to: pending")
    else:
        print(f"\n{epic_id}: No manifest (will be created by Phase 0)")

print("\n" + "=" * 80)
print("[OK] Wave 1 targets fixed. Ready to re-run Phase 0 for all 10 epics.")

# Made with Bob
