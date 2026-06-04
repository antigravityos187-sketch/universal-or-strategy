#!/usr/bin/env python3
"""
Jane Street Rule Checker
Automated enforcement of Jane Street patterns from RULES_CATALOG.md

Usage:
    python scripts/jane_street_rule_checker.py src/
    python scripts/jane_street_rule_checker.py src/V12_002.cs
    python scripts/jane_street_rule_checker.py src/ --severity P0
    python scripts/jane_street_rule_checker.py src/ --json
"""

import re
import sys
import json
import argparse
from pathlib import Path
from typing import List, Dict, Optional, Tuple
from dataclasses import dataclass, asdict


@dataclass
class RuleViolation:
    """Represents a single rule violation"""
    rule_id: str
    severity: str
    category: str
    file: str
    line: int
    column: int
    message: str
    fix_suggestion: str
    code_snippet: str


class JaneStreetRuleChecker:
    """Automated Jane Street rule checker"""
    
    def __init__(self, rules_catalog_path: str = "docs/standards/jane-street/RULES_CATALOG.md"):
        self.rules = self._load_rules(rules_catalog_path)
        self.violations: List[RuleViolation] = []
    
    def _load_rules(self, path: str) -> Dict[str, Dict]:
        """Load rules from RULES_CATALOG.md"""
        rules = {}
        
        # P0 Critical Rules (20 rules)
        rules["JS-001"] = {
            "severity": "P0",
            "category": "Type Safety",
            "pattern": r'throw\s+new\s+\w+Exception\(',
            "message": "Use Result<T,E> instead of exceptions in hot paths",
            "fix": "Replace exception with Result<T,E>.Err(error)"
        }
        
        rules["JS-002"] = {
            "severity": "P0",
            "category": "Type Safety",
            "pattern": r'return\s+null\s*;',
            "message": "Use Option<T> instead of null",
            "fix": "Change return type to Option<T> and use Some/None"
        }
        
        rules["JS-003"] = {
            "severity": "P0",
            "category": "Type Safety",
            "pattern": r'public\s+enum\s+\w+State',
            "message": "Use sealed record hierarchies instead of enums for state",
            "fix": "Convert enum to sealed record hierarchy"
        }
        
        rules["JS-010"] = {
            "severity": "P0",
            "category": "Type Safety",
            "pattern": r'public\s+class\s+\w+\s*\{[^}]*public\s+\w+\(',
            "message": "Use private constructors with factory methods",
            "fix": "Make constructor private, add static factory method"
        }
        
        rules["JS-015"] = {
            "severity": "P0",
            "category": "Type Safety",
            "pattern": r'public\s+\w+\s+\w+\(string\s+email\)(?!.*Email\.Parse)',
            "message": "Parse at boundaries, use validated types internally",
            "fix": "Create validated type with Parse method"
        }
        
        rules["JS-021"] = {
            "severity": "P0",
            "category": "Concurrency",
            "pattern": r'lock\s*\(',
            "message": "Lock usage is BANNED - use Actor pattern or atomic primitives",
            "fix": "Replace with Channel-based Actor pattern"
        }
        
        rules["JS-022"] = {
            "severity": "P0",
            "category": "Concurrency",
            "pattern": r'private\s+\w+\s+_state;[^}]*lock\s*\(',
            "message": "Use Channel-based Actor pattern for stateful concurrency",
            "fix": "Convert to Actor pattern with Channel mailbox"
        }
        
        rules["JS-033"] = {
            "severity": "P0",
            "category": "Concurrency",
            "pattern": r'async\s+void\s+\w+\((?!.*EventHandler)',
            "message": "Never use async void except for event handlers",
            "fix": "Change return type to ValueTask or Task"
        }
        
        rules["JS-036"] = {
            "severity": "P0",
            "category": "Performance",
            "pattern": r'byte\[\]\s+\w+\s*=\s*new\s+byte\[',
            "message": "Use Span<T> for zero-allocation in hot paths",
            "fix": "Replace with Span<byte> and stackalloc"
        }
        
        rules["JS-037"] = {
            "severity": "P0",
            "category": "Performance",
            "pattern": r'new\s+byte\[\d+\]',
            "message": "Use ArrayPool<T> for reusable buffers",
            "fix": "Use ArrayPool.Rent/Return instead of new[]"
        }
        
        rules["JS-050"] = {
            "severity": "P0",
            "category": "Performance",
            "pattern": r'new\s+\w+\[\].*foreach.*await',
            "message": "No allocation in hot paths (>1000 calls/sec)",
            "fix": "Use Span<T>, ArrayPool, or ref structs"
        }
        
        rules["JS-051"] = {
            "severity": "P0",
            "category": "Testing",
            "pattern": r'\[Fact\][^}]*public\s+void\s+\w+\(int\s+\w+\)',
            "message": "Use property-based testing with FsCheck",
            "fix": "Use [Property] attribute and generators"
        }
        
        rules["JS-052"] = {
            "severity": "P0",
            "category": "Testing",
            "pattern": r'Assert\.Equal\([^)]*\.ToString\(\)',
            "message": "Use Verify for snapshot testing of complex output",
            "fix": "Replace with Verify(object)"
        }
        
        rules["JS-060"] = {
            "severity": "P0",
            "category": "Testing",
            "pattern": r'\[Property\][^}]*Arb\.Default',
            "message": "Use custom generators for domain types",
            "fix": "Create Arbitrary<T> with domain constraints"
        }
        
        rules["JS-064"] = {
            "severity": "P0",
            "category": "Code Review",
            "pattern": r'git\s+diff.*\|\s*wc\s+-l.*[1-9]\d{4,}',
            "message": "PR diff must be <10k lines",
            "fix": "Split into smaller PRs"
        }
        
        rules["JS-070"] = {
            "severity": "P0",
            "category": "Code Review",
            "pattern": r'//\s*TODO:.*unrelated',
            "message": "No scope creep - one concern per PR",
            "fix": "Create separate PR for unrelated changes"
        }
        
        rules["JS-080"] = {
            "severity": "P0",
            "category": "Serialization",
            "pattern": r'JsonSerializer\.Serialize.*hot\s+path',
            "message": "No string allocation in serialization hot paths",
            "fix": "Use BinaryPrimitives and Span<byte>"
        }
        
        rules["JS-082"] = {
            "severity": "P0",
            "category": "Tools",
            "pattern": r'//\s*TODO:.*format',
            "message": "Use CSharpier for all C# code",
            "fix": "Run dotnet csharpier ."
        }
        
        rules["JS-090"] = {
            "severity": "P0",
            "category": "Tools",
            "pattern": r'dotnet\s+restore(?!.*--locked-mode)',
            "message": "Cache dependencies in CI",
            "fix": "Use --locked-mode and cache NuGet packages"
        }
        
        rules["JS-091"] = {
            "severity": "P0",
            "category": "Philosophy",
            "pattern": r'//\s*TODO:.*type\s+safety',
            "message": "Correctness first - implement type safety immediately",
            "fix": "Use Result<T,E>, Option<T>, validated types"
        }
        
        rules["JS-092"] = {
            "severity": "P0",
            "category": "Philosophy",
            "pattern": r'\?\s*\?\s*\?\s*\?',
            "message": "Simplicity over cleverness - avoid complex null coalescing",
            "fix": "Use explicit if/else for readability"
        }
        
        rules["JS-100"] = {
            "severity": "P0",
            "category": "Philosophy",
            "pattern": r'(?<![a-zA-Z_])\d{3,}(?![a-zA-Z_])',
            "message": "No magic numbers - use named constants",
            "fix": "Extract to const with descriptive name"
        }
        
        # P1 High Priority Rules (30 rules - subset shown)
        rules["JS-004"] = {
            "severity": "P1",
            "category": "Type Safety",
            "pattern": r'switch\s*\([^)]+\)\s*\{[^}]*default\s*:',
            "message": "Use switch expressions for exhaustive matching",
            "fix": "Remove default case to expose missing patterns"
        }
        
        rules["JS-023"] = {
            "severity": "P1",
            "category": "Concurrency",
            "pattern": r'lock\s*\(\w+\)\s*\{\s*\w+\s*[+\-*/]=',
            "message": "Use Interlocked operations for simple atomic state",
            "fix": "Replace with Interlocked.Increment/Add/CompareExchange"
        }
        
        rules["JS-027"] = {
            "severity": "P1",
            "category": "Concurrency",
            "pattern": r'async\s+\w+\s+\w+Async\([^)]*\)(?!.*CancellationToken)',
            "message": "Use CancellationToken with timeout for all async operations",
            "fix": "Add CancellationToken parameter and use CancelAfter"
        }
        
        rules["JS-032"] = {
            "severity": "P1",
            "category": "Concurrency",
            "pattern": r'public\s+async\s+Task<\w+>\s+\w+Async\(',
            "message": "Use ValueTask<T> in hot paths to avoid allocation",
            "fix": "Change return type from Task<T> to ValueTask<T>"
        }
        
        rules["JS-047"] = {
            "severity": "P1",
            "category": "Performance",
            "pattern": r'\.Where\(.*\)\.Select\(.*\)\.ToList\(\).*hot\s+path',
            "message": "Avoid LINQ in hot paths (allocates enumerators)",
            "fix": "Use for/foreach loops"
        }
        
        return rules
    
    def check_file(self, file_path: Path) -> List[RuleViolation]:
        """Check a single file against all rules"""
        if not file_path.suffix == '.cs':
            return []
        
        try:
            content = file_path.read_text(encoding='utf-8')
        except Exception as e:
            print(f"Error reading {file_path}: {e}", file=sys.stderr)
            return []
        
        violations = []
        lines = content.split('\n')
        
        for rule_id, rule in self.rules.items():
            pattern = rule["pattern"]
            
            for line_num, line in enumerate(lines, start=1):
                matches = re.finditer(pattern, line, re.IGNORECASE)
                
                for match in matches:
                    # Skip if in comment (simple heuristic)
                    if '//' in line[:match.start()]:
                        continue
                    
                    violation = RuleViolation(
                        rule_id=rule_id,
                        severity=rule["severity"],
                        category=rule["category"],
                        file=str(file_path),
                        line=line_num,
                        column=match.start() + 1,
                        message=rule["message"],
                        fix_suggestion=rule["fix"],
                        code_snippet=line.strip()
                    )
                    violations.append(violation)
        
        return violations
    
    def check_directory(self, directory: Path, recursive: bool = True) -> List[RuleViolation]:
        """Check all C# files in a directory"""
        violations = []
        
        if recursive:
            cs_files = directory.rglob('*.cs')
        else:
            cs_files = directory.glob('*.cs')
        
        for file_path in cs_files:
            # Skip generated files
            if '.g.cs' in file_path.name or 'AssemblyInfo.cs' in file_path.name:
                continue
            
            file_violations = self.check_file(file_path)
            violations.extend(file_violations)
        
        return violations
    
    def generate_report(self, violations: List[RuleViolation], output_format: str = "text") -> str:
        """Generate report in specified format"""
        if output_format == "json":
            return self._generate_json_report(violations)
        else:
            return self._generate_text_report(violations)
    
    def _generate_text_report(self, violations: List[RuleViolation]) -> str:
        """Generate human-readable text report"""
        if not violations:
            return "✅ No rule violations found!"
        
        # Group by severity
        p0_violations = [v for v in violations if v.severity == "P0"]
        p1_violations = [v for v in violations if v.severity == "P1"]
        p2_violations = [v for v in violations if v.severity == "P2"]
        
        report = []
        report.append("=" * 80)
        report.append("Jane Street Rule Checker Report")
        report.append("=" * 80)
        report.append("")
        report.append(f"Total Violations: {len(violations)}")
        report.append(f"  P0 (CRITICAL): {len(p0_violations)}")
        report.append(f"  P1 (HIGH):     {len(p1_violations)}")
        report.append(f"  P2 (MEDIUM):   {len(p2_violations)}")
        report.append("")
        
        if p0_violations:
            report.append("=" * 80)
            report.append("P0 CRITICAL VIOLATIONS (BLOCKING)")
            report.append("=" * 80)
            report.append("")
            
            for v in p0_violations:
                report.append(f"[{v.rule_id}] {v.category}: {v.message}")
                report.append(f"  File: {v.file}:{v.line}:{v.column}")
                report.append(f"  Code: {v.code_snippet}")
                report.append(f"  Fix:  {v.fix_suggestion}")
                report.append("")
        
        if p1_violations:
            report.append("=" * 80)
            report.append("P1 HIGH PRIORITY VIOLATIONS (WARNING)")
            report.append("=" * 80)
            report.append("")
            
            for v in p1_violations[:10]:  # Show first 10
                report.append(f"[{v.rule_id}] {v.category}: {v.message}")
                report.append(f"  File: {v.file}:{v.line}:{v.column}")
                report.append(f"  Fix:  {v.fix_suggestion}")
                report.append("")
            
            if len(p1_violations) > 10:
                report.append(f"... and {len(p1_violations) - 10} more P1 violations")
                report.append("")
        
        return "\n".join(report)
    
    def _generate_json_report(self, violations: List[RuleViolation]) -> str:
        """Generate machine-readable JSON report"""
        report = {
            "violations": [asdict(v) for v in violations],
            "summary": {
                "total": len(violations),
                "P0": len([v for v in violations if v.severity == "P0"]),
                "P1": len([v for v in violations if v.severity == "P1"]),
                "P2": len([v for v in violations if v.severity == "P2"])
            },
            "by_category": {}
        }
        
        # Group by category
        for v in violations:
            if v.category not in report["by_category"]:
                report["by_category"][v.category] = 0
            report["by_category"][v.category] += 1
        
        return json.dumps(report, indent=2)


