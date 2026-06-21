#!/usr/bin/env python3
"""
Reset Phase 1 status to 'pending' for 24 blocked epics
Fixes "Invalid status transition: in_progress -> in_progress" error
"""

import json
from pathlib import Path

# 24 blocked epics
BLOCKED_EPICS = [
    "001", "004", "016", "020", "021", "028", "050", "051", "052", "053",
    "054", "055", "056", "057", "058", "059", "060", "061", "070", "073",
    "076", "077", "078", "079"
]

def reset_phase1_status(epic_num):
    """Reset Phase 1 status to pending for an epic."""
    epic_id = f"EPIC-CCN-{epic_num}"
    manifest_path = Path(f"docs/brain/{epic_id}/manifest.json")
    
    if not manifest_path.exists():
        print(f"  ✗ Manifest not found: {manifest_path}")
        return False
    
    try:
        with open(manifest_path, 'r') as f:
            manifest = json.load(f)
        
        # Reset Phase 1 status
        if '1' in manifest.get('phases', {}):
            old_status = manifest['phases']['1'].get('status')
            manifest['phases']['1']['status'] = 'pending'
            
            # Remove timestamps if present
            manifest['phases']['1'].pop('started_at', None)
            manifest['phases']['1'].pop('completed_at', None)
            
            with open(manifest_path, 'w') as f:
                json.dump(manifest, f, indent=2)
            
            print(f"  ✓ {epic_id}: {old_status} → pending")
            return True
        else:
            print(f"  - {epic_id}: Phase 1 not in manifest")
            return False
            
    except Exception as e:
        print(f"  ✗ {epic_id}: Error - {e}")
        return False

def main():
    print("=== Resetting Phase 1 Status for 24 Blocked Epics ===\n")
    
    success_count = 0
    for epic_num in BLOCKED_EPICS:
        if reset_phase1_status(epic_num):
            success_count += 1
    
    print(f"\n=== Complete: {success_count}/{len(BLOCKED_EPICS)} epics reset ===")

if __name__ == "__main__":
    main()

# Made with Bob
