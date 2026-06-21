#!/usr/bin/env python3
"""
Lamport Clock Complete Cleanup for EPIC-CCN-003 Phase 1
Removes ALL Phase 1 events for EPIC-CCN-003 to allow fresh start.
"""

import json
import os
import shutil

LAMPORT_LOG = ".lamport/event_log.jsonl"
BACKUP_LOG = ".lamport/event_log.jsonl.backup_all_phase1"

def cleanup_all_phase1():
    """Remove ALL Phase 1 events for EPIC-CCN-003"""
    
    if not os.path.exists(LAMPORT_LOG):
        print(f"❌ Lamport log not found: {LAMPORT_LOG}")
        return
    
    # Backup original log
    shutil.copy(LAMPORT_LOG, BACKUP_LOG)
    print(f"✅ Backup created: {BACKUP_LOG}")
    
    # Read all events
    events = []
    with open(LAMPORT_LOG, 'r') as f:
        for line in f:
            if line.strip():
                events.append(json.loads(line))
    
    # Filter out ALL EPIC-CCN-003 Phase 1 events
    filtered_events = []
    removed_count = 0
    
    for event in events:
        # Remove ALL Phase 1 events for EPIC-CCN-003
        if (event.get('epic_id') == 'EPIC-CCN-003' and 
            event.get('phase') == '1'):
            removed_count += 1
            print(f"   Removing: clock={event['clock']}, type={event['event_type']}, status={event['status']}")
            continue
        
        filtered_events.append(event)
    
    # Write filtered log
    with open(LAMPORT_LOG, 'w') as f:
        for event in filtered_events:
            f.write(json.dumps(event) + '\n')
    
    print(f"\n✅ Complete cleanup:")
    print(f"   - Events removed: {removed_count}")
    print(f"   - Events retained: {len(filtered_events)}")
    print(f"   - Backup: {BACKUP_LOG}")
    print(f"\n✅ EPIC-CCN-003 Phase 1 is completely clean - ready for fresh execution")

if __name__ == "__main__":
    cleanup_all_phase1()

# Made with Bob
