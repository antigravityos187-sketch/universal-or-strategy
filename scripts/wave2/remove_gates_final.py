#!/usr/bin/env python3
"""Remove Director approval gates from autonomous refactor commands."""

import re
from pathlib import Path

# Commands to fix
COMMANDS = [
    "epic-scope-boundary",
    "epic-plan", 
    "epic-scan",
    "epic-tickets",
    "epic-validate",
    "epic-verify-ticket",
    "epic-review-final"
]

def remove_gate_section(content: str, command_name: str) -> str:
    """Remove the gate section from command content."""
    
    # Pattern 1: Remove entire gate section (## !! GATE !! ... Output: ...)
    # Match from "## !! " to the final "Output:" line
    pattern1 = r'---\s*\n\s*## !! .*?GATE !!.*?Output:.*?\n'
    content = re.sub(pattern1, '---\n\n## PHASE COMPLETE\n\nPhase artifacts written and manifest updated.\n\n**Next Phase**: See manifest for next available phase\n**Review Artifacts**: Check `docs/brain/EPIC-*/` directory\n**Check Status**: `python scripts/epic_manifest.py status EPIC-*`\n\n', content, flags=re.DOTALL)
    
    # Pattern 2: Fix role description if it still has "STOP for Director approval"
    content = re.sub(
        r'> You produce .* then STOP for Director approval\.',
        '> You produce planning artifacts then complete the phase.',
        content
    )
    
    # Pattern 3: Remove "Scope changes require Director approval"
    content = re.sub(r'- Scope changes require Director approval\n', '', content)
    
    return content

def main():
    """Process all commands."""
    base_path = Path('.bob/commands')
    fixed_count = 0
    
    print("=== Removing Director Gates (Python) ===\n")
    
    for cmd in COMMANDS:
        file_path = base_path / f"{cmd}.md"
        
        if not file_path.exists():
            print(f"SKIP: {cmd}.md not found")
            continue
            
        print(f"Processing: {cmd}.md")
        
        # Read content
        content = file_path.read_text(encoding='utf-8')
        original = content
        
        # Remove gates
        content = remove_gate_section(content, cmd)
        
        # Write if changed
        if content != original:
            file_path.write_text(content, encoding='utf-8')
            print(f"  [OK] Gates removed")
            fixed_count += 1
        else:
            print(f"  [INFO] No changes needed")
    
    print(f"\n=== Summary ===")
    print(f"Commands fixed: {fixed_count}/{len(COMMANDS)}")
    print(f"\nNext: Review changes with 'git diff .bob/commands/'")

if __name__ == '__main__':
    main()

# Made with Bob
