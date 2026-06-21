#!/usr/bin/env python3
"""
Fix function name: verify_dependencies -> validate_dependencies
The pilot template had the wrong function name.
"""

import glob

# Function name fix
OLD_NAME = 'module.verify_dependencies'
NEW_NAME = 'module.validate_dependencies'

def fix_script(script_path):
    """Fix function name in a single script."""
    with open(script_path, 'r') as f:
        content = f.read()
    
    if OLD_NAME in content:
        content = content.replace(OLD_NAME, NEW_NAME)
        with open(script_path, 'w') as f:
            f.write(content)
        return True
    return False

def main():
    print("=" * 50)
    print("Fixing Function Names")
    print("verify_dependencies -> validate_dependencies")
    print("=" * 50)
    
    scripts = glob.glob("scripts/wave6/_p1_epic_ccn_*.sh")
    scripts.sort()
    
    fixed_count = 0
    for script in scripts:
        if fix_script(script):
            print(f"Fixed: {script}")
            fixed_count += 1
    
    print()
    print("=" * 50)
    print(f"Complete: {fixed_count}/{len(scripts)} scripts fixed")
    print("=" * 50)

if __name__ == "__main__":
    main()

# Made with Bob
