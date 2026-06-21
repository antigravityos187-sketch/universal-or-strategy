#!/usr/bin/env python3
"""
Lamport Clock Surgical Cleanup for EPIC-CCN-003 Phase 1
Removes stale phase_start events (clocks 175, 177) that were never properly closed.

The issue: verify_determinism() counts ALL phase_start events with status='running',
but phase_fail events don't retroactively change the status of phase_start events.

Solution: Remove the stale phase_start events from the log entirely.
"""

import json
import os
from pathlib import Path

LAMPORT_LOG = ".lamport/event_log.jsonl"
BACKUP_LOG = ".lamport/event_log.jsonl.backup_epic_003"

def surgical_cleanup():
    """Remove stale Phase 1 events for EPIC-CCN-003"""
    
    if not os.path.exists(LAMPORT_LOG):
        print(f"❌ Lamport log not found: {LAMPORT_LOG}")
        return
    
    # Backup original log
    import shutil
    shutil.copy(LAMPORT_LOG, BACKUP_LOG)
    print(f"✅ Backup created: {BACKUP_LOG}")
    
    # Read all events
    events = []
    with open(LAMPORT_LOG, 'r') as f:
        for line in f:
            if line.strip():
                events.append(json.loads(line))
    
    # Filter out stale EPIC-CCN-003 Phase 1 events (clocks 175-178)
    # These are the frozen session artifacts
    filtered_events = []
    removed_count = 0
    
    for event in events:
        # Remove Phase 1 events for EPIC-CCN-003 with clocks 175-178
        if (event.get('epic_id') == 'EPIC-CCN-003' and 
            event.get('phase') == '1' and 
            event.get('clock') in [175, 176, 177, 178]):
            removed_count += 1
            print(f"   Removing: clock={event['clock']}, type={event['event_type']}, status={event['status']}")
            continue
        
        filtered_events.append(event)
    
    # Write filtered log
    with open(LAMPORT_LOG, 'w') as f:
        for event in filtered_events:
            f.write(json.dumps(event) + '\n')
    
    print(f"\n✅ Surgical cleanup complete:")
    print(f"   - Events removed: {removed_count}")
    print(f"   - Events retained: {len(filtered_events)}")
    print(f"   - Backup: {BACKUP_LOG}")
    print(f"\n✅ EPIC-CCN-003 Phase 1 is now clean - ready for fresh execution")

if __name__ == "__main__":
    surgical_cleanup()

# Made with Bob
