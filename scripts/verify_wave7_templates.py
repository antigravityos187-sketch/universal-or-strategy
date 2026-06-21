#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Wave 7 Template Verification Script

Verifies that all Wave 7 phase templates:
1. Use temp file + command substitution pattern (MANDATORY)
2. Have correct EPIC-W7-XXX naming convention
3. Follow Building-Blocks Method compliance

Usage:
    python scripts/verify_wave7_templates.py
    python scripts/verify_wave7_templates.py --fix  # Auto-fix EPIC naming
"""

import os
import re
import sys
from pathlib import Path
from typing import List, Tuple, Dict

# Fix Windows console encoding
if sys.platform == 'win32':
    import codecs
    sys.stdout = codecs.getwriter('utf-8')(sys.stdout.buffer, 'strict')
    sys.stderr = codecs.getwriter('utf-8')(sys.stderr.buffer, 'strict')

# ANSI color codes
GREEN = '\033[92m'
RED = '\033[91m'
YELLOW = '\033[93m'
BLUE = '\033[94m'
RESET = '\033[0m'

TEMPLATES_DIR = Path("building-blocks/wave7")
REQUIRED_TEMPLATES = [
    "phase0_template_wave7.sh",
    "phase1_template_wave7.sh",
    "phase1_5_template_wave7.sh",
    "phase2_template_wave7.sh",
    "phase3_template_wave7.sh",
    "phase4_template_wave7.sh",
    "phase5_template_wave7.sh",
    "phase5_v_template_wave7.sh",
    "phase6_template_wave7.sh",
]

# Patterns to check
TEMP_FILE_PATTERN = re.compile(r'cat\s+>\s+/tmp/[\w$_]+\.txt\s+<<\s+[\'"]?EOF\w*[\'"]?')
COMMAND_SUBSTITUTION_PATTERN = re.compile(r'\$\(cat\s+/tmp/[\w$_]+\.txt\)')
INLINE_BOB_PATTERN = re.compile(r'bob\s+--yolo\s+--chat-mode\s+\w+\s+"[^"]*"')
OLD_EPIC_PATTERN = re.compile(r'EPIC-CCN-\d+')
NEW_EPIC_PATTERN = re.compile(r'EPIC-W7-\d+')

def check_file_exists() -> Tuple[bool, List[str]]:
    """Check if all required template files exist."""
    missing = []
    for template in REQUIRED_TEMPLATES:
        if not (TEMPLATES_DIR / template).exists():
            missing.append(template)
    return len(missing) == 0, missing

def check_temp_file_pattern(content: str, filename: str) -> Tuple[bool, List[str]]:
    """Check if template uses temp file + command substitution pattern."""
    issues = []
    
    # Check for temp file creation
    has_temp_file = bool(TEMP_FILE_PATTERN.search(content))
    if not has_temp_file:
        issues.append(f"Missing temp file creation pattern (cat > /tmp/...)")
    
    # Check for command substitution
    has_cmd_sub = bool(COMMAND_SUBSTITUTION_PATTERN.search(content))
    if not has_cmd_sub:
        issues.append(f"Missing command substitution pattern ($(cat /tmp/...))")
    
    # Check for BANNED inline pattern
    has_inline = bool(INLINE_BOB_PATTERN.search(content))
    if has_inline:
        issues.append(f"❌ CRITICAL: Uses BANNED inline bob pattern (causes freeze)")
    
    return len(issues) == 0, issues

def check_epic_naming(content: str, filename: str) -> Tuple[bool, List[str], int, int]:
    """Check EPIC naming convention."""
    issues = []
    
    old_epics = OLD_EPIC_PATTERN.findall(content)
    new_epics = NEW_EPIC_PATTERN.findall(content)
    
    if old_epics:
        issues.append(f"Found {len(old_epics)} old EPIC-CCN-XXX references (should be EPIC-W7-XXX)")
    
    return len(old_epics) == 0, issues, len(old_epics), len(new_epics)

def fix_epic_naming(content: str) -> str:
    """Replace EPIC-CCN-XXX with EPIC-W7-XXX."""
    # Replace EPIC-CCN-001 -> EPIC-W7-001, etc.
    def replace_epic(match):
        old_epic = match.group(0)
        epic_num = old_epic.split('-')[-1]
        return f"EPIC-W7-{epic_num}"
    
    return OLD_EPIC_PATTERN.sub(replace_epic, content)

def verify_template(template_path: Path, fix_naming: bool = False) -> Dict:
    """Verify a single template file."""
    result = {
        'filename': template_path.name,
        'exists': template_path.exists(),
        'temp_file_ok': False,
        'epic_naming_ok': False,
        'issues': [],
        'old_epic_count': 0,
        'new_epic_count': 0,
        'fixed': False
    }
    
    if not result['exists']:
        result['issues'].append("File does not exist")
        return result
    
    content = template_path.read_text(encoding='utf-8')
    
    # Check temp file pattern
    temp_ok, temp_issues = check_temp_file_pattern(content, template_path.name)
    result['temp_file_ok'] = temp_ok
    result['issues'].extend(temp_issues)
    
    # Check EPIC naming
    epic_ok, epic_issues, old_count, new_count = check_epic_naming(content, template_path.name)
    result['epic_naming_ok'] = epic_ok
    result['issues'].extend(epic_issues)
    result['old_epic_count'] = old_count
    result['new_epic_count'] = new_count
    
    # Fix naming if requested
    if fix_naming and not epic_ok:
        fixed_content = fix_epic_naming(content)
        template_path.write_text(fixed_content, encoding='utf-8')
        result['fixed'] = True
        result['epic_naming_ok'] = True
        result['issues'] = [i for i in result['issues'] if 'EPIC-CCN' not in i]
        result['issues'].append(f"✅ Fixed: Replaced {old_count} EPIC-CCN references with EPIC-W7")
    
    return result

def print_summary(results: List[Dict]):
    """Print verification summary."""
    print(f"\n{BLUE}{'='*80}{RESET}")
    print(f"{BLUE}Wave 7 Template Verification Summary{RESET}")
    print(f"{BLUE}{'='*80}{RESET}\n")
    
    total = len(results)
    passed = sum(1 for r in results if r['temp_file_ok'] and r['epic_naming_ok'])
    temp_file_ok = sum(1 for r in results if r['temp_file_ok'])
    epic_naming_ok = sum(1 for r in results if r['epic_naming_ok'])
    fixed = sum(1 for r in results if r['fixed'])
    
    print(f"Total Templates: {total}")
    print(f"Temp File Pattern: {GREEN if temp_file_ok == total else RED}{temp_file_ok}/{total}{RESET}")
    print(f"EPIC Naming: {GREEN if epic_naming_ok == total else RED}{epic_naming_ok}/{total}{RESET}")
    if fixed > 0:
        print(f"Auto-Fixed: {GREEN}{fixed}{RESET}")
    print(f"\nOverall: {GREEN if passed == total else RED}{passed}/{total} PASSED{RESET}\n")
    
    # Detailed results
    for result in results:
        status = f"{GREEN}✓{RESET}" if result['temp_file_ok'] and result['epic_naming_ok'] else f"{RED}✗{RESET}"
        print(f"{status} {result['filename']}")
        
        if result['issues']:
            for issue in result['issues']:
                if '✅' in issue:
                    print(f"  {GREEN}{issue}{RESET}")
                elif '❌' in issue:
                    print(f"  {RED}{issue}{RESET}")
                else:
                    print(f"  {YELLOW}- {issue}{RESET}")
        
        if result['old_epic_count'] > 0 or result['new_epic_count'] > 0:
            print(f"  {BLUE}EPIC References: {result['old_epic_count']} old, {result['new_epic_count']} new{RESET}")
        print()

def main():
    """Main verification routine."""
    fix_naming = '--fix' in sys.argv
    
    print(f"\n{BLUE}Wave 7 Template Verification{RESET}")
    print(f"Directory: {TEMPLATES_DIR}")
    print(f"Mode: {'FIX' if fix_naming else 'CHECK'}\n")
    
    # Check if all files exist
    all_exist, missing = check_file_exists()
    if not all_exist:
        print(f"{RED}ERROR: Missing template files:{RESET}")
        for f in missing:
            print(f"  - {f}")
        return 1
    
    # Verify each template
    results = []
    for template in REQUIRED_TEMPLATES:
        template_path = TEMPLATES_DIR / template
        result = verify_template(template_path, fix_naming)
        results.append(result)
    
    # Print summary
    print_summary(results)
    
    # Exit code
    all_passed = all(r['temp_file_ok'] and r['epic_naming_ok'] for r in results)
    if all_passed:
        print(f"{GREEN}✅ All templates verified successfully!{RESET}\n")
        return 0
    else:
        if not fix_naming:
            print(f"{YELLOW}⚠️  Issues found. Run with --fix to auto-correct EPIC naming.{RESET}\n")
        return 1

if __name__ == '__main__':
    sys.exit(main())

# Made with Bob
