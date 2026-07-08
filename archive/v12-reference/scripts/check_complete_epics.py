#!/usr/bin/env python3
import json

with open('epic_roadmap.json') as f:
    data = json.load(f)

complete = [e for e in data if e.get('status') == 'complete']
pending = [e for e in data if e.get('status') != 'complete']

print(f"Complete epics: {len(complete)}")
for e in complete:
    print(f"  {e['epic_number']}: {e['method']}")

print(f"\nPending epics: {len(pending)}")
print("\nFirst 10 pending:")
for i, e in enumerate(pending[:10], 1):
    print(f"  {i}. {e['epic_number']}: {e['method']} (CYC={e['cyclomatic']})")

# Made with Bob
