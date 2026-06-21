#!/usr/bin/env python3
"""
Fix synthetic events in manifests to include status field.
The verification code expects events to have a 'status' field.
"""

import json
from pathlib import Path

def fix_manifest_events(epic_id: str):
    """Add status field to synthetic events in manifest."""
    manifest_path = Path(f"docs/brain/{epic_id}/manifest.json")
    
    if not manifest_path.exists():
        print(f"[SKIP] {epic_id}: No manifest found")
        return False
    
    with open(manifest_path, 'r') as f:
        manifest = json.load(f)
    
    modified = False
    for phase_id, phase_data in manifest.get('phases', {}).items():
        events = phase_data.get('lamport_events', [])
        for event in events:
            # Add status field based on event_type
            if 'status' not in event:
                if event.get('event_type') == 'phase_start':
                    event['status'] = 'running'
                    modified = True
                elif event.get('event_type') == 'phase_complete':
                    event['status'] = 'completed'
                    modified = True
    
    if modified:
        with open(manifest_path, 'w') as f:
            json.dump(manifest, f, indent=4)
        print(f"[OK] {epic_id}: Fixed synthetic events")
        return True
    else:
        print(f"[SKIP] {epic_id}: No changes needed")
        return False

def main():
    print("Fixing synthetic events in manifests...")
    print("=" * 60)
    
    # Fix pilot epics
    pilot_epics = ['EPIC-CCN-001', 'EPIC-CCN-002', 'EPIC-CCN-004']
    
    fixed_count = 0
    for epic_id in pilot_epics:
        if fix_manifest_events(epic_id):
            fixed_count += 1
    
    print("=" * 60)
    print(f"[OK] Fixed {fixed_count}/{len(pilot_epics)} manifests")
    print("\nNext: Verify Phase 1.5 can execute")

if __name__ == '__main__':
    main()

# Made with Bob
