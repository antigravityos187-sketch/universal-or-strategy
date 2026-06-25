#!/usr/bin/env python3
"""
Fix Wave 7 naming convention: EPIC-CCN-XXX -> EPIC-W7-XXX
"""

import os
import glob
import re

def main():
    print("=" * 80)
    print("FIXING WAVE 7 NAMING CONVENTION")
    print("=" * 80)
    print("\nChanging: EPIC-CCN-XXX -> EPIC-W7-XXX")
    
    # Get all Phase 0 scripts
    scripts = sorted(glob.glob("_p0_*.sh"))
    print(f"\nFound {len(scripts)} Phase 0 scripts to fix")
    
    fixed_count = 0
    
    for script in scripts:
        with open(script, 'r') as f:
            content = f.read()
        
        # Replace EPIC-CCN- with EPIC-W7-
        new_content = content.replace('EPIC-CCN-', 'EPIC-W7-')
        
        if new_content != content:
            with open(script, 'w') as f:
                f.write(new_content)
            fixed_count += 1
            
            # Extract epic number for reporting
            match = re.search(r'_p0_(\d+)\.sh', script)
            if match and fixed_count <= 5:
                epic_num = match.group(1)
                print(f"  ✓ Fixed {script}: EPIC-CCN-{epic_num} -> EPIC-W7-{epic_num}")
    
    if fixed_count > 5:
        print(f"  ... and {fixed_count - 5} more scripts")
    
    print(f"\n✅ Fixed {fixed_count} scripts")
    
    # Check for any EPIC-CCN directories that need renaming
    ccn_dirs = glob.glob("docs/brain/EPIC-CCN-*")
    if ccn_dirs:
        print(f"\n⚠️  Found {len(ccn_dirs)} EPIC-CCN-* directories that need renaming:")
        for d in ccn_dirs[:5]:
            epic_num = d.split('-')[-1]
            new_name = f"docs/brain/EPIC-W7-{epic_num}"
            print(f"  {d} -> {new_name}")
        if len(ccn_dirs) > 5:
            print(f"  ... and {len(ccn_dirs) - 5} more")
        print("\nRun: for d in docs/brain/EPIC-CCN-*; do mv \"$d\" \"${d/EPIC-CCN-/EPIC-W7-}\"; done")
    
    print("\n" + "=" * 80)
    print("NEXT STEPS:")
    print("=" * 80)
    print("1. Rename any existing EPIC-CCN-* directories to EPIC-W7-*")
    print("2. Update Python launcher to check for EPIC-W7-* directories")
    print("3. Resume Wave 7 execution with correct naming")
    print("=" * 80)

if __name__ == "__main__":
    main()

# Made with Bob
