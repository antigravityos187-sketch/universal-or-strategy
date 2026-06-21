#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Validate the 180 method count from complexity audit.

This script:
1. Parses complexity_audit_fresh_2026-06-14.txt
2. Extracts all methods with CYC > 8
3. Validates the count is exactly 180
4. Generates detailed breakdown by complexity tier
5. Exports method list for Wave 7 extraction
"""

import re
import json
import sys
from collections import defaultdict
from pathlib import Path

# Fix Windows console encoding
if sys.platform == 'win32':
    try:
        sys.stdout.reconfigure(encoding='utf-8')
    except AttributeError:
        # Python < 3.7 or TextIO wrapper doesn't support reconfigure
        import codecs
        sys.stdout = codecs.getwriter('utf-8')(sys.stdout.buffer, 'strict')

def parse_complexity_audit(filepath):
    """Parse complexity audit and extract methods > 8."""
    methods = []
    
    with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
        for line in f:
            # Match format: "  - File.cs::MethodName (CYC=15, LOC=32)"
            # More flexible regex to handle variations
            match = re.search(r'-\s+([^:]+)::([^\(]+)\s*\(CYC=(\d+)', line)
            if match:
                file_name = match.group(1).strip()
                method_name = match.group(2).strip()
                cyc = int(match.group(3))
                
                if cyc > 8:
                    methods.append({
                        'file': file_name,
                        'method': method_name,
                        'cyc': cyc
                    })
    
    return methods

def validate_count(methods, expected=180):
    """Validate method count matches expected."""
    actual = len(methods)
    if actual == expected:
        print(f"[PASS] VALIDATION PASSED: {actual} methods (expected {expected})")
        return True
    else:
        print(f"[FAIL] VALIDATION FAILED: {actual} methods (expected {expected})")
        print(f"       Difference: {actual - expected:+d}")
        return False

def analyze_distribution(methods):
    """Analyze complexity distribution."""
    tiers = {
        'low': [],      # CYC 9-14
        'medium': [],   # CYC 15-19
        'high': [],     # CYC 20+
    }
    
    for method in methods:
        cyc = method['cyc']
        if cyc <= 14:
            tiers['low'].append(method)
        elif cyc <= 19:
            tiers['medium'].append(method)
        else:
            tiers['high'].append(method)
    
    return tiers

def analyze_by_file(methods):
    """Group methods by file."""
    by_file = defaultdict(list)
    for method in methods:
        by_file[method['file']].append(method)
    return dict(by_file)

def main():
    print("=" * 80)
    print("180 METHOD COUNT VALIDATION")
    print("=" * 80)
    print()
    
    # Parse complexity audit
    filepath = 'complexity_audit_fresh_2026-06-14.txt'
    print(f"Parsing: {filepath}")
    methods = parse_complexity_audit(filepath)
    print(f"Found: {len(methods)} methods with CYC > 8")
    print()
    
    # Validate count
    print("VALIDATION:")
    is_valid = validate_count(methods, expected=180)
    print()
    
    if not is_valid:
        print("⚠️  WARNING: Method count mismatch!")
        print("   Review complexity_audit_fresh_2026-06-14.txt for accuracy")
        print()
    
    # Analyze distribution
    print("COMPLEXITY DISTRIBUTION:")
    tiers = analyze_distribution(methods)
    print(f"  Low (CYC 9-14):     {len(tiers['low']):3d} methods ({len(tiers['low'])/len(methods)*100:.1f}%)")
    print(f"  Medium (CYC 15-19): {len(tiers['medium']):3d} methods ({len(tiers['medium'])/len(methods)*100:.1f}%)")
    print(f"  High (CYC 20+):     {len(tiers['high']):3d} methods ({len(tiers['high'])/len(methods)*100:.1f}%)")
    print()
    
    # Analyze by file
    print("FILE DISTRIBUTION:")
    by_file = analyze_by_file(methods)
    print(f"  Total files: {len(by_file)}")
    print(f"  Avg methods per file: {len(methods)/len(by_file):.1f}")
    print()
    
    # Top 10 files by method count
    print("TOP 10 FILES (by method count):")
    sorted_files = sorted(by_file.items(), key=lambda x: len(x[1]), reverse=True)
    for i, (file, file_methods) in enumerate(sorted_files[:10], 1):
        print(f"  {i:2d}. {file:50s} {len(file_methods):3d} methods")
    print()
    
    # Top 20 most complex methods
    print("TOP 20 MOST COMPLEX METHODS:")
    sorted_methods = sorted(methods, key=lambda x: x['cyc'], reverse=True)
    for i, method in enumerate(sorted_methods[:20], 1):
        print(f"  {i:2d}. {method['file']:30s} :: {method['method']:40s} CYC: {method['cyc']}")
    print()
    
    # Export to JSON
    output_file = 'baseline_180_methods.json'
    with open(output_file, 'w', encoding='utf-8') as f:
        json.dump({
            'total_methods': len(methods),
            'validation_passed': is_valid,
            'tiers': {
                'low': len(tiers['low']),
                'medium': len(tiers['medium']),
                'high': len(tiers['high'])
            },
            'methods': methods
        }, f, indent=2)
    print(f"[OK] Exported to: {output_file}")
    print()
    
    # Summary
    print("=" * 80)
    print("SUMMARY:")
    print("=" * 80)
    if is_valid:
        print(f"[PASS] Validation PASSED: {len(methods)} methods confirmed")
        print(f"[PASS] Ready to proceed with Wave 7 extraction")
    else:
        print(f"[FAIL] Validation FAILED: {len(methods)} methods found (expected 180)")
        print(f"[WARN] DO NOT proceed until count is corrected")
    print()

if __name__ == '__main__':
    main()

# Made with Bob
