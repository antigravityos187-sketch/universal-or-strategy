#!/usr/bin/env python3
"""Validate epic_id in lamport_events matches manifest epic for all 24 epics."""

import json
import sys
from pathlib import Path

# All 24 Wave 6 Phase 1 epics
EPICS = [f"{i:03d}" for i in [1, 4, 16, 20, 21, 28] + list(range(50, 62)) + [70, 73] + list(range(76, 80))]

mismatches = []
fixed = []

for epic_num in EPICS:
    epic_id = f"EPIC-CCN-{epic_num}"
    manifest_path = Path(f"docs/brain/{epic_id}/manifest.json")
    
    if not manifest_path.exists():
        print(f"[SKIP] {epic_id}: Manifest not found")
        continue
    
    with open(manifest_path) as f:
        manifest = json.load(f)
    
    events = manifest.get("lamport_events", [])
    if not events:
        print(f"[SKIP] {epic_id}: No lamport_events")
        continue
    
    # Check for epic_id mismatches
    for i, event in enumerate(events):
        event_epic_id = event.get("epic_id")
        if event_epic_id and event_epic_id != epic_id:
            mismatches.append((epic_id, event_epic_id, i))
            print(f"[MISMATCH] {epic_id}: Event {i} has epic_id={event_epic_id}")
            # Fix it
            event["epic_id"] = epic_id
            fixed.append(epic_id)
    
    # Write back if fixed
    if epic_id in fixed:
        with open(manifest_path, 'w') as f:
            json.dump(manifest, f, indent=2)

print(f"\n=== Summary ===")
print(f"Total epics checked: {len(EPICS)}")
print(f"Mismatches found: {len(mismatches)}")
print(f"Epics fixed: {len(set(fixed))}")

if mismatches:
    print(f"\nFixed epics: {sorted(set(e[0] for e in mismatches))}")
    sys.exit(1)
else:
    print("\n[OK] All epic_ids match!")
    sys.exit(0)

# Made with Bob
