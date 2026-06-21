#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Complete Wave 6/7/8 Cross-Reference Analysis
Maps: Baseline 180 methods → Wave 6 epics → Wave 7 methods → Jane Street violations
"""

import json
import re
import sys
from pathlib import Path
from collections import defaultdict

# Force UTF-8 encoding for Windows console
if sys.platform == 'win32':
    import io
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')
    sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8', errors='replace')

def extract_baseline_methods():
    """Extract all 180 methods with CYC > 8 from baseline audit"""
    baseline_file = Path("complexity_audit_fresh_2026-06-14.txt")
    methods = []
    
    # Try different encodings
    for encoding in ['utf-8', 'utf-16', 'latin-1', 'cp1252']:
        try:
            with open(baseline_file, 'r', encoding=encoding) as f:
                for line in f:
                    # Match pattern: "  - V12_002.File.cs::MethodName (CYC=15, LOC=32)"
                    match = re.match(r'\s+-\s+(.+?)::(.+?)\s+\(CYC=(\d+)', line)
                    if match:
                        file_path, method_name, cyc = match.groups()
                        cyc_int = int(cyc)
                        if cyc_int > 8:
                            methods.append({
                                'file': file_path,
                                'method': method_name,
                                'cyc': cyc_int,
                                'full_name': f"{file_path}::{method_name}"
                            })
            break  # Success, exit encoding loop
        except UnicodeDecodeError:
            continue
    
    print(f"✅ Extracted {len(methods)} methods with CYC > 8 from baseline")
    return methods

def analyze_wave6_epics():
    """Analyze Wave 6 epics (EPIC-CCN-001 through 080) for Phase 0/1 completion"""
    brain_dir = Path("docs/brain")
    epics = []
    
    for i in range(1, 81):
        epic_id = f"EPIC-CCN-{i:03d}"
        epic_dir = brain_dir / epic_id
        
        hotspot_file = epic_dir / "00-hotspots.md"
        scope_file = epic_dir / "00-scope.md"
        
        has_phase0 = hotspot_file.exists()
        has_phase1 = scope_file.exists()
        
        method_name = "N/A"
        cyc = 0
        file_path = "N/A"
        
        if has_phase0:
            content = hotspot_file.read_text(encoding='utf-8')
            
            # Extract method name
            method_match = re.search(r'##\s+Method:\s+`([^`]+)`', content)
            if method_match:
                method_name = method_match.group(1)
            
            # Extract CYC
            cyc_match = re.search(r'Cyclomatic Complexity:\s+\*\*(\d+)\*\*', content)
            if cyc_match:
                cyc = int(cyc_match.group(1))
            
            # Extract file path
            file_match = re.search(r'File:\s+`([^`]+)`', content)
            if file_match:
                file_path = file_match.group(1)
        
        status = "READY" if (has_phase0 and has_phase1) else \
                 "PHASE1_PENDING" if has_phase0 else \
                 "MISSING"
        
        epics.append({
            'epic_id': epic_id,
            'method': method_name,
            'file': file_path,
            'cyc': cyc,
            'phase0': has_phase0,
            'phase1': has_phase1,
            'status': status
        })
    
    phase0_count = sum(1 for e in epics if e['phase0'])
    phase1_count = sum(1 for e in epics if e['phase1'])
    ready_count = sum(1 for e in epics if e['status'] == 'READY')
    
    print(f"✅ Wave 6 Analysis:")
    print(f"   - Phase 0 complete: {phase0_count}/80")
    print(f"   - Phase 1 complete: {phase1_count}/80")
    print(f"   - Both complete (READY): {ready_count}/80")
    
    return epics

def map_baseline_to_wave6(baseline_methods, wave6_epics):
    """Map baseline methods to Wave 6 epics"""
    mapped = []
    unmapped = []
    
    # Create lookup by method name
    wave6_lookup = {e['method']: e for e in wave6_epics if e['method'] != 'N/A'}
    
    for method in baseline_methods:
        method_name = method['method']
        
        if method_name in wave6_lookup:
            epic = wave6_lookup[method_name]
            mapped.append({
                **method,
                'epic_id': epic['epic_id'],
                'wave6_status': epic['status'],
                'wave': 'Wave 6'
            })
        else:
            unmapped.append({
                **method,
                'epic_id': None,
                'wave6_status': None,
                'wave': 'Wave 7'
            })
    
    print(f"✅ Baseline → Wave 6 Mapping:")
    print(f"   - Mapped to Wave 6: {len(mapped)}")
    print(f"   - Unmapped (Wave 7): {len(unmapped)}")
    
    return mapped, unmapped

def analyze_jane_street_violations():
    """Analyze Jane Street violations from P0 file"""
    violations_file = Path("jane_street_p0_violations.json")
    
    if not violations_file.exists():
        print("[WARN] Jane Street violations file not found")
        return []
    
    # Try different encodings
    for encoding in ['utf-8', 'utf-16', 'latin-1', 'cp1252']:
        try:
            with open(violations_file, 'r', encoding=encoding) as f:
                data = json.load(f)
            break
        except (UnicodeDecodeError, json.JSONDecodeError):
            continue
    else:
        print("[WARN] Could not decode Jane Street violations file")
        return []
    
    violations = data.get('violations', [])
    print(f"[OK] Loaded {len(violations)} Jane Street P0 violations")
    
    return violations

def cross_reference_jane_street(baseline_methods, violations):
    """Cross-reference Jane Street violations with Wave 8 methods"""
    # Create lookup by file
    method_files = {m['file'] for m in baseline_methods}
    
    in_wave8 = []
    not_in_wave8 = []
    
    for violation in violations:
        file_path = violation.get('file', '')
        
        # Normalize file path for comparison
        file_normalized = file_path.replace('\\', '/').replace('src/', '')
        
        is_in_wave8 = any(file_normalized in mf or mf in file_normalized 
                          for mf in method_files)
        
        if is_in_wave8:
            in_wave8.append(violation)
        else:
            not_in_wave8.append(violation)
    
    print(f"✅ Jane Street → Wave 8 Cross-Reference:")
    print(f"   - Violations in Wave 8 files: {len(in_wave8)}")
    print(f"   - Violations NOT in Wave 8 files: {len(not_in_wave8)}")
    
    return in_wave8, not_in_wave8

def generate_report(baseline_methods, wave6_epics, wave6_mapped, wave7_methods, 
                   js_in_wave8, js_not_in_wave8):
    """Generate comprehensive cross-reference report"""
    
    report = {
        'summary': {
            'baseline_methods': len(baseline_methods),
            'wave6_epics': len(wave6_epics),
            'wave6_phase0_complete': sum(1 for e in wave6_epics if e['phase0']),
            'wave6_phase1_complete': sum(1 for e in wave6_epics if e['phase1']),
            'wave6_ready': sum(1 for e in wave6_epics if e['status'] == 'READY'),
            'wave6_mapped_methods': len(wave6_mapped),
            'wave7_methods': len(wave7_methods),
            'jane_street_total': len(js_in_wave8) + len(js_not_in_wave8),
            'jane_street_in_wave8': len(js_in_wave8),
            'jane_street_not_in_wave8': len(js_not_in_wave8)
        },
        'wave6_epics': wave6_epics,
        'wave6_mapped_methods': wave6_mapped,
        'wave7_methods': wave7_methods,
        'jane_street_in_wave8': js_in_wave8,
        'jane_street_not_in_wave8': js_not_in_wave8
    }
    
    # Export to JSON
    output_file = Path("complete_wave_cross_reference.json")
    with open(output_file, 'w', encoding='utf-8') as f:
        json.dump(report, f, indent=2)
    
    print(f"\n✅ Exported: {output_file}")
    
    # Generate markdown summary
    generate_markdown_summary(report)
    
    return report

def generate_markdown_summary(report):
    """Generate human-readable markdown summary"""
    
    md = f"""# Complete Wave 6/7/8 Cross-Reference

