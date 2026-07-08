#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Fix status field in lamport_events for 4 blocked epics."""

import json
import sys
from pathlib import Path

# Fix Windows console encoding
if sys.platform == 'win32':
    sys.stdout.reconfigure(encoding='utf-8')

BLOCKED_EPICS = ["001", "004", "016", "028"]

for epic_num in BLOCKED_EPICS:
    epic_id = f"EPIC-CCN-{epic_num}"
    manifest_path = Path(f"docs/brain/{epic_id}/manifest.json")
    
    if not manifest_path.exists():
        print(f"[X] {epic_id}: Manifest not found")
        continue
    
    with open(manifest_path) as f:
        manifest = json.load(f)
    
    events = manifest.get("lamport_events", [])
    if not events:
        print(f"[X] {epic_id}: No lamport_events")
        continue
    
    # Fix status fields
    fixed = False
    for event in events:
        if event.get("event_type") == "phase_start" and event.get("status") is None:
            event["status"] = "running"
            event["epic_id"] = epic_id
            fixed = True
        elif event.get("event_type") == "phase_complete" and event.get("status") is None:
            event["status"] = "completed"
            event["epic_id"] = epic_id
            fixed = True
    
    if fixed:
        with open(manifest_path, 'w') as f:
            json.dump(manifest, f, indent=2)
        print(f"[OK] {epic_id}: Fixed {len(events)} events")
    else:
        print(f"[SKIP] {epic_id}: Already fixed")

# Made with Bob
