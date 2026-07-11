#!/usr/bin/env python3
"""
Pre-Flight Validation for Epic Execution
Detects special cases based on file patterns, not epic numbers.
"""

import os
import sys
import json
import chardet
from pathlib import Path
from typing import Dict, List, Optional

# File-based pattern registry
ENCODING_SENSITIVE_PATTERNS = [
    '*DrawingHelpers.cs',
    '*ChartControl.cs',
    '*Localization*.cs',
]

CRITICAL_PATH_PATTERNS = [
    '*Atm.cs',
    '*SIMA*.cs',
    '*Execution*.cs',
]

KNOWN_INVALID_METHODS = {
    'Dispatch_PublishMarketBracketToPhoton': 'src/V12_002.SIMA.Dispatch.cs',
}


def detect_encoding_issues(file_path: str) -> bool:
    """Detect if file requires local execution due to encoding."""
    if not os.path.exists(file_path):
        return False
    
    try:
        # Check for non-ASCII characters
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
            if any(ord(c) > 127 for c in content):
                return True
    except UnicodeDecodeError:
        return True
    
    # Check file encoding
    with open(file_path, 'rb') as f:
        result = chardet.detect(f.read())
        if result['encoding'] and result['encoding'].lower() not in ['utf-8', 'ascii']:
            return True
    
    return False


def detect_invalid_target(method_name: str, file_path: str) -> bool:
    """Detect if target method exists in specified file."""
    if not os.path.exists(file_path):
        return True  # File doesn't exist = invalid
    
    # Check known invalid methods
    if method_name in KNOWN_INVALID_METHODS:
        if KNOWN_INVALID_METHODS[method_name] == file_path:
            return True
    
    # Search file for method signature
    with open(file_path, 'r', encoding='utf-8', errors='ignore') as f:
        content = f.read()
    
    # Check for method declaration patterns
    patterns = [
        f"void {method_name}(",
        f"bool {method_name}(",
        f"int {method_name}(",
        f"string {method_name}(",
        f"Task {method_name}(",
        f"async Task {method_name}(",
        f"private void {method_name}(",
        f"public void {method_name}(",
        f"internal void {method_name}(",
    ]
    
    return not any(pattern in content for pattern in patterns)


def detect_test_requirements(method_name: str, file_path: str, cyc: int) -> Dict:
    """Detect if method requires extensive test generation."""
    # High complexity = more test cases
    if cyc > 30:
        return {
            "requires_extended_time": True,
            "estimated_test_cases": cyc * 2,
            "framework": "xUnit",
            "coverage_target": 90,
            "reason": f"High complexity (CYC {cyc})"
        }
    
    # Check if method is in critical path
    for pattern in CRITICAL_PATH_PATTERNS:
        if pattern.replace('*', '') in file_path:
            return {
                "requires_extended_time": True,
                "estimated_test_cases": int(cyc * 1.5),
                "framework": "xUnit",
                "coverage_target": 85,
                "reason": "Critical execution path"
            }
    
    return {
        "requires_extended_time": False,
        "estimated_test_cases": cyc,
        "framework": "xUnit",
        "coverage_target": 80,
        "reason": "Standard"
    }


def detect_already_complete(epic_id: str) -> bool:
    """Detect if epic is already complete with clean execution."""
    completion_file = f"docs/brain/{epic_id}/06-completion-report.md"
    
    if not os.path.exists(completion_file):
        return False
    
    # Check if completion was clean (no issues)
    with open(completion_file, 'r', encoding='utf-8') as f:
        content = f.read().lower()
    
    # Look for issue indicators
    issue_indicators = [
        "p0 issue",
        "p1 issue",
        "compilation error",
        "behavioral change",
        "jane street violation",
        "greptile",  # If Greptile mentioned, check for issues
    ]
    
    has_issues = any(indicator in content for indicator in issue_indicators)
    
    # If Greptile mentioned, check for "0 issues" or "clean"
    if "greptile" in content:
        return "0 issues" in content or "clean" in content
    
    return not has_issues


def preflight_validation(epic_id: str, method_name: str, file_path: str, cyc: int) -> Dict:
    """Run all special case detections before starting epic."""
    
    results = {
        "epic_id": epic_id,
        "method_name": method_name,
        "file_path": file_path,
        "cyclomatic": cyc,
        "special_cases": [],
        "routing": "normal",  # normal, local, skip
        "labels": [],
        "details": {}
    }
    
    # Check 1: Invalid target
    if detect_invalid_target(method_name, file_path):
        results["special_cases"].append("invalid-target")
        results["routing"] = "skip"
        results["labels"].append("invalid-target")
        results["details"]["invalid_reason"] = f"Method {method_name} not found in {file_path}"
        return results  # Early exit
    
    # Check 2: Already complete
    if detect_already_complete(epic_id):
        results["special_cases"].append("already-complete")
        results["routing"] = "skip"
        results["labels"].append("already-complete")
        results["details"]["completion_status"] = "Clean Phase 6 completion exists"
        return results  # Early exit
    
    # Check 3: Encoding issues
    if detect_encoding_issues(file_path):
        results["special_cases"].append("encoding-sensitive")
        results["routing"] = "local"
        results["labels"].append("encoding-sensitive")
        results["details"]["encoding_issue"] = "Non-UTF-8 encoding detected"
    
    # Check 4: Test requirements
    test_req = detect_test_requirements(method_name, file_path, cyc)
    if test_req["requires_extended_time"]:
        results["special_cases"].append("test-heavy")
        results["labels"].append("test-heavy")
        results["details"]["test_requirements"] = test_req
    
    return results