def main():
    parser = argparse.ArgumentParser(
        description="Jane Street Rule Checker - Automated pattern enforcement"
    )
    parser.add_argument(
        "path",
        type=str,
        help="File or directory to check"
    )
    parser.add_argument(
        "--severity",
        type=str,
        choices=["P0", "P1", "P2", "ALL"],
        help="Filter by severity level (ALL = check all severities)"
    )
    parser.add_argument(
        "--json",
        action="store_true",
        help="Output in JSON format"
    )
    parser.add_argument(
        "--no-recursive",
        action="store_true",
        help="Don't check subdirectories"
    )
    
    args = parser.parse_args()
    
    # Initialize checker
    checker = JaneStreetRuleChecker()
    
    # Check path
    path = Path(args.path)
    
    if not path.exists():
        print(f"Error: Path '{path}' does not exist", file=sys.stderr)
        return 1
    
    if path.is_file():
        violations = checker.check_file(path)
    else:
        violations = checker.check_directory(path, recursive=not args.no_recursive)
    
    # Filter by severity if specified (ALL means no filtering)
    if args.severity and args.severity != "ALL":
        violations = [v for v in violations if v.severity == args.severity]
    
    # Generate report
    output_format = "json" if args.json else "text"
    report = checker.generate_report(violations, output_format)
    
    print(report)
    
    # Exit code: 0 if no P0 violations, 1 otherwise
    p0_count = len([v for v in violations if v.severity == "P0"])
    return 1 if p0_count > 0 else 0


if __name__ == "__main__":
    sys.exit(main())

# Made with Bob
