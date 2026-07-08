#!/usr/bin/env python3
"""
Add PATH export to the top of all generated Phase 0 scripts.
This ensures they work regardless of shell environment.
"""

import os

def add_path_to_script(script_path):
    """Add PATH export after shebang."""
    with open(script_path, 'r') as f:
        lines = f.readlines()
    
    # Check if PATH is already set
    if any('export PATH=' in line for line in lines):
        return False
    
    # Find the line after shebang and 'set -e'
    insert_pos = 1
    for i, line in enumerate(lines):
        if line.startswith('set -e'):
            insert_pos = i + 1
            break
    
    # Insert PATH export
    path_line = 'export PATH="/usr/bin:/usr/local/bin:/home/malhitticrypto/.npm-global/bin:/usr/local/sbin:/usr/sbin:/sbin:$PATH"\n'
    lines.insert(insert_pos, path_line)
    
    with open(script_path, 'w') as f:
        f.writelines(lines)
    
    return True

def main():
    """Add PATH to all generated Phase 0 scripts."""
    print("=== Add PATH Export to Phase 0 Scripts ===")
    print()
    
    ranges = [
        range(81, 107),
        range(116, 162)
    ]
    
    fixed = 0
    
    for r in ranges:
        for epic_num in r:
            script_path = f"_p0_{epic_num:03d}.sh"
            
            if not os.path.exists(script_path):
                continue
            
            if add_path_to_script(script_path):
                print(f"✅ {script_path} - PATH added")
                fixed += 1
    
    print()
    print(f"Fixed: {fixed} scripts")
    print("✅ All scripts now set PATH internally")

if __name__ == '__main__':
    main()

# Made with Bob
