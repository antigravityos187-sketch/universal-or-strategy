#!/usr/bin/env python3
"""Generate fresh epic roadmap from complexity audit using two-tier system."""

import re
import json

# Read audit file
with open('complexity_audit_fresh_2026-06-14.txt', 'r', encoding='utf-8', errors='ignore') as f:
    lines = f.readlines()

# Extract methods needing refactoring
methods = []
current_file = None

for line in lines:
    # Track current file
    if line.startswith('=== FILE:'):
        current_file = line.split('FILE:')[1].strip().replace('===', '').strip()
        continue
    
    # Find REFACTOR lines (table format with pipes)
    if '|' in line and 'REFACTOR' in line:
        parts = [p.strip() for p in line.split('|')]
        
        # Skip header rows and malformed lines
        if len(parts) < 5:
            continue
        if parts[1] == 'Method' or parts[1] == '':
            continue
        
        method = parts[1]
        loc_str = parts[2]
        cyc_str = parts[3]
        
        # Validate numeric fields
        if not loc_str.isdigit() or not cyc_str.isdigit():
            continue
        
        loc = int(loc_str)
        cyc = int(cyc_str)
        
        methods.append({
            'method': method,
            'file': current_file,
            'cyclomatic': cyc,
            'loc': loc
        })

print(f"Total methods needing refactoring (CYC >8): {len(methods)}")

# Sort by cyclomatic complexity (descending)
methods.sort(key=lambda x: x['cyclomatic'], reverse=True)

# Two-tier selection
tier1 = [m for m in methods if m['cyclomatic'] >= 15]  # High complexity
tier2 = [m for m in methods if 9 <= m['cyclomatic'] <= 14]  # Medium complexity

print(f"\nTier 1 (CYC 15-30): {len(tier1)} methods")
print(f"Tier 2 (CYC 9-14): {len(tier2)} methods")

# Select top 80 (40 from each tier, or adjust if one tier is smaller)
tier1_count = min(40, len(tier1))
tier2_count = min(40, len(tier2))

# If tier1 < 40, take more from tier2
if tier1_count < 40:
    tier2_count = min(80 - tier1_count, len(tier2))
# If tier2 < 40, take more from tier1
elif tier2_count < 40:
    tier1_count = min(80 - tier2_count, len(tier1))

selected = tier1[:tier1_count] + tier2[:tier2_count]

print(f"\nSelected: {tier1_count} from Tier 1 + {tier2_count} from Tier 2 = {len(selected)} epics")

# Create epic roadmap
epics = []
for i, method in enumerate(selected, 1):
    tier = 1 if method['cyclomatic'] >= 15 else 2
    epics.append({
        'epic_number': f'EPIC-CCN-{i:03d}',
        'method': method['method'],
        'file': method['file'],
        'cyclomatic': method['cyclomatic'],
        'loc': method['loc'],
        'tier': tier,
        'status': 'pending'
    })

# Display top 20
print(f"\nTop 20 epics:")
for epic in epics[:20]:
    tier_label = f"T{epic['tier']}"
    print(f"{epic['epic_number']} | {tier_label} | CYC={epic['cyclomatic']:2d} | {epic['method']:50s} | {epic['file']}")

# Save to JSON
output_file = 'epic_roadmap_wave4_fresh.json'
with open(output_file, 'w') as f:
    json.dump(epics, f, indent=2)

print(f"\nSaved {len(epics)} epics to {output_file}")
print(f"Epic range: {epics[0]['epic_number']} to {epics[-1]['epic_number']}")
print(f"\nTier breakdown:")
print(f"  Tier 1 (CYC 15-30): {sum(1 for e in epics if e['tier'] == 1)} epics")
print(f"  Tier 2 (CYC 9-14): {sum(1 for e in epics if e['tier'] == 2)} epics")

# Made with Bob
