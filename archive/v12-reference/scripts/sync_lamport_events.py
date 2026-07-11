#!/usr/bin/env python3
"""
Sync Lamport events from manifests to global event log.
Used to backfill events after fixing the import bug.
"""

import json
from pathlib import Path

def sync_events_to_global_log(epic_id: str):
    """Sync manifest Lamport events to global event log."""
    manifest_path = Path(f"docs/brain/{epic_id}/manifest.json")
    event_log_path = Path(".lamport/event_log.jsonl")
    
    if not manifest_path.exists():
        print(f"[ERROR] Manifest not found: {manifest_path}")
        return False
    
    # Ensure event log directory exists
    event_log_path.parent.mkdir(parents=True, exist_ok=True)
    
    # Load manifest
    with open(manifest_path, 'r') as f:
        manifest = json.load(f)
    
    # Load existing events
    existing_events = []
    if event_log_path.exists():
        with open(event_log_path, 'r') as f:
            existing_events = [json.loads(line) for line in f if line.strip()]
    
    # Check which events already exist
    existing_keys = {(e.get('epic_id'), e.get('phase'), e.get('event_type')) for e in existing_events}
    
    # Collect events to add
    events_to_add = []
    for phase_id, phase_data in manifest['phases'].items():
        for event in phase_data.get('lamport_events', []):
            event_key = (epic_id, phase_id, event['event_type'])
            if event_key not in existing_keys:
                # Add epic_id to event
                global_event = {
                    'epic_id': epic_id,
                    'phase': phase_id,
                    **event
                }
                events_to_add.append(global_event)
                print(f"  [ADD] {epic_id} Phase {phase_id} {event['event_type']}")
    
    if events_to_add:
        # Append to global log
        with open(event_log_path, 'a') as f:
            for event in events_to_add:
                f.write(json.dumps(event) + '\n')
        print(f"[OK] {epic_id}: Added {len(events_to_add)} events to global log")
    else:
        print(f"[INFO] {epic_id}: No new events to add")
    
    return True

def main():
    pilot_epics = ['EPIC-CCN-001', 'EPIC-CCN-002', 'EPIC-CCN-004']
    
    print("Syncing Lamport events to global log...")
    print("=" * 60)
    
    for epic_id in pilot_epics:
        print(f"\n{epic_id}:")
        sync_events_to_global_log(epic_id)
    
    print("\n" + "=" * 60)
    print("[OK] Event sync complete")
    print("\nNext: Verify Phase 1.5 can execute")

if __name__ == '__main__':
    main()

# Made with Bob
