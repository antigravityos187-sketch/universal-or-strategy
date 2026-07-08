#!/usr/bin/env python3
"""Check which epics are in the roadmap."""

import json

with open('epic_roadmap.json') as f:
    data = json.load(f)

print(f"Total epics in roadmap: {len(data)}")
epic_numbers = sorted([e['epic_number'] for e in data])
print(f"\nEpic numbers ({len(epic_numbers)} total):")
for i, epic in enumerate(epic_numbers, 1):
    print(f"{i:2d}. {epic}")

# Check for 027 and 045
if 'EPIC-CCN-027' in epic_numbers:
    print("\n✅ EPIC-CCN-027 is in roadmap")
else:
    print("\n❌ EPIC-CCN-027 is NOT in roadmap")

if 'EPIC-CCN-045' in epic_numbers:
    print("✅ EPIC-CCN-045 is in roadmap")
else:
    print("❌ EPIC-CCN-045 is NOT in roadmap")

# Check what's missing from 001-080
expected = [f"EPIC-CCN-{i:03d}" for i in range(1, 81)]
missing = [e for e in expected if e not in epic_numbers]
if missing:
    print(f"\n⚠️  Missing from 001-080 sequence: {missing}")

# Made with Bob
