#!/usr/bin/env python3
"""Check manifest events for blocked epics."""

import json
from pathlib import Path

BLOCKED_EPICS = ["001", "004", "016", "028"]

for epic_num in BLOCKED_EPICS:
    epic_id = f"EPIC-CCN-{epic_num}"
    manifest_path = Path(f"docs/brain/{epic_id}/manifest.json")
    
    if not manifest_path.exists():
        print(f"❌ {epic_id}: Manifest not found")
        continue
    
    with open(manifest_path) as f:
        manifest = json.load(f)
    
    events = manifest.get("lamport_events", [])
    print(f"\n{epic_id}: {len(events)} events")
    
    for i, event in enumerate(events):
        phase = event.get("phase")
        event_type = event.get("event_type")
        status = event.get("status")
        print(f"  {i}: phase={phase}, type={event_type}, status={status}")
    
    # Check for Phase 0 completion
    phase0_complete = any(
        e.get("phase") == "0" and
        e.get("event_type") == "phase_complete" and
        e.get("status") == "completed"
        for e in events
    )
    
    print(f"  Phase 0 complete: {'✅' if phase0_complete else '❌'}")

# Made with Bob