def validate_epic(epic_id: str) -> Dict:
    """Validate a single epic from roadmap."""
    # Load roadmap
    with open('epic_roadmap.json', 'r') as f:
        roadmap = json.load(f)
    
    # Find epic
    epic = next((e for e in roadmap if e['epic_number'] == epic_id), None)
    if not epic:
        return {
            "error": f"Epic {epic_id} not found in roadmap"
        }
    
    # Run validation
    return preflight_validation(
        epic_id=epic['epic_number'],
        method_name=epic['method'],
        file_path=epic['file'],
        cyc=epic['cyclomatic']
    )


def validate_all_epics() -> Dict:
    """Validate all epics in roadmap."""
    # Load roadmap
    with open('epic_roadmap.json', 'r') as f:
        roadmap = json.load(f)
    
    results = {
        "total_epics": len(roadmap),
        "normal": [],
        "local": [],
        "skip": [],
        "summary": {}
    }
    
    for epic in roadmap:
        validation = preflight_validation(
            epic_id=epic['epic_number'],
            method_name=epic['method'],
            file_path=epic['file'],
            cyc=epic['cyclomatic']
        )
        
        # Add to appropriate list
        if validation['routing'] == 'normal':
            results['normal'].append(validation)
        elif validation['routing'] == 'local':
            results['local'].append(validation)
        elif validation['routing'] == 'skip':
            results['skip'].append(validation)
    
    # Generate summary
    results['summary'] = {
        "normal_execution": len(results['normal']),
        "local_execution": len(results['local']),
        "skipped": len(results['skip']),
        "encoding_sensitive": len([e for e in results['local'] if 'encoding-sensitive' in e['labels']]),
        "invalid_target": len([e for e in results['skip'] if 'invalid-target' in e['labels']]),
        "already_complete": len([e for e in results['skip'] if 'already-complete' in e['labels']]),
        "test_heavy": len([e for e in results['normal'] + results['local'] if 'test-heavy' in e['labels']]),
    }
    
    return results


def generate_report(results: Dict) -> str:
    """Generate markdown report from validation results."""
    report = f"""# Pre-Flight Validation Report

## Summary
- **Total epics**: {results['total_epics']}
- **Normal execution**: {results['summary']['normal_execution']}
- **Local execution**: {results['summary']['local_execution']}
- **Skipped**: {results['summary']['skipped']}

## Special Cases Detected

### Local Execution Required ({results['summary']['local_execution']})
"""
    
    for epic in results['local']:
        report += f"\n- **{epic['epic_id']}**: {epic['method']} in {epic['file']}\n"
        report += f"  - Labels: {', '.join(epic['labels'])}\n"
        if 'encoding_issue' in epic['details']:
            report += f"  - Reason: {epic['details']['encoding_issue']}\n"
    
    report += f"\n### Skipped - Invalid Target ({results['summary']['invalid_target']})\n"
    
    for epic in [e for e in results['skip'] if 'invalid-target' in e['labels']]:
        report += f"\n- **{epic['epic_id']}**: {epic['method']} in {epic['file']}\n"
        report += f"  - Labels: {', '.join(epic['labels'])}\n"
        if 'invalid_reason' in epic['details']:
            report += f"  - Reason: {epic['details']['invalid_reason']}\n"
    
    report += f"\n### Skipped - Already Complete ({results['summary']['already_complete']})\n"
    
    for epic in [e for e in results['skip'] if 'already-complete' in e['labels']]:
        report += f"\n- **{epic['epic_id']}**: {epic['method']}\n"
        report += f"  - Labels: {', '.join(epic['labels'])}\n"
        if 'completion_status' in epic['details']:
            report += f"  - Reason: {epic['details']['completion_status']}\n"
    
    report += f"\n### Test-Heavy ({results['summary']['test_heavy']})\n"
    
    test_heavy = [e for e in results['normal'] + results['local'] if 'test-heavy' in e['labels']]
    for epic in test_heavy[:10]:  # Show first 10
        report += f"\n- **{epic['epic_id']}**: {epic['method']} (CYC {epic['cyclomatic']})\n"
        if 'test_requirements' in epic['details']:
            req = epic['details']['test_requirements']
            report += f"  - Estimated tests: {req['estimated_test_cases']}\n"
            report += f"  - Coverage target: {req['coverage_target']}%\n"
    
    if len(test_heavy) > 10:
        report += f"\n... and {len(test_heavy) - 10} more\n"
    
    return report


def main():
    """Main entry point."""
    import argparse
    
    parser = argparse.ArgumentParser(description='Pre-flight validation for epic execution')
    parser.add_argument('--epic', help='Validate single epic (e.g., EPIC-CCN-001)')
    parser.add_argument('--all', action='store_true', help='Validate all epics in roadmap')
    parser.add_argument('--report', help='Output report file (markdown)')
    parser.add_argument('--json', help='Output JSON file')
    
    args = parser.parse_args()
    
    if args.epic:
        # Validate single epic
        result = validate_epic(args.epic)
        print(json.dumps(result, indent=2))
        
    elif args.all:
        # Validate all epics
        results = validate_all_epics()
        
        # Generate report
        if args.report:
            report = generate_report(results)
            with open(args.report, 'w') as f:
                f.write(report)
            print(f"Report written to {args.report}")
        
        # Output JSON
        if args.json:
            with open(args.json, 'w') as f:
                json.dump(results, f, indent=2)
            print(f"JSON written to {args.json}")
        
        # Print summary
        print("\nSummary:")
        print(f"  Normal execution: {results['summary']['normal_execution']}")
        print(f"  Local execution: {results['summary']['local_execution']}")
        print(f"  Skipped: {results['summary']['skipped']}")
        
    else:
        parser.print_help()
        sys.exit(1)


if __name__ == '__main__':
    main()

# Made with Bob
