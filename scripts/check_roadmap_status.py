#!/usr/bin/env python3
"""Check epic roadmap completion status."""

import json
from pathlib import Path

# Load roadmap
with open('epic_roadmap.json') as f:
    epics = json.load(f)

# Analyze status
total = len(epics)
complete = [e for e in epics if e.get('status') == 'complete']
incomplete = [e for e in epics if e.get('status') != 'complete']
no_status = [e for e in epics if 'status' not in e]

print(f"Total Epics: {total}")
print(f"Complete: {len(complete)} ({len(complete)/total*100:.1f}%)")
print(f"Incomplete: {len(incomplete)} ({len(incomplete)/total*100:.1f}%)")
print(f"No Status: {len(no_status)}")
print()

if incomplete:
    print("Incomplete Epics:")
    for e in incomplete[:10]:
        epic_num = e['epic_number']
        status = e.get('status', 'NO STATUS')
        method = e['method']
        print(f"  EPIC-CCN-{epic_num}: {status} - {method}")
    
    if len(incomplete) > 10:
        print(f"  ... and {len(incomplete) - 10} more")

# Made with Bob
