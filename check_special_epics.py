#!/usr/bin/env python3
"""Check for special epics that need local execution or are invalid."""

import json

# Load roadmap
with open('epic_roadmap.json') as f:
    data = json.load(f)

# Check EPIC-CCN-027
epic27 = [e for e in data if e['epic_number'] == 'EPIC-CCN-027']
print("=" * 60)
print("EPIC-CCN-027 Status:")
print("=" * 60)
if epic27:
    e = epic27[0]
    print(f"Method: {e['method']}")
    print(f"File: {e['file']}")
    print(f"Status: {e.get('status', 'pending')}")
    print(f"Completion: {e.get('completion_date', 'N/A')}")
else:
    print("NOT FOUND in roadmap")

# Check for encoding-sensitive epics (DrawingHelpers, ChartControl)
print("\n" + "=" * 60)
print("Encoding-Sensitive Epics (require local execution):")
print("=" * 60)
encoding_files = ['DrawingHelpers', 'ChartControl']
encoding_epics = [e for e in data if any(f in e.get('file', '') for f in encoding_files)]

if encoding_epics:
    for e in encoding_epics:
        print(f"\n{e['epic_number']}:")
        print(f"  Method: {e['method']}")
        print(f"  File: {e['file']}")
        print(f"  Status: {e.get('status', 'pending')}")
        print(f"  Reason: File contains non-ASCII characters")
else:
    print("None found")

# Check for any epics with special notes
print("\n" + "=" * 60)
print("Summary:")
print("=" * 60)
print(f"Total epics in roadmap: {len(data)}")
print(f"EPIC-CCN-027: {'FOUND' if epic27 else 'NOT FOUND'}")
print(f"Encoding-sensitive epics: {len(encoding_epics)}")

# Made with Bob
