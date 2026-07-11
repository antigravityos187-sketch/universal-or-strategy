#!/usr/bin/env python3
"""
Filter Wave 7 events from global event log.

Extracts all events for Wave 7 epics (EPIC-W7-*) from the global
event log and writes them to .lamport/wave7/event_log.jsonl.

Usage:
    python scripts/filter_wave7_events.py
"""

import json
from pathlib import Path
from typing import List, Dict


def filter_wave7_events() -> List[Dict]:
    """
    Filter Wave 7 events from global event log.
    
    Returns:
        List of Wave 7 events in causal order (sorted by clock)
    """
    global_log = Path(".lamport/event_log.jsonl")
    if not global_log.exists():
        print("No global event log found")
        return []
    
    wave7_events = []
    with open(global_log, 'r') as f:
        for line in f:
            event = json.loads(line.strip())
            epic_id = event.get('epic_id', '')
            
            # Filter Wave 7 epics (EPIC-W7-*)
            if epic_id.startswith('EPIC-W7-'):
                wave7_events.append(event)
    
    # Sort by clock (causal order)
    wave7_events.sort(key=lambda e: e.get('clock', 0))
    
    return wave7_events


def write_wave7_log(events: List[Dict]):
    """
    Write Wave 7 events to wave-specific log.
    
    Args:
        events: List of Wave 7 events
    """
    wave7_dir = Path(".lamport/wave7")
    wave7_dir.mkdir(parents=True, exist_ok=True)
    
    wave7_log = wave7_dir / "event_log.jsonl"
    with open(wave7_log, 'w') as f:
        for event in events:
            f.write(json.dumps(event) + '\n')
    
    print(f"Wrote {len(events)} Wave 7 events to {wave7_log}")


def main():
    """Main entry point."""
    print("Filtering Wave 7 events from global log...")
    events = filter_wave7_events()
    
    if events:
        write_wave7_log(events)
        
        # Print summary
        epic_ids = set(e['epic_id'] for e in events)
        phases = set(e['phase'] for e in events)
        
        print(f"\nSummary:")
        print(f"  Total events: {len(events)}")
        print(f"  Unique epics: {len(epic_ids)}")
        print(f"  Phases: {sorted(phases)}")
        
        # Count by status
        status_counts = {}
        for event in events:
            status = event.get('status', 'unknown')
            status_counts[status] = status_counts.get(status, 0) + 1
        
        print(f"\nStatus breakdown:")
        for status, count in sorted(status_counts.items()):
            print(f"  {status}: {count}")
    else:
        print("No Wave 7 events found")


if __name__ == "__main__":
    main()

# Made with Bob
