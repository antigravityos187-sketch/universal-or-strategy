#!/usr/bin/env python3
"""
Remove phase_start events from completed phases in manifests.

For completed phases, we only need the phase_complete event.
Having phase_start with status='running' makes it look like
there are concurrent agents executing.
"""

import json
from pathlib import Path

def fix_manifest(epic_id: str):
    """Remove phase_start events from completed phases."""
    manifest_path = Path(f"docs/brain/{epic_id}/manifest.json")
    
    if not manifest_path.exists():
        print(f"[SKIP] {epic_id}: No manifest found")
        return False
    
    with open(manifest_path, 'r') as f:
        manifest = json.load(f)
    
    modified = False
    for phase_id, phase_data in manifest.get('phases', {}).items():
        # Only process completed phases
        if phase_data.get('status') != 'completed':
            continue
        
        events = phase_data.get('lamport_events', [])
        if not events:
            continue
        
        # Keep only phase_complete events for completed phases
        original_count = len(events)
        phase_data['lamport_events'] = [
            e for e in events
            if e.get('event_type') == 'phase_complete'
        ]
        new_count = len(phase_data['lamport_events'])
        
        if new_count < original_count:
            print(f"  Phase {phase_id}: Removed {original_count - new_count} phase_start events")
            modified = True
    
    if modified:
        with open(manifest_path, 'w') as f:
            json.dump(manifest, f, indent=4)
        print(f"[OK] {epic_id}: Fixed manifest")
        return True
    else:
        print(f"[SKIP] {epic_id}: No changes needed")
        return False

def main():
    print("Removing phase_start events from completed phases...")
    print("=" * 60)
    
    # Fix pilot epics
    pilot_epics = ['EPIC-CCN-001', 'EPIC-CCN-002', 'EPIC-CCN-004']
    
    fixed_count = 0
    for epic_id in pilot_epics:
        print(f"\n{epic_id}:")
        if fix_manifest(epic_id):
            fixed_count += 1
    
    print("\n" + "=" * 60)
    print(f"[OK] Fixed {fixed_count}/{len(pilot_epics)} manifests")
    print("\nNext: Verify Phase 1.5 can execute")

if __name__ == '__main__':
    main()

# Made with Bob
