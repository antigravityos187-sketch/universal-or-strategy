#!/usr/bin/env python3
"""
Fix synthetic events to have proper status.
The issue: synthetic phase_start events have no status field,
causing them to be counted as 'running' agents.
"""

import json
from pathlib import Path

def fix_event_log():
    """Remove synthetic events from global log - they're in manifests."""
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
    
    # Filter out synthetic events (they have 'note' field)
    real_events = [e for e in events if 'note' not in e or 'Synthetic' not in e.get('note', '')]
    
    print(f"Total events: {len(events)}")
    print(f"Synthetic events: {len(events) - len(real_events)}")
    print(f"Real events: {len(real_events)}")
    
    # Rewrite log with only real events
    with open(event_log_path, 'w') as f:
        for event in real_events:
            f.write(json.dumps(event) + '\n')
    
    print(f"[OK] Event log cleaned - removed {len(events) - len(real_events)} synthetic events")

def main():
    print("Fixing synthetic events...")
    print("=" * 60)
    fix_event_log()
    print("=" * 60)
    print("[OK] Synthetic events removed from global log")
    print("\nNote: Synthetic events remain in manifests for dependency checking")
    print("Next: Verify Phase 1.5 can execute")

if __name__ == '__main__':
    main()

# Made with Bob
