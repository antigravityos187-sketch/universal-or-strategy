#!/usr/bin/env python3
"""Prepare Wave 1 epic data for Phase 0 execution."""

import json

# Load roadmap
with open('epic_roadmap.json') as f:
    roadmap = json.load(f)

# Wave 1 epic numbers
wave1_numbers = [21, 22, 23, 24, 25, 26, 27, 28, 29, 30]

print("Wave 1 Epic Data for Phase 0 Execution")
print("=" * 80)

wave1_epics = []
for epic in roadmap:
    epic_num = int(epic['epic_number'].split('-')[-1])
    if epic_num in wave1_numbers:
        wave1_epics.append({
            'epic_id': epic['epic_number'],
            'method': epic['method'],
            'file': epic['file'],
            'cyc': epic['cyclomatic']
        })

# Sort by epic number
wave1_epics.sort(key=lambda x: int(x['epic_id'].split('-')[-1]))

print(f"\nFound {len(wave1_epics)} Wave 1 epics:\n")
for epic in wave1_epics:
    print(f"{epic['epic_id']}: {epic['method']} (CYC={epic['cyc']}) in {epic['file']}")

print("\n" + "=" * 80)
print(f"Ready to execute Phase 0 for {len(wave1_epics)} epics")
print("Target CYC: 8 (all epics)")

# Made with Bob
