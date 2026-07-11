#!/usr/bin/env python3
"""
Inject missing Phase 0 completion events into Lamport event log.
The 4 blocked epics have Phase 0 complete in their manifests but missing from event_log.jsonl.
"""

import json
from pathlib import Path
from datetime import datetime

BLOCKED_EPICS = ["001", "004", "016", "028"]
EVENT_LOG_PATH = Path(".lamport/event_log.jsonl")

def main():
    print("=== Injecting Missing Phase 0 Events ===\n")
    
    # Read existing event log
    events = []
    if EVENT_LOG_PATH.exists():
        with open(EVENT_LOG_PATH) as f:
            events = [json.loads(line) for line in f if line.strip()]
        print(f"Loaded {len(events)} existing events from event log")
    else:
        print("❌ Event log not found!")
        return
    
    # Check which epics are missing Phase 0 completion events
    missing_epics = []
    for epic_num in BLOCKED_EPICS:
        epic_id = f"EPIC-CCN-{epic_num}"
        
        # Check if Phase 0 completion event exists in log
        has_phase0_complete = any(
            e.get("epic_id") == epic_id and
            e.get("event_type") == "phase_complete" and
            e.get("phase") == "0"
            for e in events
        )
        
        if not has_phase0_complete:
            missing_epics.append(epic_num)
            print(f"❌ {epic_id}: Missing Phase 0 completion event in log")
        else:
            print(f"✅ {epic_id}: Phase 0 completion event found in log")
    
    if not missing_epics:
        print("\n✅ All 4 epics have Phase 0 completion events in log!")
        return
    
    print(f"\n📝 Injecting events for {len(missing_epics)} epics...")
    
    # Get current max clock value
    max_clock = max((e.get("clock", 0) for e in events), default=0)
    next_clock = max_clock + 1
    
    # Inject missing events
    injected_count = 0
    for epic_num in missing_epics:
        epic_id = f"EPIC-CCN-{epic_num}"
        manifest_path = Path(f"docs/brain/{epic_id}/manifest.json")
        
        if not manifest_path.exists():
            print(f"⚠️  {epic_id}: Manifest not found, skipping")
            continue
        
        with open(manifest_path) as f:
            manifest = json.load(f)
        
        # Get Phase 0 completion timestamp from manifest
        phase0_data = manifest.get("phases", {}).get("0", {})
        completed_at = phase0_data.get("completed_at", datetime.utcnow().isoformat() + "Z")
        
        # Create phase_start event
        start_event = {
            "epic_id": epic_id,
            "phase": "0",
            "event_type": "phase_start",
            "clock": next_clock,
            "timestamp": completed_at,  # Use same timestamp as completion
            "status": "running"
        }
        events.append(start_event)
        next_clock += 1
        
        # Create phase_complete event
        complete_event = {
            "epic_id": epic_id,
            "phase": "0",
            "event_type": "phase_complete",
            "clock": next_clock,
            "timestamp": completed_at,
            "status": "completed"
        }
        events.append(complete_event)
        next_clock += 1
        
        print(f"✅ {epic_id}: Injected Phase 0 events (clocks {next_clock-2}, {next_clock-1})")
        injected_count += 2
    
    # Write updated event log
    with open(EVENT_LOG_PATH, 'w') as f:
        for event in events:
            f.write(json.dumps(event) + '\n')
    
    print(f"\n✅ Injected {injected_count} events into {EVENT_LOG_PATH}")
    print(f"Total events in log: {len(events)}")

if __name__ == "__main__":
    main()

# Made with Bob
