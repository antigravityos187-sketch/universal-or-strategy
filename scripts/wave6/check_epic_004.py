#!/usr/bin/env python3
import json
m = json.load(open("docs/brain/EPIC-CCN-004/manifest.json"))
events = m.get("lamport_events", [])
print(f"Events: {len(events)}")
for i, e in enumerate(events):
    print(f"  {i}: epic_id={e.get('epic_id')}, phase={e.get('phase')}, type={e.get('event_type')}")

# Made with Bob
