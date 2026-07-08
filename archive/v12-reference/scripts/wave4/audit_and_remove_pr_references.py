#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Audit and remove PR references from Wave 4 autonomous workflow.

The autonomous workflow should NOT create PRs - it only commits to gitbutler/workspace.
PR creation is a separate manual step after wave completion.
"""

import re
import sys
from pathlib import Path
from collections import defaultdict

# Fix Windows console encoding
if sys.platform == 'win32':
    sys.stdout.reconfigure(encoding='utf-8')

# Patterns to find (case-insensitive)
PR_PATTERNS = [
    r'\bPR\b',
    r'\bpull.?request\b',
    r'create.*pr\b',
    r'submit.*pr\b',
    r'pr.*hygiene',
    r'pr.*diff',
    r'github.*pr'
]

# Acceptable contexts (don't flag these)
ACCEPTABLE_CONTEXTS = [
    'CreateFollowerTargetRequest',  # Method name
    'private',  # Keyword
    'proprietary',  # Word
    'approve',  # Word
    'expression',  # Word
    'compress',  # Word
    'represent',  # Word
]

def is_acceptable_context(line):
    """Check if line contains acceptable PR context."""
    line_lower = line.lower()
    for context in ACCEPTABLE_CONTEXTS:
        if context.lower() in line_lower:
            return True
    return False

def find_pr_references(file_path):
    """Find all PR references in a file."""
    matches = []
    
    try:
        with open(file_path, 'r', encoding='utf-8', errors='ignore') as f:
            lines = f.readlines()
            
        for i, line in enumerate(lines, 1):
            # Skip acceptable contexts
            if is_acceptable_context(line):
                continue
            
            # Check each pattern
            for pattern in PR_PATTERNS:
                if re.search(pattern, line, re.IGNORECASE):
                    matches.append({
                        'line_num': i,
                        'line': line.strip(),
                        'pattern': pattern
                    })
                    break  # Only count once per line
    
    except Exception as e:
        print(f"Error reading {file_path}: {e}")
    
    return matches

def audit_directory(directory):
    """Audit all markdown files in directory."""
    results = defaultdict(list)
    
    for md_file in Path(directory).rglob('*.md'):
        matches = find_pr_references(md_file)
        if matches:
            results[str(md_file)] = matches
    
    return results

def generate_fixes(results):
    """Generate recommended fixes for PR references."""
    fixes = []
    
    for file_path, matches in results.items():
        file_fixes = {
            'file': file_path,
            'matches': len(matches),
            'recommendations': []
        }
        
        # Analyze patterns
        has_pr_hygiene = any('hygiene' in m['line'].lower() for m in matches)
        has_pr_audit = any('audit' in m['line'].lower() for m in matches)
        has_pr_diff = any('diff' in m['line'].lower() for m in matches)
        
        if has_pr_hygiene:
            file_fixes['recommendations'].append(
                "Replace 'PR Hygiene' with 'Code Quality Checks' or remove section"
            )
        
        if has_pr_audit:
            file_fixes['recommendations'].append(
                "Replace 'DNA & PR Audit' with 'DNA Audit' (Phase 3 name)"
            )
        
        if has_pr_diff:
            file_fixes['recommendations'].append(
                "Remove 'PR diff <10,000' requirement (not applicable to autonomous workflow)"
            )
        
        if not file_fixes['recommendations']:
            file_fixes['recommendations'].append(
                "Review and remove PR references - autonomous workflow doesn't create PRs"
            )
        
        fixes.append(file_fixes)
    
    return fixes

def main():
    print("=== PR REFERENCE AUDIT ===\n")
    print("Scanning Wave 4 epic files for PR references...\n")
    
    # Audit docs/brain/EPIC-CCN-0* directories
    results = audit_directory('docs/brain')
    
    # Filter to Wave 4 epics only (EPIC-CCN-001 through EPIC-CCN-080)
    wave4_results = {
        k: v for k, v in results.items()
        if any(f'EPIC-CCN-{i:03d}' in k for i in range(1, 81))
    }
    
    if not wave4_results:
        print("[OK] No PR references found in Wave 4 epics!")
        return
    
    print(f"[X] Found PR references in {len(wave4_results)} files\n")
    
    # Generate fixes
    fixes = generate_fixes(wave4_results)
    
    # Summary by phase
    phase_counts = defaultdict(int)
    for file_path in wave4_results.keys():
        if '00-hotspots.md' in file_path:
            phase_counts['Phase 0'] += 1
        elif '01-scope' in file_path:
            phase_counts['Phase 1'] += 1
        elif '02-architecture' in file_path:
            phase_counts['Phase 2'] += 1
        elif '03-audit' in file_path:
            phase_counts['Phase 3'] += 1
        elif '04-tickets' in file_path:
            phase_counts['Phase 4'] += 1
        elif '05-' in file_path or 'ticket-' in file_path:
            phase_counts['Phase 5'] += 1
        elif '06-' in file_path:
            phase_counts['Phase 6'] += 1
    
    print("=== SUMMARY BY PHASE ===\n")
    for phase, count in sorted(phase_counts.items()):
        print(f"{phase}: {count} files")
    
    print("\n=== DETAILED FINDINGS ===\n")
    for fix in fixes[:10]:  # Show first 10
        print(f"File: {fix['file']}")
        print(f"Matches: {fix['matches']}")
        print("Recommendations:")
        for rec in fix['recommendations']:
            print(f"  - {rec}")
        print()
    
    if len(fixes) > 10:
        print(f"... and {len(fixes) - 10} more files\n")
    
    # Critical findings
    print("\n=== CRITICAL ISSUES ===\n")
    
    phase3_files = [f for f in wave4_results.keys() if '03-audit' in f]
    if phase3_files:
        print(f"[!] Phase 3 files still reference 'PR Audit': {len(phase3_files)} files")
        print("   Action: Rename to 'DNA Audit' in Phase 3 MCP tool and templates")
    
    phase4_files = [f for f in wave4_results.keys() if '04-tickets' in f]
    if phase4_files:
        print(f"[!] Phase 4 files mention PR requirements: {len(phase4_files)} files")
        print("   Action: Remove PR hygiene checks from ticket templates")
    
    # Recommendations
    print("\n=== RECOMMENDED ACTIONS ===\n")
    print("1. Update Phase 3 MCP tool:")
    print("   - Rename 'DNA & PR Audit' to 'DNA Audit'")
    print("   - Remove PR hygiene checks")
    print("   - File: scripts/phase_3_audit_mcp.py")
    
    print("\n2. Update Phase 4 MCP tool:")
    print("   - Remove 'PR diff <10,000' requirement")
    print("   - Remove PR hygiene section from tickets")
    print("   - File: scripts/phase_4_tickets_mcp.py")
    
    print("\n3. Update building-blocks templates:")
    print("   - Remove PR references from all phase templates")
    print("   - Update SOP to clarify: autonomous = commits only, no PRs")
    
    print("\n4. Re-run affected phases:")
    print("   - Phases 3-6 for all 80 epics (if fixing templates)")
    print("   - OR: Accept current files and fix in next wave")
    
    print("\n5. Update documentation:")
    print("   - WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md")
    print("   - V12_EPIC_WORKFLOW_10_PHASE_SOP.md")
    print("   - autonomous-refactor mode description")
    
    # Save detailed report
    report_path = Path('scripts/wave4/pr_reference_audit_report.txt')
    with open(report_path, 'w', encoding='utf-8') as f:
        f.write("=== PR REFERENCE AUDIT REPORT ===\n\n")
        f.write(f"Total files with PR references: {len(wave4_results)}\n\n")
        
        for file_path, matches in wave4_results.items():
            f.write(f"\n{file_path}\n")
            f.write("=" * len(file_path) + "\n")
            for match in matches:
                f.write(f"Line {match['line_num']}: {match['line']}\n")
    
    print(f"\nDetailed report saved to: {report_path}")

if __name__ == '__main__':
    main()

# Made with Bob
