#!/usr/bin/env python3
"""
Clean Lamport event log entries for specific epics.
Removes all events for epics that need to be re-executed.
"""

import json
from pathlib import Path

EPICS_TO_CLEAN = ["EPIC-CCN-001", "EPIC-CCN-004", "EPIC-CCN-016", "EPIC-CCN-028"]

def clean_event_log():
    event_log_path = Path(".lamport/event_log.jsonl")
    
    if not event_log_path.exists():
        print(f"❌ Event log not found: {event_log_path}")
        return
    
    # Read all events
    events = []
    with open(event_log_path, 'r') as f:
        for line in f:
            if line.strip():
                events.append(json.loads(line))
    
    print(f"📊 Total events before cleaning: {len(events)}")
    
    # Filter out events for epics to clean
    cleaned_events = []
    removed_count = 0
    
    for event in events:
        epic_id = event.get('epic_id', '')
        if epic_id in EPICS_TO_CLEAN:
            removed_count += 1
            print(f"  🗑️  Removing: clock={event['clock']}, epic={epic_id}, phase={event.get('phase')}, event={event['event_type']}")
        else:
            cleaned_events.append(event)
    
    print(f"\n📊 Total events after cleaning: {len(cleaned_events)}")
    print(f"🗑️  Removed {removed_count} events for {len(EPICS_TO_CLEAN)} epics")
    
    # Write back cleaned events
    with open(event_log_path, 'w') as f:
        for event in cleaned_events:
            f.write(json.dumps(event) + '\n')
    
    print(f"\n✅ Event log cleaned: {event_log_path}")
    print(f"Ready to relaunch Phase 0 for: {', '.join(EPICS_TO_CLEAN)}")

if __name__ == "__main__":
    clean_event_log()

# Made with Bob
