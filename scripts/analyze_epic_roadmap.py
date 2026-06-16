#!/usr/bin/env python3
"""Analyze epic_roadmap.json to understand epic count and status."""

import json
from collections import Counter

with open('epic_roadmap.json', 'r') as f:
    data = json.load(f)

print(f"Total epics in roadmap: {len(data)}")
print()

# Get all epic numbers
epic_numbers = sorted([e["epic_number"] for e in data])
print(f"Epic number range: {epic_numbers[0]} to {epic_numbers[-1]}")
print()

# Status breakdown
statuses = Counter([e.get("status", "pending") for e in data])
print("Status breakdown:")
for status, count in statuses.most_common():
    print(f"  {status}: {count}")
print()

# Check for gaps in 001-080 range
expected = set([f"EPIC-CCN-{i:03d}" for i in range(1, 81)])
actual = set(epic_numbers)
missing = sorted(expected - actual)
extra = sorted(actual - expected)

print(f"Expected epics (001-080): {len(expected)}")
print(f"Actual epics in roadmap: {len(actual)}")
print()

if missing:
    print(f"Missing from 001-080 range ({len(missing)} epics):")
    for epic in missing[:20]:  # Show first 20
        print(f"  {epic}")
    if len(missing) > 20:
        print(f"  ... and {len(missing) - 20} more")
    print()

if extra:
    print(f"Extra epics beyond 080 ({len(extra)} epics):")
    for epic in extra[:20]:  # Show first 20
        print(f"  {epic}")
    if len(extra) > 20:
        print(f"  ... and {len(extra) - 20} more")
    print()

# Analyze 001-080 range specifically
range_001_080 = [e for e in data if e["epic_number"] in expected]
print(f"Epics in 001-080 range: {len(range_001_080)}")

range_statuses = Counter([e.get("status", "pending") for e in range_001_080])
print("Status breakdown for 001-080:")
for status, count in range_statuses.most_common():
    print(f"  {status}: {count}")

# Made with Bob
