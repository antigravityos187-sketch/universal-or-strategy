#!/usr/bin/env python3
"""
Fix Phase 6 prerequisite check V2 - Use proper OR logic.
Issue: ls with multiple patterns fails if ANY pattern fails, even if one succeeds.
Solution: Check each pattern separately with OR logic.
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
    
    # OLD: Flawed check (ls with multiple patterns - fails if ANY fails)
    old_check_pattern = re.compile(
        r'# Prerequisite check:.*?\n'
        r'if ! ls docs/brain/' + re.escape(epic_id) + r'/05-\*\.md docs/brain/' + re.escape(epic_id) + r'/ticket-\*-completion\.md.*?then\n'
        r'.*?exit 1\n'
        r'fi',
        re.DOTALL
    )
    
    # NEW: Proper OR logic (check each pattern separately)
    new_check = f'''# Prerequisite check: Verify Phase 5 completion file exists (proper OR logic)
if [ ! -e docs/brain/{epic_id}/05-*.md ] && [ ! -e docs/brain/{epic_id}/ticket-*-completion.md ]; then
    echo "ERROR: Missing Phase 5 completion files for {epic_id}"
    echo "Expected: docs/brain/{epic_id}/05-*.md OR ticket-*-completion.md"
    exit 1
fi'''
    
    if old_check_pattern.search(content):
        # Create backup
        backup_path = script_path.with_suffix('.sh.bak2')
        backup_path.write_text(content)
        
        # Apply fix
        new_content = old_check_pattern.sub(new_check, content)
        script_path.write_text(new_content)
        
        fixed_count += 1
        if fixed_count <= 5 or fixed_count % 10 == 0:
            print(f"Fixed: {script_path.name}")
    else:
        print(f"SKIP: {script_path.name} (pattern not found)")

print(f"\n✅ Fixed {fixed_count}/{len(scripts)} scripts")
print(f"✅ Backups created with .bak2 extension")

# Made with Bob
