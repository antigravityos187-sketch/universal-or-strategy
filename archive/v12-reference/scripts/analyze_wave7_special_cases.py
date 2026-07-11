#!/usr/bin/env python3
"""
Wave 7 Special Cases Analysis
Identifies epics requiring local execution or special handling
"""

import json
import sys
import io
from pathlib import Path
from typing import Dict, List, Set

# Fix Windows console encoding
if sys.platform == 'win32':
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')
    sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8', errors='replace')

def load_roadmap(path: str = "epic_roadmap_wave7.json") -> Dict:
    """Load the Wave 7 roadmap"""
    with open(path, 'r', encoding='utf-8') as f:
        return json.load(f)

def load_complexity_audit(path: str = "complexity_audit_fresh_2026-06-14.txt") -> List[str]:
    """Load complexity audit to cross-reference methods"""
    methods = []
    try:
        with open(path, 'r', encoding='utf-8') as f:
            for line in f:
                line = line.strip()
                if line and not line.startswith('#') and '::' in line:
                    # Extract method signature
                    methods.append(line)
    except FileNotFoundError:
        print(f"Warning: {path} not found", file=sys.stderr)
    return methods

def analyze_special_cases(roadmap: Dict) -> Dict[str, List[str]]:
    """Analyze epics for special case requirements"""
    
    special_cases = {
        'dll_dependencies': [],
        'utf8_violations': [],
        'test_framework_violations': [],
        'high_complexity': [],  # CYC > 20
        'very_high_complexity': [],  # CYC > 30
        'external_dependencies': [],
        'requires_local': []
    }
    
    epics = roadmap.get('epics', {})
    
    for epic_id, epic_data in epics.items():
        method = epic_data.get('method', '')
        file_path = epic_data.get('file', '')
        complexity = epic_data.get('complexity', 0)
        
        # Check for DLL dependencies (NinjaTrader-specific files)
        if 'NinjaTrader' in file_path or '.dll' in method.lower():
            special_cases['dll_dependencies'].append(epic_id)
            special_cases['requires_local'].append(epic_id)
        
        # Check for potential UTF-8 issues (files with special chars or encoding history)
        if any(char in method for char in ['©', '®', '™', '€', '£', '¥']):
            special_cases['utf8_violations'].append(epic_id)
        
        # Check complexity thresholds
        if complexity > 30:
            special_cases['very_high_complexity'].append(epic_id)
        elif complexity > 20:
            special_cases['high_complexity'].append(epic_id)
        
        # Check for external dependencies (database, network, file I/O)
        method_lower = method.lower()
        if any(keyword in method_lower for keyword in ['database', 'sql', 'connection', 'http', 'api', 'file', 'stream']):
            special_cases['external_dependencies'].append(epic_id)
    
    return special_cases

