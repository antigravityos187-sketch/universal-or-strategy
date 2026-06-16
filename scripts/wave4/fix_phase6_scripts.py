#!/usr/bin/env python3
"""
Fix Phase 6 scripts to accept any Phase 5 completion file pattern.
Issue: Scripts check for exact "05-completion.md" but Phase 5 created various patterns.
"""

import re
from pathlib import Path

# Find all Phase 6 scripts
scripts_dir = Path('/home/malhitticrypto/universal-or-strategy/scripts/wave4')
scripts = sorted(scripts_dir.glob('_p6_*.sh'))

print(f"Found {len(scripts)} Phase 6 scripts to fix")

fixed_count = 0
for script_path in scripts:
    # Read script
    content = script_path.read_text()
    
    # Extract epic ID from script
    match = re.search(r'EPIC_ID="(EPIC-CCN-\d+)"', content)
    if not match:
        print(f"WARNING: Could not find EPIC_ID in {script_path.name}")
        continue
    
    epic_id = match.group(1)
    
    # Replace strict prerequisite check with flexible one
    old_check = f'''# Prerequisite check: Verify Phase 5 completion file exists
if [ ! -f "docs/brain/{epic_id}/05-completion.md" ]; then
    echo "ERROR: Missing Phase 5 completion file for {epic_id}"
    exit 1
fi'''
    
    new_check = f'''# Prerequisite check: Verify Phase 5 completion file exists (flexible pattern)
if ! ls docs/brain/{epic_id}/05-*.md docs/brain/{epic_id}/ticket-*-completion.md 1>/dev/null 2>&1; then
    echo "ERROR: Missing Phase 5 completion files for {epic_id}"
    echo "Expected: docs/brain/{epic_id}/05-*.md OR ticket-*-completion.md"
    exit 1
fi'''
    
    if old_check in content:
        # Create backup
        backup_path = script_path.with_suffix('.sh.bak')
        backup_path.write_text(content)
        
        # Apply fix
        new_content = content.replace(old_check, new_check)
        script_path.write_text(new_content)
        
        fixed_count += 1
        if fixed_count <= 5 or fixed_count % 10 == 0:
            print(f"Fixed: {script_path.name}")
    else:
        print(f"SKIP: {script_path.name} (already fixed or different format)")

print(f"\n✅ Fixed {fixed_count}/{len(scripts)} scripts")
print(f"✅ Backups created with .bak extension")

# Made with Bob
