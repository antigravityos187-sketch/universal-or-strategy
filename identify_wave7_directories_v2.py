#!/usr/bin/env python3
"""
Identify which existing directories belong to Wave 7 by matching method names.
Version 2: Uses 'method' field from roadmap.
"""

import os
import json
import glob
import re

def extract_method_from_epic_dir(epic_dir):
    """Extract method name from epic directory's 00-hotspots.md if it exists."""
    hotspots_file = os.path.join(epic_dir, "00-hotspots.md")
    if not os.path.exists(hotspots_file):
        return None
    
    try:
        with open(hotspots_file, 'r', encoding='utf-8') as f:
            content = f.read()
            # Look for method signature patterns
            match = re.search(r'\*\*Method\*\*:?\s*`([^`]+)`', content, re.IGNORECASE)
            if not match:
                match = re.search(r'Method:?\s*`([^`]+)`', content, re.IGNORECASE)
            if not match:
                # Try to find any method name in backticks near the top
                match = re.search(r'`([A-Z][a-zA-Z0-9_]+)`', content[:500])
            
            if match:
                return match.group(1)
    except Exception as e:
        print(f"  Error reading {hotspots_file}: {e}")
    
    return None

def main():
    print("=" * 80)
    print("IDENTIFYING WAVE 7 DIRECTORIES (V2)")
    print("=" * 80)
    
    # Load Wave 7 roadmap
    with open('epic_roadmap_wave7.json', 'r') as f:
        roadmap = json.load(f)
    
    # Extract Wave 7 method names from 'method' field
    wave7_methods = {}
    for epic_id, epic_data in roadmap['epics'].items():
        method_name = epic_data.get('method', '')
        if method_name:
            wave7_methods[method_name] = epic_id
    
    print(f"\nWave 7 contains {len(wave7_methods)} unique methods")
    print(f"Sample methods:")
    for i, method in enumerate(list(wave7_methods.keys())[:5]):
        print(f"  {i+1}. {method}")
    
    # Find all existing epic directories (all patterns)
    all_dirs = []
    all_dirs.extend(glob.glob("docs/brain/EPIC-[0-9]*"))
    all_dirs.extend(glob.glob("docs/brain/EPIC-CCN-*"))
    all_dirs.extend(glob.glob("docs/brain/EPIC-W7-*"))
    
    print(f"\nFound {len(all_dirs)} total epic directories")
    
    # Categorize directories
    wave7_dirs = []
    other_wave_dirs = []
    unknown_dirs = []
    
    for epic_dir in sorted(all_dirs):
        method_name = extract_method_from_epic_dir(epic_dir)
        
        if method_name:
            if method_name in wave7_methods:
                wave7_dirs.append((epic_dir, method_name, wave7_methods[method_name]))
            else:
                other_wave_dirs.append((epic_dir, method_name))
        else:
            unknown_dirs.append(epic_dir)
    
    print("\n" + "=" * 80)
    print("RESULTS")
    print("=" * 80)
    
    print(f"\n✅ WAVE 7 DIRECTORIES ({len(wave7_dirs)}):")
    if wave7_dirs:
        print("\nCurrent Name -> Method -> Should Be")
        print("-" * 80)
        for dir_path, method, correct_id in sorted(wave7_dirs, key=lambda x: x[2]):
            current_name = os.path.basename(dir_path)
            needs_rename = "✓ RENAME" if current_name != correct_id else "✓ OK"
            print(f"{current_name:25} -> {method:40} -> {correct_id:20} {needs_rename}")
    
    print(f"\n⚠️  OTHER WAVE DIRECTORIES ({len(other_wave_dirs)}):")
    if other_wave_dirs:
        print("(These are from previous waves - DO NOT RENAME)")
        for dir_path, method in other_wave_dirs[:10]:
            current_name = os.path.basename(dir_path)
            print(f"  {current_name:25} -> {method}")
        if len(other_wave_dirs) > 10:
            print(f"  ... and {len(other_wave_dirs) - 10} more")
    
    print(f"\n❓ UNKNOWN DIRECTORIES ({len(unknown_dirs)}):")
    if unknown_dirs:
        print("(No 00-hotspots.md found - likely incomplete)")
        for dir_path in unknown_dirs[:10]:
            print(f"  {os.path.basename(dir_path)}")
        if len(unknown_dirs) > 10:
            print(f"  ... and {len(unknown_dirs) - 10} more")
    
    # Generate safe rename script
    print("\n" + "=" * 80)
    print("SAFE RENAME SCRIPT")
    print("=" * 80)
    
    renames_needed = [(d, m, c) for d, m, c in wave7_dirs if os.path.basename(d) != c]
    
    if renames_needed:
        print(f"\n# {len(renames_needed)} Wave 7 directories need renaming\n")
        print("#!/bin/bash")
        print("set -e\n")
        
        for dir_path, method, correct_id in sorted(renames_needed, key=lambda x: x[2]):
            current_name = os.path.basename(dir_path)
            print(f'# {method}')
            print(f'mv "docs/brain/{current_name}" "docs/brain/{correct_id}"')
            print()
    else:
        print("\n✅ All Wave 7 directories already have correct names!")
    
    print("\n" + "=" * 80)
    print("SUMMARY")
    print("=" * 80)
    print(f"Wave 7 directories found: {len(wave7_dirs)}")
    print(f"  - Already correct: {len(wave7_dirs) - len(renames_needed)}")
    print(f"  - Need renaming: {len(renames_needed)}")
    print(f"Other wave directories: {len(other_wave_dirs)} (SAFE - will not touch)")
    print(f"Unknown directories: {len(unknown_dirs)} (incomplete epics)")
    print("=" * 80)

if __name__ == "__main__":
    main()

# Made with Bob
