#!/usr/bin/env python3
"""Validate epic_roadmap_wave4_fresh.json for Wave 6"""

import json

# Load roadmap
with open('epic_roadmap_wave4_fresh.json', 'r', encoding='utf-8-sig') as f:
    roadmap = json.load(f)

print(f"Total epics in roadmap: {len(roadmap)}")
print(f"\nFirst 5 epics:")
for epic in roadmap[:5]:
    print(f"  {epic['epic_number']}: {epic['method']} (CYC {epic['cyclomatic']})")

print(f"\nLast 5 epics:")
for epic in roadmap[-5:]:
    print(f"  {epic['epic_number']}: {epic['method']} (CYC {epic['cyclomatic']})")

# Check Wave 6 scope (001-080, excluding 024, 027)
wave6_count = 0
for epic in roadmap:
    epic_num = int(epic['epic_number'].split('-')[-1])
    if 1 <= epic_num <= 80 and epic_num not in [24, 27]:
        wave6_count += 1

print(f"\nWave 6 scope: {wave6_count} epics (001-080, excluding 024, 027)")
print(f"Expected: 78 epics")
print(f"Match: {'✓ YES' if wave6_count == 78 else '✗ NO'}")

# Check for required fields
print(f"\nValidating required fields...")
required_fields = ['epic_number', 'method', 'file', 'cyclomatic']
missing_fields = []
for epic in roadmap:
    for field in required_fields:
        if field not in epic:
            missing_fields.append(f"{epic.get('epic_number', 'UNKNOWN')}: missing {field}")

if missing_fields:
    print(f"✗ Missing fields found:")
    for msg in missing_fields[:10]:  # Show first 10
        print(f"  {msg}")
else:
    print(f"✓ All required fields present")

print(f"\nRoadmap validation: {'✓ PASS' if wave6_count == 78 and not missing_fields else '✗ FAIL'}")

# Made with Bob
