#!/usr/bin/env python3
"""Generate fresh epic roadmap from complexity audit."""

import re
import json

# Read audit file
with open('complexity_audit_fresh_2026-06-14.txt', 'r') as f:
    lines = f.readlines()

# Extract methods needing refactoring
epics = []
current_file = None
epic_num = 1

for line in lines:
    # Track current file
    if line.startswith('=== FILE:'):
        current_file = line.split('FILE:')[1].strip().replace('===', '').strip()
        continue
    
    # Find REFACTOR lines
    if 'REFACTOR' in line and '|' in line:
        parts = [p.strip() for p in line.split('|')]
        if len(parts) >= 5:
            method = parts[1]
            loc = parts[2]
            cyc = parts[3]
            
            # Skip header rows
            if method == 'Method' or not cyc.isdigit():
                continue
            
            epics.append({
                'epic_number': f'EPIC-CCN-{epic_num:03d}',
                'method': method,
                'file': current_file,
                'cyclomatic': int(cyc),
                'loc': int(loc),
                'status': 'pending'
            })
            epic_num += 1

print(f"Total methods needing refactoring (CYC >8): {len(epics)}")
print(f"\nTop 80 epics:")
for i, epic in enumerate(epics[:80], 1):
    print(f"{i:3d}. {epic['epic_number']} | {epic['method']:50s} | CYC={epic['cyclomatic']:2d} | {epic['file']}")

# Save to JSON
output_file = 'epic_roadmap_fresh_2026-06-14.json'
with open(output_file, 'w') as f:
    json.dump(epics, f, indent=2)

print(f"\n✅ Saved {len(epics)} epics to {output_file}")
print(f"\nEpic number range: {epics[0]['epic_number']} to {epics[-1]['epic_number']}")

# Made with Bob
