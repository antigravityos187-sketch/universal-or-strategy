#!/usr/bin/env python3
"""
Fix Phase 6 prerequisite check V3 - Use find command for robust pattern matching.
Issue: bash glob patterns with -e don't work, ls with multiple patterns fails on ANY miss.
Solution: Use find command which properly handles OR logic and glob patterns.
"""

import re
from pathlib import Path

scripts_dir = Path('/home/malhitticrypto/universal-or-strategy/scripts/wave4')
scripts = sorted(scripts_dir.glob('_p6_*.sh'))

print(f"Found {len(scripts)} Phase 6 scripts to fix")

fixed_count = 0
for script_path in scripts:
    content = script_path.read_text()
    
    # Extract epic ID
    match = re.search(r'EPIC_ID="(EPIC-CCN-\d+)"', content)
    if not match:
        print(f"WARNING: Could not find EPIC_ID in {script_path.name}")
        continue
    
    epic_id = match.group(1)
    
    # Find and replace the prerequisite check section
    # Look for the section between "# Prerequisite check" and "fi"
    pattern = re.compile(
        r'(# Prerequisite check:.*?\n)(.*?)(fi\n)',
        re.DOTALL
    )
    
    def replace_check(match):
        # NEW: Use find command for robust OR logic
        new_check = f'''# Prerequisite check: Verify Phase 5 completion file exists (robust OR logic)
if ! find docs/brain/{epic_id} -maxdepth 1 \\( -name "05-*.md" -o -name "ticket-*-completion.md" \\) -print -quit | grep -q .; then
    echo "ERROR: Missing Phase 5 completion files for {epic_id}"
    echo "Expected: docs/brain/{epic_id}/05-*.md OR ticket-*-completion.md"
    exit 1
fi
'''
        return new_check
    
    if pattern.search(content):
        # Create backup
        backup_path = script_path.with_suffix('.sh.bak3')
        backup_path.write_text(content)
        
        # Apply fix
        new_content = pattern.sub(replace_check, content, count=1)
        script_path.write_text(new_content)
        
        fixed_count += 1
        if fixed_count <= 5 or fixed_count % 10 == 0:
            print(f"Fixed: {script_path.name}")
    else:
        print(f"SKIP: {script_path.name} (pattern not found)")

print(f"\n✅ Fixed {fixed_count}/{len(scripts)} scripts")
print(f"✅ Backups created with .bak3 extension")
print(f"\nTest with: bash scripts/wave4/_p6_002.sh")

# Made with Bob
