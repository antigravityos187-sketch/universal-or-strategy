#!/usr/bin/env python3
"""Extract Wave 2 epic complexity values from epic_roadmap.json"""
import json
import sys

# Set UTF-8 encoding for Windows console
if sys.platform == 'win32':
    sys.stdout.reconfigure(encoding='utf-8')

with open('epic_roadmap.json', 'r') as f:
    data = json.load(f)

wave2_epics = ['EPIC-CCN-107', 'EPIC-CCN-108', 'EPIC-CCN-109', 'EPIC-CCN-110',
               'EPIC-CCN-111', 'EPIC-CCN-112', 'EPIC-CCN-113', 'EPIC-CCN-114', 'EPIC-CCN-115']

print("Wave 2 Epic Complexity Values:\n")
print("Epic ID       | Method                              | CYC | Needs Redo?")
print("--------------|-------------------------------------|-----|------------")

for epic in data:
    if epic['epic_number'] in wave2_epics:
        cyc = epic['cyclomatic']
        needs_redo = "[YES]" if cyc > 8 else "[NO]"
        print(f"{epic['epic_number']:13} | {epic['method']:35} | {cyc:3} | {needs_redo}")

print("\n" + "="*80)
print("Summary:")
epics_to_redo = [e for e in data if e['epic_number'] in wave2_epics and e['cyclomatic'] > 8]
print(f"Epics needing redo (CYC > 8): {len(epics_to_redo)}/{len(wave2_epics)}")
print("\nEpics to redo:")
for epic in epics_to_redo:
    print(f"  - {epic['epic_number']}: {epic['method']} (CYC {epic['cyclomatic']})")

# Made with Bob
