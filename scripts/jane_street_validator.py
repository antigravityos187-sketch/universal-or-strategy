#!/usr/bin/env python3
"""
Jane Street Pattern Validator
Automated anti-pattern detector for V12 Universal OR Strategy

Detects violations of Jane Street-aligned patterns:
- P0 (CRITICAL): Blocking violations (lock usage, nullable without checks, etc.)
- P1 (HIGH): Warning violations (magic numbers, exceptions for control flow, etc.)
- P2 (MEDIUM): Info violations (missing docs, TODO comments, etc.)

Usage:
    python scripts/jane_street_validator.py src/
    python scripts/jane_street_validator.py src/ --json
    python scripts/jane_street_validator.py src/ --fix-suggestions

Exit Codes:
    0 = No P0 violations (P1/P2 may exist)
    1 = P0 violations detected (blocking)
"""

import re
import sys
import json
import argparse
from pathlib import Path
from typing import List, Dict, Any, Tuple
from dataclasses import dataclass, asdict


@dataclass
class Violation:
    """Represents a single pattern violation"""
    severity: str  # P0, P1, P2
    rule: str
    file: str
    line: int
    column: int
    message: str
    code_snippet: str
    fix_suggestion: str


class JaneStreetValidator:
    """Validates C# code against Jane Street patterns"""
    
    def __init__(self, root_path: Path):
        self.root_path = root_path
        self.violations: List[Violation] = []
        
        # P0 patterns (CRITICAL - Blocking)
        self.p0_patterns = {
            'LOCK_USAGE': {
                'pattern': r'\block\s*\(',
                'message': 'lock() usage detected - use Actor/FSM pattern',
                'fix': 'Replace with Channel<T> for producer/consumer or Interlocked for atomic ops'
            },
            'NULLABLE_WITHOUT_CHECK': {
                'pattern': r'(\w+)\?\s+(\w+)\s*=.*?;\s*(?!.*\2\s*==\s*null)(?!.*\2\s*!=\s*null)(?!.*\2\?\.).*?\2\.',
                'message': 'Nullable reference used without null check',
                'fix': 'Use Option<T> pattern or add null check before dereference'
            },
            'MUTABLE_SHARED_STATE': {
                'pattern': r'(public|internal)\s+static\s+(?!readonly)(?!const)\w+',
                'message': 'Mutable shared state detected',
                'fix': 'Use readonly/const or Actor pattern for shared state'
            },
            'UNICODE_IN_STRING': {
                'pattern': r'[^\x00-\x7F]',
                'message': 'Non-ASCII character detected in string literal',
                'fix': 'Replace with ASCII equivalent or use escape sequence'
            }
        }
        
        # P1 patterns (HIGH - Warning)
        self.p1_patterns = {
            'MAGIC_NUMBER': {
                'pattern': r'(?<![a-zA-Z_])[0-9]{2,}(?![a-zA-Z_])',
                'message': 'Magic number detected',
                'fix': 'Extract to const: private const int MAX_RETRIES = 3;',
                'exclude_patterns': [r'\.cs:\d+:', r'Version\(', r'TimeSpan\.From']
            },
            'EXCEPTION_CONTROL_FLOW': {
                'pattern': r'throw\s+new\s+\w+Exception\([^)]*\)\s*;(?!.*catch)',
                'message': 'Exception used for control flow',
                'fix': 'Use Result<T,E> pattern instead of exceptions'
            },
            'NESTED_LOOPS_DEEP': {
                'pattern': r'for\s*\([^)]*\)\s*\{[^}]*for\s*\([^)]*\)\s*\{[^}]*for\s*\(',
                'message': 'Nested loops >2 levels detected',
                'fix': 'Extract inner loops to separate methods'
            },
            'LONG_METHOD': {
                'pattern': None,  # Handled by line counting
                'message': 'Method exceeds 50 lines',
                'fix': 'Split into smaller, focused methods'
            }
        }
        
        # P2 patterns (MEDIUM - Info)
        self.p2_patterns = {
            'MISSING_XML_DOCS': {
                'pattern': r'(public|protected)\s+(class|interface|enum|struct|delegate)\s+\w+(?!\s*\{[^}]*///)',
                'message': 'Missing XML documentation on public API',
                'fix': 'Add /// <summary> documentation'
            },
            'TODO_COMMENT': {
                'pattern': r'//\s*TODO:',
                'message': 'TODO comment detected (technical debt)',
                'fix': 'Create issue or remove comment'
            },
            'COMMENTED_CODE': {
                'pattern': r'//\s*(public|private|protected|internal|class|void|int|string|bool)',
                'message': 'Commented-out code detected',
                'fix': 'Remove or explain why commented'
            }
        }
    
    def validate_file(self, file_path: Path) -> None:
        """Validate a single C# file"""
        try:
            content = file_path.read_text(encoding='utf-8')
            lines = content.split('\n')
            
            # Check P0 patterns
            for rule, config in self.p0_patterns.items():
                if rule == 'UNICODE_IN_STRING':
                    self._check_unicode(file_path, lines, config)
                else:
                    self._check_pattern(file_path, lines, config, rule, 'P0')
            
            # Check P1 patterns
            for rule, config in self.p1_patterns.items():
                if rule == 'LONG_METHOD':
                    self._check_long_methods(file_path, lines, config)
                else:
                    self._check_pattern(file_path, lines, config, rule, 'P1')
            
            # Check P2 patterns
            for rule, config in self.p2_patterns.items():
                self._check_pattern(file_path, lines, config, rule, 'P2')
                
        except Exception as e:
            print(f"Error processing {file_path}: {e}", file=sys.stderr)
    
    def _check_pattern(self, file_path: Path, lines: List[str], 
                      config: Dict[str, Any], rule: str, severity: str) -> None:
        """Check for a regex pattern in file"""
        if not config['pattern']:
            return
            
        pattern = re.compile(config['pattern'])
        exclude_patterns = [re.compile(p) for p in config.get('exclude_patterns', [])]
        
        for line_num, line in enumerate(lines, 1):
            # Skip excluded patterns
            if any(ep.search(line) for ep in exclude_patterns):
                continue
                
            match = pattern.search(line)
            if match:
                # Get context (3 lines before and after)
                start = max(0, line_num - 4)
                end = min(len(lines), line_num + 3)
                snippet = '\n'.join(f"{i+1:4d} | {lines[i]}" for i in range(start, end))
                
                self.violations.append(Violation(
                    severity=severity,
                    rule=rule,
                    file=str(file_path.relative_to(self.root_path)),
                    line=line_num,
                    column=match.start() + 1,
                    message=config['message'],
                    code_snippet=snippet,
                    fix_suggestion=config['fix']
                ))
    
    def _check_unicode(self, file_path: Path, lines: List[str], 
                      config: Dict[str, Any]) -> None:
        """Check for non-ASCII characters in string literals"""
        pattern = re.compile(r'"([^"]*)"')
        unicode_pattern = re.compile(config['pattern'])
        
        for line_num, line in enumerate(lines, 1):
            # Find all string literals
            for match in pattern.finditer(line):
                string_content = match.group(1)
                if unicode_pattern.search(string_content):
                    # Get context
                    start = max(0, line_num - 4)
                    end = min(len(lines), line_num + 3)
                    snippet = '\n'.join(f"{i+1:4d} | {lines[i]}" for i in range(start, end))
                    
                    self.violations.append(Violation(
                        severity='P0',
                        rule='UNICODE_IN_STRING',
                        file=str(file_path.relative_to(self.root_path)),
                        line=line_num,
                        column=match.start() + 1,
                        message=config['message'],
                        code_snippet=snippet,
                        fix_suggestion=config['fix']
                    ))
    
    def _check_long_methods(self, file_path: Path, lines: List[str], 
                           config: Dict[str, Any]) -> None:
        """Check for methods exceeding 50 lines"""
        method_pattern = re.compile(r'^\s*(public|private|protected|internal).*\s+\w+\s*\([^)]*\)\s*$')
        brace_pattern = re.compile(r'\{|\}')
        
        in_method = False
        method_start = 0
        brace_count = 0
        
        for line_num, line in enumerate(lines, 1):
            if method_pattern.match(line):
                in_method = True
                method_start = line_num
                brace_count = 0
            
            if in_method:
                for char in line:
                    if char == '{':
                        brace_count += 1
                    elif char == '}':
                        brace_count -= 1
                        if brace_count == 0:
                            method_length = line_num - method_start + 1
                            if method_length > 50:
                                snippet = f"Method at line {method_start} is {method_length} lines"
                                self.violations.append(Violation(
                                    severity='P1',
                                    rule='LONG_METHOD',
                                    file=str(file_path.relative_to(self.root_path)),
                                    line=method_start,
                                    column=1,
                                    message=config['message'],
                                    code_snippet=snippet,
                                    fix_suggestion=config['fix']
                                ))
                            in_method = False
    
    def validate_directory(self) -> None:
        """Validate all C# files in directory"""
        cs_files = list(self.root_path.rglob('*.cs'))
        
        for cs_file in cs_files:
            # Skip test files and generated files
            if any(x in str(cs_file) for x in ['Test', 'Generated', '.g.cs', 'AssemblyInfo']):
                continue
            self.validate_file(cs_file)
    
    def get_summary(self) -> Dict[str, int]:
        """Get violation count summary"""
        summary = {'P0': 0, 'P1': 0, 'P2': 0}
        for v in self.violations:
            summary[v.severity] += 1
        return summary
    
    def print_report(self, show_fix_suggestions: bool = False) -> None:
        """Print human-readable report"""
        summary = self.get_summary()
        
        print("\n" + "="*80)
        print("JANE STREET PATTERN VALIDATION REPORT")
        print("="*80)
        
        if not self.violations:
            print("\n✅ No violations detected!")
            return
        
        # Group by severity
        for severity in ['P0', 'P1', 'P2']:
            severity_violations = [v for v in self.violations if v.severity == severity]
            if not severity_violations:
                continue
            
            severity_label = {
                'P0': 'CRITICAL (Blocking)',
                'P1': 'HIGH (Warning)',
                'P2': 'MEDIUM (Info)'
            }[severity]
            
            print(f"\n{severity} - {severity_label}: {len(severity_violations)} violations")
            print("-" * 80)
            
            for v in severity_violations:
                print(f"\n[{v.rule}] {v.file}:{v.line}:{v.column}")
                print(f"  {v.message}")
                if show_fix_suggestions:
                    print(f"  Fix: {v.fix_suggestion}")
                print(f"\n{v.code_snippet}")
        
        print("\n" + "="*80)
        print(f"SUMMARY: P0={summary['P0']}, P1={summary['P1']}, P2={summary['P2']}")
        print("="*80)
    
    def export_json(self) -> str:
        """Export violations as JSON"""
        return json.dumps({
            'violations': [asdict(v) for v in self.violations],
            'summary': self.get_summary()
        }, indent=2)


def main():
    parser = argparse.ArgumentParser(
        description='Jane Street Pattern Validator for V12 Universal OR Strategy'
    )
    parser.add_argument('path', type=Path, help='Path to source directory')
    parser.add_argument('--json', action='store_true', help='Output JSON format')
    parser.add_argument('--fix-suggestions', action='store_true', 
                       help='Show fix suggestions in report')
    
    args = parser.parse_args()
    
    if not args.path.exists():
        print(f"Error: Path {args.path} does not exist", file=sys.stderr)
        sys.exit(1)
    
    validator = JaneStreetValidator(args.path)
    validator.validate_directory()
    
    if args.json:
        print(validator.export_json())
    else:
        validator.print_report(args.fix_suggestions)
    
    # Exit with error if P0 violations found
    summary = validator.get_summary()
    if summary['P0'] > 0:
        sys.exit(1)
    else:
        sys.exit(0)


if __name__ == '__main__':
    main()

# Made with Bob