**Generated**: 2026-06-18

---

## Executive Summary

### Baseline (CYC > 8)
- **Total Methods**: {report['summary']['baseline_methods']}

### Wave 6 (EPIC-CCN-001 through 080)
- **Total Epics**: {report['summary']['wave6_epics']}
- **Phase 0 Complete**: {report['summary']['wave6_phase0_complete']}/80
- **Phase 1 Complete**: {report['summary']['wave6_phase1_complete']}/80
- **Both Complete (READY)**: {report['summary']['wave6_ready']}/80
- **Mapped Methods**: {report['summary']['wave6_mapped_methods']}

### Wave 7 (Remaining Methods)
- **Total Methods**: {report['summary']['wave7_methods']}
- **Status**: Ready for Phase 0 generation

### Wave 8 (Wave 6 + Wave 7)
- **Total Methods**: {report['summary']['wave6_mapped_methods'] + report['summary']['wave7_methods']}
- **Validation**: {'✅ PASS' if report['summary']['wave6_mapped_methods'] + report['summary']['wave7_methods'] == report['summary']['baseline_methods'] else '❌ FAIL'}

### Jane Street Violations
- **Total P0 Violations**: {report['summary']['jane_street_total']}
- **In Wave 8 Files**: {report['summary']['jane_street_in_wave8']}
- **NOT in Wave 8 Files**: {report['summary']['jane_street_not_in_wave8']}

---

## Wave 6 Ready Epics (Phase 0 AND Phase 1 Complete)