def generate_report(roadmap: Dict, special_cases: Dict[str, List[str]]) -> str:
    """Generate comprehensive special cases report"""
    
    total_epics = roadmap.get('total_epics', 0)
    
    report = []
    report.append("=" * 80)
    report.append("WAVE 7 SPECIAL CASES ANALYSIS")
    report.append("=" * 80)
    report.append("")
    report.append(f"Total Epics: {total_epics}")
    report.append(f"Source: {roadmap.get('source_file', 'N/A')}")
    report.append(f"Created: {roadmap.get('created', 'N/A')}")
    report.append("")
    
    # Summary
    report.append("SPECIAL CASES SUMMARY")
    report.append("-" * 80)
    
    total_special = len(set(epic for epics in special_cases.values() for epic in epics))
    report.append(f"Total Epics with Special Cases: {total_special}")
    report.append("")
    
    for category, epics in special_cases.items():
        if epics:
            report.append(f"  {category.replace('_', ' ').title()}: {len(epics)}")
    
    report.append("")
    
    # Detailed breakdown
    report.append("DETAILED BREAKDOWN")
    report.append("-" * 80)
    report.append("")
    
    for category, epic_ids in special_cases.items():
        if not epic_ids:
            continue
            
        report.append(f"## {category.replace('_', ' ').title()} ({len(epic_ids)} epics)")
        report.append("")
        
        if category == 'dll_dependencies':
            report.append("[!] REQUIRES LOCAL EXECUTION - Cannot build on VM without NinjaTrader DLLs")
            report.append("")
        elif category == 'utf8_violations':
            report.append("[!] UTF-8 ENCODING CHECK REQUIRED - May contain non-ASCII characters")
            report.append("")
        elif category == 'test_framework_violations':
            report.append("[!] XUNIT FRAMEWORK REQUIRED - Must use xUnit, not NUnit/MSTest")
            report.append("")
        elif category == 'very_high_complexity':
            report.append("[!] VERY HIGH COMPLEXITY (CYC > 30) - May require multiple tickets")
            report.append("")
        elif category == 'high_complexity':
            report.append("[i] HIGH COMPLEXITY (CYC > 20) - Extra scrutiny recommended")
            report.append("")
        
        for epic_id in sorted(epic_ids)[:10]:  # Show first 10
            epic_data = roadmap['epics'].get(epic_id, {})
            method = epic_data.get('method', 'N/A')
            complexity = epic_data.get('complexity', 0)
            report.append(f"  - {epic_id}: {method} (CYC: {complexity})")
        
        if len(epic_ids) > 10:
            report.append(f"  ... and {len(epic_ids) - 10} more")
        
        report.append("")
    
    # Execution recommendations
    report.append("EXECUTION RECOMMENDATIONS")
    report.append("-" * 80)
    report.append("")
    
    requires_local = special_cases['requires_local']
    if requires_local:
        report.append(f"[!] {len(requires_local)} epics MUST be executed locally:")
        report.append("   - DLL dependencies require NinjaTrader installation")
        report.append("   - Cannot build on VM without proper references")
        report.append("   - Execute these epics AFTER VM-based epics complete")
        report.append("")
    
    vm_safe = total_epics - len(requires_local)
    report.append(f"[OK] {vm_safe} epics can be executed on VM")
    report.append("")
    
    # Polling strategy
    report.append("POLLING STRATEGY")
    report.append("-" * 80)
    report.append("")
    report.append("Phase Launch (First 10 epics):")
    report.append("  - Poll every 1 minute")
    report.append("  - Verify successful launch")
    report.append("  - Check for errors in Lamport events")
    report.append("")
    report.append("Full Wave Execution (After first 10):")
    report.append("  - Poll every 4 minutes (cost-optimized)")
    report.append("  - Monitor progress via Lamport events")
    report.append("  - Apply recovery loop for failures")
    report.append("")
    
    # Critical requirements
    report.append("CRITICAL REQUIREMENTS (ALL EPICS)")
    report.append("-" * 80)
    report.append("")
    report.append("1. UTF-8 Encoding:")
    report.append("   - ALL source files MUST be UTF-8 encoded")
    report.append("   - No BOM, no ASCII-only violations")
    report.append("   - Verify before every commit")
    report.append("")
    report.append("2. xUnit Test Framework:")
    report.append("   - ALWAYS generate xUnit tests ([Fact], Assert.Equal())")
    report.append("   - NEVER use NUnit or MSTest")
    report.append("   - Violation = P0 blocker")
    report.append("")
    report.append("3. Building-Blocks Method:")
    report.append("   - ALWAYS copy scripts from previous wave")
    report.append("   - NEVER generate from scratch")
    report.append("   - Update only epic-specific parameters")
    report.append("")
    
    report.append("=" * 80)
    report.append("END OF REPORT")
    report.append("=" * 80)
    
    return "\n".join(report)

def main():
    """Main execution"""
    try:
        # Load data
        print("Loading Wave 7 roadmap...", file=sys.stderr)
        roadmap = load_roadmap()
        
        print("Analyzing special cases...", file=sys.stderr)
        special_cases = analyze_special_cases(roadmap)
        
        # Generate report
        report = generate_report(roadmap, special_cases)
        print(report)
        
        # Save to file
        output_path = Path("docs/workflow/WAVE7_SPECIAL_CASES_ANALYSIS.md")
        output_path.parent.mkdir(parents=True, exist_ok=True)
        output_path.write_text(report, encoding='utf-8')
        print(f"\nReport saved to: {output_path}", file=sys.stderr)
        
        # Return exit code based on special cases
        requires_local = len(special_cases['requires_local'])
        if requires_local > 0:
            print(f"\n[!] WARNING: {requires_local} epics require local execution", file=sys.stderr)
            return 1
        
        return 0
        
    except Exception as e:
        print(f"Error: {e}", file=sys.stderr)
        import traceback
        traceback.print_exc()
        return 1

if __name__ == '__main__':
    sys.exit(main())

# Made with Bob
