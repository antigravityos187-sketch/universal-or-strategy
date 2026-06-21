#!/usr/bin/env python3
"""Investigate the 8 'complete' epics and check for special cases."""

import json
from pathlib import Path

# Load epic roadmap
with open('epic_roadmap.json', 'r') as f:
    data = json.load(f)

wave6 = [e for e in data if e['epic_number'].startswith('EPIC-CCN-')]
complete = [e for e in wave6 if e.get('status') == 'complete']

print("=" * 80)
print("INVESTIGATING 'COMPLETE' EPICS")
print("=" * 80)

print(f"\nFound {len(complete)} epics marked as 'complete':\n")

for e in complete:
    print(f"\n{e['epic_number']}: {e['method']}")
    print(f"  File: {e['file']}")
    print(f"  Status: {e.get('status')}")
    print(f"  Final CYC: {e.get('final_cyc', 'N/A')}")
    print(f"  Completion Date: {e.get('completion_date', 'N/A')}")
    print(f"  Build Tag: {e.get('build_tag', 'N/A')}")
    print(f"  Notes: {e.get('notes', 'N/A')}")
    
    # Check if brain directory exists
    brain_dir = Path(f'docs/brain/{e["epic_number"]}')
    if brain_dir.exists():
        files = list(brain_dir.glob('*.md'))
        print(f"  Brain files: {len(files)} files")
        for f in sorted(files):
            print(f"    - {f.name}")
    else:
        print(f"  Brain directory: NOT FOUND")

# Check for special cases
print("\n" + "=" * 80)
print("CHECKING FOR SPECIAL CASES")
print("=" * 80)

# EPIC-CCN-027 (user mentioned)
epic_27 = next((e for e in wave6 if e['epic_number'] == 'EPIC-CCN-027'), None)
if epic_27:
    print(f"\nEPIC-CCN-027 (User mentioned):")
    print(f"  Method: {epic_27['method']}")
    print(f"  File: {epic_27['file']}")
    print(f"  Status: {epic_27.get('status', 'pending')}")
    print(f"  Notes: {epic_27.get('notes', 'N/A')}")
else:
    print("\nEPIC-CCN-027: NOT FOUND in roadmap")

# Check for .dll related epics
print("\n\nSearching for .dll related epics:")
dll_epics = [e for e in wave6 if '.dll' in e.get('notes', '').lower() or 
             '.dll' in e.get('file', '').lower() or
             'dll' in e['method'].lower()]

if dll_epics:
    for e in dll_epics:
        print(f"\n  {e['epic_number']}: {e['method']}")
        print(f"    File: {e['file']}")
        print(f"    Status: {e.get('status', 'pending')}")
        print(f"    Notes: {e.get('notes', 'N/A')}")
else:
    print("  No .dll related epics found in roadmap")

# Check for EPIC-003 (user mentioned as local epic)
epic_003 = next((e for e in wave6 if e['epic_number'] == 'EPIC-CCN-003'), None)
if epic_003:
    print(f"\n\nEPIC-CCN-003 (Possible local epic):")
    print(f"  Method: {epic_003['method']}")
    print(f"  File: {epic_003['file']}")
    print(f"  Status: {epic_003.get('status', 'pending')}")
    print(f"  Notes: {epic_003.get('notes', 'N/A')}")

print("\n" + "=" * 80)

# Made with Bob
