#!/usr/bin/env python3
"""
Fix Phase 1 import errors by replacing broken sys.path pattern with working importlib pattern.
Uses Python string replacement for accurate matching.
"""

import os
import glob
from pathlib import Path

# Patterns to replace
REPLACEMENTS = [
    # verify_dependencies
    (
        'python3 -c "import sys; sys.path.insert(0, \'scripts\'); from epic_manifest import verify_dependencies; result = verify_dependencies(',
        'python3 -c "import importlib.util; spec = importlib.util.spec_from_file_location(\'epic_manifest\', \'scripts/epic_manifest.py\'); module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); result = module.verify_dependencies(',
    ),
    # Add import sys before sys.exit
    (
        '); sys.exit(0 if result else 1)"',
        '); import sys; sys.exit(0 if result else 1)"'
    ),
    (
        '); print(reason if not can_exec else \'OK\'); sys.exit(0 if can_exec else 1)"',
        '); print(reason if not can_exec else \'OK\'); import sys; sys.exit(0 if can_exec else 1)"'
    ),
    (
        '); print(reason if not started else \'OK\'); sys.exit(0 if started else 1)"',
        '); print(reason if not started else \'OK\'); import sys; sys.exit(0 if started else 1)"'
    ),
    (
        '); print(reason if not completed else \'OK\'); sys.exit(0 if completed else 1)"',
        '); print(reason if not completed else \'OK\'); import sys; sys.exit(0 if completed else 1)"'
    ),
    # verify_can_execute
    (
        'python3 -c "import sys; sys.path.insert(0, \'scripts\'); from epic_manifest import verify_can_execute; can_exec, reason = verify_can_execute(',
        'python3 -c "import importlib.util; spec = importlib.util.spec_from_file_location(\'epic_manifest\', \'scripts/epic_manifest.py\'); module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); can_exec, reason = module.verify_can_execute('
    ),
    # verify_filesystem_state
    (
        'python3 -c "import sys; sys.path.insert(0, \'scripts\'); from epic_manifest import verify_filesystem_state; result = verify_filesystem_state(',
        'python3 -c "import importlib.util; spec = importlib.util.spec_from_file_location(\'epic_manifest\', \'scripts/epic_manifest.py\'); module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); result = module.verify_filesystem_state('
    ),
    # start_phase_execution
    (
        'python3 -c "import sys; sys.path.insert(0, \'scripts\'); from epic_manifest import start_phase_execution; started, reason = start_phase_execution(',
        'python3 -c "import importlib.util; spec = importlib.util.spec_from_file_location(\'epic_manifest\', \'scripts/epic_manifest.py\'); module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); started, reason = module.start_phase_execution('
    ),
    # fail_phase_execution
    (
        'python3 -c "import sys; sys.path.insert(0, \'scripts\'); from epic_manifest import fail_phase_execution; fail_phase_execution(',
        'python3 -c "import importlib.util; spec = importlib.util.spec_from_file_location(\'epic_manifest\', \'scripts/epic_manifest.py\'); module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); module.fail_phase_execution('
    ),
    # complete_phase_execution
    (
        'python3 -c "import sys; sys.path.insert(0, \'scripts\'); from epic_manifest import complete_phase_execution; completed, reason = complete_phase_execution(',
        'python3 -c "import importlib.util; spec = importlib.util.spec_from_file_location(\'epic_manifest\', \'scripts/epic_manifest.py\'); module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module); completed, reason = module.complete_phase_execution('
    ),
]

def fix_script(script_path):
    """Fix imports in a single script."""
    # Backup if not already backed up
    backup_path = f"{script_path}.backup"
    if not os.path.exists(backup_path):
        with open(script_path, 'r') as f:
            content = f.read()
        with open(backup_path, 'w') as f:
            f.write(content)
    
    # Read current content
    with open(script_path, 'r') as f:
        content = f.read()
    
    # Apply all replacements
    modified = False
    for old, new in REPLACEMENTS:
        if old in content:
            content = content.replace(old, new)
            modified = True
    
    # Write back if modified
    if modified:
        with open(script_path, 'w') as f:
            f.write(content)
        return True
    return False

def main():
    print("=" * 50)
    print("Fixing Phase 1 Import Errors (Python method)")
    print("=" * 50)
    
    # Find all Phase 1 scripts
    scripts = glob.glob("scripts/wave6/_p1_epic_ccn_*.sh")
    scripts.sort()
    
    fixed_count = 0
    for script in scripts:
        print(f"Fixing: {script}")
        if fix_script(script):
            fixed_count += 1
    
    print()
    print("=" * 50)
    print(f"Fix Complete: {fixed_count}/{len(scripts)} scripts modified")
    print("=" * 50)
    
    # Test one script
    print("\nTesting EPIC-CCN-004...")
    os.system("bash scripts/wave6/_p1_epic_ccn_004.sh 2>&1 | head -20")

if __name__ == "__main__":
    main()

# Made with Bob
