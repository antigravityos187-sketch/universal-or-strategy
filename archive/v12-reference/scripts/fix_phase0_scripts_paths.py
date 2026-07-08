#!/usr/bin/env python3
"""
Fix PATH issues in generated Phase 0 scripts.
Replace ALL relative commands with absolute paths, including bob CLI.
"""

import os
import re

def fix_script(script_path):
    """Fix PATH issues in a single script."""
    with open(script_path, 'r') as f:
        content = f.read()
    
    original = content
    
    # Fix bob CLI command (most critical)
    content = re.sub(r'\bbob\s+--', r'/home/malhitticrypto/.npm-global/bin/bob --', content)
    
    # Fix all other command references
    commands = [
        'mkdir', 'cat', 'tee', 'echo', 'ls', 'wc', 'head', 'tail',
        'grep', 'sed', 'awk', 'find', 'sort', 'uniq', 'sleep'
    ]
    
    for cmd in commands:
        # Fix standalone commands at start of line or after pipe/semicolon
        content = re.sub(rf'(^|\n|\||;)\s*{cmd}\s+', rf'\1/usr/bin/{cmd} ', content)
        # Fix commands in command substitution
        content = re.sub(rf'\$\({cmd}\s+', rf'$(/usr/bin/{cmd} ', content)
    
    # Only write if changed
    if content != original:
        with open(script_path, 'w') as f:
            f.write(content)
        return True
    return False

def main():
    """Fix all generated Phase 0 scripts."""
    print("=== Fix Phase 0 Scripts PATH Issues (Including Bob CLI) ===")
    print()
    
    # Ranges of generated scripts
    ranges = [
        range(81, 107),   # 26 scripts
        range(116, 162)   # 46 scripts
    ]
    
    fixed = 0
    skipped = 0
    
    for r in ranges:
        for epic_num in r:
            script_path = f"_p0_{epic_num:03d}.sh"
            
            if not os.path.exists(script_path):
                print(f"⚠️  {script_path} not found - skipping")
                skipped += 1
                continue
            
            if fix_script(script_path):
                print(f"✅ {script_path} fixed")
                fixed += 1
            else:
                print(f"⏭️  {script_path} already correct")
                skipped += 1
    
    print()
    print(f"Fixed: {fixed} scripts")
    print(f"Skipped: {skipped} scripts")
    print()
    
    if fixed > 0:
        print("✅ All generated scripts now use absolute paths (including bob CLI)")
    else:
        print("ℹ️  No scripts needed fixing")

if __name__ == '__main__':
    main()

# Made with Bob
