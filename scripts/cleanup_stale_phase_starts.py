#!/usr/bin/env python3
"""
Clean up stale phase_start events from global event log.

Removes phase_start events that don't have a corresponding phase_complete.
These are from failed/frozen execution attempts.
"""

import json
from pathlib import Path
from collections import defaultdict

def cleanup_event_log():
    """Remove stale phase_start events from global log."""
    event_log_path = Path(".lamport/event_log.jsonl")
    
    if not event_log_path.exists():
        print("[INFO] No event log found")
        return
    
    # Read all events
    events = []
    with open(event_log_path, 'r') as f:
        for line in f:
            if line.strip():
                events.append(json.loads(line))
    
    print(f"Total events: {len(events)}")
    
    # Track completions by (epic_id, phase)
    completions = defaultdict(list)
    for event in events:
        if event.get('event_type') == 'phase_complete':
            key = (event.get('epic_id'), event.get('phase'))
            completions[key].append(event)
    
    # Filter out phase_start events that have no corresponding phase_complete
    # OR are duplicates (multiple phase_start for same epic/phase)
    seen_starts = defaultdict(list)
    clean_events = []
    
    for event in events:
        event_type = event.get('event_type')
        epic_id = event.get('epic_id')
        phase = event.get('phase')
        key = (epic_id, phase)
        
        if event_type == 'phase_start':
            # Check if this phase was completed
            if key in completions:
                # Phase was completed - keep only the first phase_start
                if len(seen_starts[key]) == 0:
                    clean_events.append(event)
                    seen_starts[key].append(event)
                else:
                    print(f"[REMOVE] Duplicate phase_start: {epic_id} Phase {phase} (clock {event['clock']})")
            else:
                # Phase not completed - this is a stale start event
                print(f"[REMOVE] Stale phase_start: {epic_id} Phase {phase} (clock {event['clock']})")
        else:
            # Keep all non-phase_start events
            clean_events.append(event)
    
    print(f"Clean events: {len(clean_events)}")
    print(f"Removed: {len(events) - len(clean_events)} stale/duplicate events")
    
    # Rewrite log
    with open(event_log_path, 'w') as f:
        for event in clean_events:
            f.write(json.dumps(event) + '\n')
    
    print(f"[OK] Event log cleaned")

def main():
    print("Cleaning up stale phase_start events...")
    print("=" * 60)
    cleanup_event_log()
    print("=" * 60)
    print("[OK] Cleanup complete")
    print("\nNext: Verify Phase 1.5 can execute")

if __name__ == '__main__':
    main()

# Made with Bob
