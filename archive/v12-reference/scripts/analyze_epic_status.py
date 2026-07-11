#!/usr/bin/env python3
"""Analyze epic roadmap status and structure."""

import json
from collections import Counter, defaultdict
import sys

# Force UTF-8 encoding for Windows console
if sys.platform == 'win32':
    sys.stdout.reconfigure(encoding='utf-8')

# Load epic roadmap
with open('epic_roadmap.json', 'r') as f:
    data = json.load(f)

print("=" * 80)
print("EPIC ROADMAP ANALYSIS")
print("=" * 80)

# Status breakdown
statuses = Counter(e.get('status', 'pending') for e in data)
print("\nEpic Status Breakdown:")
for status, count in sorted(statuses.items()):
    print(f"  {status:15s}: {count:3d}")

total = len(data)
complete = sum(1 for e in data if e.get('status') == 'complete')
print(f"\nProgress: {complete}/{total} ({complete*100//total}%)")

# Complexity analysis
print("\nComplexity Distribution:")
cyc_ranges = defaultdict(int)
for e in data:
    cyc = e.get('cyclomatic', 0)
    if cyc <= 8:
        cyc_ranges['CYC ≤8 (DONE)'] += 1
    elif cyc <= 12:
        cyc_ranges['CYC 9-12 (HIGH)'] += 1
    elif cyc <= 20:
        cyc_ranges['CYC 13-20 (CRITICAL)'] += 1
    else:
        cyc_ranges['CYC >20 (EXTREME)'] += 1

for range_name, count in sorted(cyc_ranges.items()):
    print(f"  {range_name:20s}: {count:3d}")

# File distribution (multi-method epics)
print("\nMethods per File (Top 10):")
file_counts = Counter(e['file'] for e in data)
for file, count in file_counts.most_common(10):
    filename = file.split('/')[-1]
    print(f"  {filename:50s}: {count:2d} methods")

# Pending epics
pending = [e for e in data if e.get('status') != 'complete']
print(f"\nPending Epics: {len(pending)}")
if pending:
    print("\nTop 10 Highest Priority (by composite score):")
    for e in sorted(pending, key=lambda x: x.get('composite_score', 0), reverse=True)[:10]:
        print(f"  {e['epic_number']:15s} | {e['method']:40s} | CYC={e['cyclomatic']:2d} | Score={e['composite_score']:.1f}")

# Wave 6 specific
wave6_epics = [e for e in data if e['epic_number'].startswith('EPIC-CCN-')]
print(f"\nWave 6 Epics (EPIC-CCN-*): {len(wave6_epics)}")
wave6_complete = sum(1 for e in wave6_epics if e.get('status') == 'complete')
wave6_pending = len(wave6_epics) - wave6_complete
print(f"  Complete: {wave6_complete}")
print(f"  Pending: {wave6_pending}")

print("\n" + "=" * 80)

# Made with Bob
