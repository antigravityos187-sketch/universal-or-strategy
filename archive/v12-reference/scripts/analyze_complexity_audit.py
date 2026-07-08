#!/usr/bin/env python3
"""
Analyze complexity_audit_fresh_2026-06-14.txt to determine Wave 6 scope.

Counts:
1. Total methods with CYC > 8 (Jane Street threshold)
2. Methods by complexity tier
3. Comparison with Wave 6 actual scope (79 methods)
"""

import re
from collections import defaultdict

def analyze_complexity_audit(filepath):
    """Parse complexity audit and extract methods > 8."""
    methods_above_8 = []
    complexity_distribution = defaultdict(int)
    
    with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
        for line in f:
            # Match list format: "  - File.cs::MethodName (CYC=15, LOC=32)"
            match = re.search(r'-\s+(.+?)::(.+?)\s+\(CYC=(\d+)', line)
            if match:
                file_name = match.group(1).strip()
                method_name = match.group(2).strip()
                cyc = int(match.group(3))
                
                # Track all methods
                complexity_distribution[cyc] += 1
                
                # Track methods > 8
                if cyc > 8:
                    methods_above_8.append({
                        'method': method_name,
                        'file': file_name,
                        'cyc': cyc
                    })
    
    return methods_above_8, complexity_distribution

def main():
    filepath = 'complexity_audit_fresh_2026-06-14.txt'
    
    print("=" * 80)
    print("WAVE 6 SCOPE ANALYSIS")
    print("=" * 80)
    print()
    
    methods_above_8, distribution = analyze_complexity_audit(filepath)
    
    # Sort by complexity (highest first)
    methods_above_8.sort(key=lambda x: x['cyc'], reverse=True)
    
    print(f"Total methods with CYC > 8: {len(methods_above_8)}")
    print()
    
    # Complexity tiers
    tier_high = [m for m in methods_above_8 if m['cyc'] >= 20]
    tier_medium = [m for m in methods_above_8 if 15 <= m['cyc'] < 20]
    tier_low = [m for m in methods_above_8 if 9 <= m['cyc'] < 15]
    
    print("COMPLEXITY TIERS:")
    print(f"  High (CYC >= 20):    {len(tier_high)} methods")
    print(f"  Medium (15-19):      {len(tier_medium)} methods")
    print(f"  Low (9-14):          {len(tier_low)} methods")
    print()
    
    # Wave 6 comparison
    wave6_scope = 79  # From validation script
    print("WAVE 6 COMPARISON:")
    print(f"  Wave 6 Actual Scope: {wave6_scope} methods")
    print(f"  Baseline Audit:      {len(methods_above_8)} methods")
    print(f"  Coverage:            {wave6_scope / len(methods_above_8) * 100:.1f}%")
    print(f"  Missing:             {len(methods_above_8) - wave6_scope} methods")
    print()
    
    # Top 20 most complex
    print("TOP 20 MOST COMPLEX METHODS:")
    for i, method in enumerate(methods_above_8[:20], 1):
        print(f"  {i:2d}. {method['method']:50s} CYC: {method['cyc']}")
    print()
    
    # Distribution summary
    print("COMPLEXITY DISTRIBUTION (CYC > 8):")
    for cyc in sorted([k for k in distribution.keys() if k > 8], reverse=True):
        count = distribution[cyc]
        bar = '#' * min(count, 50)
        print(f"  CYC {cyc:2d}: {count:3d} methods {bar}")
    print()
    
    # Verdict
    print("=" * 80)
    print("VERDICT:")
    print("=" * 80)
    if wave6_scope < len(methods_above_8):
        print(f"Wave 6 is INCOMPLETE. Missing {len(methods_above_8) - wave6_scope} methods.")
        print("Recommendation: Expand Wave 6 scope or create Wave 7 for remaining methods.")
    else:
        print("Wave 6 scope matches baseline audit.")
    print()

if __name__ == '__main__':
    main()

# Made with Bob