"""
    
    ready_epics = [e for e in report['wave6_epics'] if e['status'] == 'READY']
    
    if ready_epics:
        md += "| Epic ID | Method | File | CYC | Status |\n"
        md += "|---------|--------|------|-----|--------|\n"
        for epic in ready_epics:
            md += f"| {epic['epic_id']} | {epic['method']} | {epic['file']} | {epic['cyc']} | ✅ READY |\n"
    else:
        md += "*No epics have both Phase 0 and Phase 1 complete.*\n"
    
    md += f"\n---\n\n## Wave 6 Pending Epics (Phase 0 Complete, Phase 1 Pending)\n\n"
    
    pending_epics = [e for e in report['wave6_epics'] if e['status'] == 'PHASE1_PENDING']
    
    if pending_epics:
        md += f"**Count**: {len(pending_epics)} epics\n\n"
        md += "| Epic ID | Method | File | CYC |\n"
        md += "|---------|--------|------|-----|\n"
        for epic in pending_epics[:10]:  # Show first 10
            md += f"| {epic['epic_id']} | {epic['method']} | {epic['file']} | {epic['cyc']} |\n"
        
        if len(pending_epics) > 10:
            md += f"\n*... and {len(pending_epics) - 10} more*\n"
    
    md += f"\n---\n\n## Wave 7 Methods (Not in Wave 6)\n\n"
    md += f"**Count**: {report['summary']['wave7_methods']} methods\n\n"
    
    if report['wave7_methods']:
        md += "| Method | File | CYC |\n"
        md += "|--------|------|-----|\n"
        for method in report['wave7_methods'][:10]:  # Show first 10
            md += f"| {method['method']} | {method['file']} | {method['cyc']} |\n"
        
        if len(report['wave7_methods']) > 10:
            md += f"\n*... and {len(report['wave7_methods']) - 10} more*\n"
    
    md += f"\n---\n\n## Jane Street Integration Plan\n\n"
    md += f"### Violations in Wave 8 Files ({report['summary']['jane_street_in_wave8']})\n\n"
    md += "These violations are in files being refactored by Wave 8 and should be addressed during refactoring.\n\n"
    
    md += f"### Violations NOT in Wave 8 Files ({report['summary']['jane_street_not_in_wave8']})\n\n"
    md += "These violations are in files NOT being refactored by Wave 8 and require separate epics.\n\n"
    
    md += "---\n\n## Next Steps\n\n"
    md += "1. Complete Wave 6 Phase 1 for remaining epics\n"
    md += "2. Generate Wave 7 epic structure\n"
    md += "3. Execute Wave 7 Phase 0\n"
    md += "4. Integrate Jane Street violations into Wave 8 execution\n"
    md += "5. Create separate epics for Jane Street violations NOT in Wave 8\n"
    
    output_file = Path("docs/brain/COMPLETE_WAVE_CROSS_REFERENCE.md")
    output_file.write_text(md, encoding='utf-8')
    
    print(f"✅ Exported: {output_file}")

def main():
    print("=== COMPLETE WAVE 6/7/8 CROSS-REFERENCE ANALYSIS ===\n")
    
    # Step 1: Extract baseline methods
    print("Step 1: Extracting baseline methods...")
    baseline_methods = extract_baseline_methods()
    
    # Step 2: Analyze Wave 6 epics
    print("\nStep 2: Analyzing Wave 6 epics...")
    wave6_epics = analyze_wave6_epics()
    
    # Step 3: Map baseline to Wave 6
    print("\nStep 3: Mapping baseline to Wave 6...")
    wave6_mapped, wave7_methods = map_baseline_to_wave6(baseline_methods, wave6_epics)
    
    # Step 4: Analyze Jane Street violations
    print("\nStep 4: Analyzing Jane Street violations...")
    js_violations = analyze_jane_street_violations()
    
    # Step 5: Cross-reference Jane Street with Wave 8
    print("\nStep 5: Cross-referencing Jane Street with Wave 8...")
    js_in_wave8, js_not_in_wave8 = cross_reference_jane_street(baseline_methods, js_violations)
    
    # Step 6: Generate report
    print("\nStep 6: Generating comprehensive report...")
    report = generate_report(baseline_methods, wave6_epics, wave6_mapped, wave7_methods,
                            js_in_wave8, js_not_in_wave8)
    
    print("\n=== ANALYSIS COMPLETE ===")
    print(f"\nSummary:")
    print(f"  Baseline: {report['summary']['baseline_methods']} methods")
    print(f"  Wave 6: {report['summary']['wave6_mapped_methods']} methods")
    print(f"  Wave 7: {report['summary']['wave7_methods']} methods")
    print(f"  Wave 8: {report['summary']['wave6_mapped_methods'] + report['summary']['wave7_methods']} methods")
    print(f"  Jane Street in Wave 8: {report['summary']['jane_street_in_wave8']}")
    print(f"  Jane Street NOT in Wave 8: {report['summary']['jane_street_not_in_wave8']}")

if __name__ == "__main__":
    main()

# Made with Bob

