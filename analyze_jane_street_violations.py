#!/usr/bin/env python3
"""Analyze Jane Street P0 violations from JSON file."""

import json
from collections import Counter

# Read the violations file
with open('jane_street_p0_violations.json', 'r', encoding='utf-8-sig') as f:
    data = json.load(f)

# Print summary
print("=" * 80)
print("JANE STREET VIOLATIONS SUMMARY")
print("=" * 80)
print(f"\nTotal violations: {data['summary']['total']}")
print(f"  P0 (Critical): {data['summary']['P0']}")
print(f"  P1 (High): {data['summary']['P1']}")
print(f"  P2 (Medium): {data['summary']['P2']}")

print("\n" + "=" * 80)
print("BY CATEGORY")
print("=" * 80)
for category, count in data['by_category'].items():
    print(f"  {category}: {count}")

# Analyze violations by file
print("\n" + "=" * 80)
print("TOP 20 FILES WITH MOST VIOLATIONS")
print("=" * 80)
file_counts = Counter()
for v in data['violations']:
    file_counts[v['file']] += 1

for file, count in file_counts.most_common(20):
    print(f"  {file}: {count} violations")

# Analyze violations by rule
print("\n" + "=" * 80)
print("TOP 20 MOST COMMON VIOLATIONS")
print("=" * 80)
rule_counts = Counter()
for v in data['violations']:
    rule_counts[v['rule_id']] += 1

for rule, count in rule_counts.most_common(20):
    print(f"  {rule}: {count} violations")

# Check overlap with 180 complexity methods
print("\n" + "=" * 80)
print("CHECKING OVERLAP WITH 180 COMPLEXITY METHODS")
print("=" * 80)

# Load baseline methods
try:
    with open('baseline_180_methods.json', 'r', encoding='utf-8') as f:
        baseline = json.load(f)
    
    # Extract file paths from baseline
    baseline_files = set()
    for method in baseline:
        baseline_files.add(method['file'])
    
    # Extract file paths from violations
    violation_files = set(v['file'] for v in data['violations'])
    
    # Calculate overlap
    overlap = baseline_files & violation_files
    
    print(f"\nBaseline complexity files: {len(baseline_files)}")
    print(f"Files with Jane Street violations: {len(violation_files)}")
    print(f"Files with BOTH complexity AND violations: {len(overlap)}")
    print(f"Overlap percentage: {len(overlap) / len(baseline_files) * 100:.1f}%")
    
    if overlap:
        print(f"\nFiles with both issues (showing first 10):")
        for i, file in enumerate(sorted(overlap)[:10], 1):
            v_count = sum(1 for v in data['violations'] if v['file'] == file)
            m_count = sum(1 for m in baseline if m['file'] == file)
            print(f"  {i}. {file}")
            print(f"     - {m_count} methods with CYC > 8")
            print(f"     - {v_count} Jane Street violations")
    
except FileNotFoundError:
    print("baseline_180_methods.json not found - skipping overlap analysis")

print("\n" + "=" * 80)

# Made with Bob
