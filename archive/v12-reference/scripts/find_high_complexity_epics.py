#!/usr/bin/env python3
"""Find high-complexity epics (CYC > 15) for Wave 2."""
import json

with open('epic_roadmap.json') as f:
    data = json.load(f)

# Filter for high complexity (CYC > 15) and not complete
high_complexity = [
    e for e in data 
    if e.get('cyclomatic', 0) > 15 
    and e.get('status') != 'complete'
]

# Sort by complexity (highest first)
high_complexity.sort(key=lambda x: x.get('cyclomatic', 0), reverse=True)

print(f"High-complexity epics (CYC > 15): {len(high_complexity)}\n")
print("Top 20 by complexity:\n")
for i, e in enumerate(high_complexity[:20], 1):
    print(f"{i:2}. {e['epic_number']}: {e['method']:<40} CYC={e['cyclomatic']:2} | {e['file']}")

print(f"\n\nRecommended Wave 2 (top 10):")
for i, e in enumerate(high_complexity[:10], 1):
    print(f"{i}. {e['epic_number']}: {e['method']} (CYC={e['cyclomatic']})")

# Made with Bob
