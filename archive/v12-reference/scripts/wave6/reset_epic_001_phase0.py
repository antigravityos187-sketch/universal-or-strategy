#!/usr/bin/env python3
"""Reset EPIC-CCN-001 Phase 0 status to pending for pilot test"""

import json
import os

def reset_phase0():
    """Reset Phase 0 status to pending"""
    epic_id = 'EPIC-CCN-001'
    manifest_path = f'docs/brain/{epic_id}/manifest.json'
    
    if not os.path.exists(manifest_path):
        print(f"ERROR: Manifest not found at {manifest_path}")
        return False
    
    # Load manifest
    with open(manifest_path, 'r', encoding='utf-8') as f:
        manifest = json.load(f)
    
    # Reset Phase 0 status
    if '0' in manifest['phases']:
        old_status = manifest['phases']['0']['status']
        manifest['phases']['0']['status'] = 'pending'
        manifest['phases']['0']['started_at'] = None
        manifest['phases']['0']['completed_at'] = None
        
        # Save manifest
        with open(manifest_path, 'w', encoding='utf-8') as f:
            json.dump(manifest, f, indent=2)
        
        print(f"✅ Reset {epic_id} Phase 0: {old_status} → pending")
        return True
    else:
        print(f"ERROR: Phase 0 not found in manifest")
        return False

if __name__ == '__main__':
    success = reset_phase0()
    exit(0 if success else 1)

# Made with Bob
