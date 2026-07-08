#!/usr/bin/env python3
"""
Wave 7 Cleanup and Relaunch Script
1. Identify the 10 complete EPIC-W7-XXX directories
2. Delete all other epic directories (confusion from previous waves)
3. Kill stopped launcher process
4. Generate list of 151 remaining epics
5. Create relaunch script
"""

import os
import json
import glob
import shutil
import subprocess

def get_wave7_methods():
    """Load Wave 7 roadmap and extract methods."""
    with open('epic_roadmap_wave7.json', 'r') as f:
        roadmap = json.load(f)
    
    wave7_methods = {}
    for epic_id, epic_data in roadmap['epics'].items():
        method_name = epic_data.get('method', '')
        if method_name:
            wave7_methods[method_name] = epic_id
    
    return wave7_methods

def extract_method_from_hotspots(epic_dir):
    """Extract method name from 00-hotspots.md."""
    hotspots_file = os.path.join(epic_dir, "00-hotspots.md")
    if not os.path.exists(hotspots_file):
        return None
    
    try:
        with open(hotspots_file, 'r', encoding='utf-8') as f:
            content = f.read()
            import re
            match = re.search(r'\*\*Method\*\*:?\s*`([^`]+)`', content, re.IGNORECASE)
            if not match:
                match = re.search(r'Method:?\s*`([^`]+)`', content, re.IGNORECASE)
            if not match:
                match = re.search(r'`([A-Z][a-zA-Z0-9_]+)`', content[:500])
            
            if match:
                return match.group(1)
    except:
        pass
    
    return None

def main():
    print("=" * 80)
    print("WAVE 7 CLEANUP AND RELAUNCH")
    print("=" * 80)
    
    # Load Wave 7 methods
    wave7_methods = get_wave7_methods()
    print(f"\nWave 7 contains {len(wave7_methods)} methods")
    
    # Find all epic directories
    all_dirs = []
    all_dirs.extend(glob.glob("docs/brain/EPIC-[0-9]*"))
    all_dirs.extend(glob.glob("docs/brain/EPIC-CCN-*"))
    all_dirs.extend(glob.glob("docs/brain/EPIC-W7-*"))
    
    print(f"Found {len(all_dirs)} total epic directories")
    
    # Identify Wave 7 complete directories
    wave7_complete = []
    to_delete = []
    
    for epic_dir in sorted(all_dirs):
        epic_name = os.path.basename(epic_dir)
        
        # Skip if not a directory
        if not os.path.isdir(epic_dir):
            continue
            
        files = os.listdir(epic_dir)
        
        # Check if Phase 0 complete (exactly 2 files)
        if len(files) == 2 and 'manifest.json' in files and '00-hotspots.md' in files:
            method = extract_method_from_hotspots(epic_dir)
            
            # Check if it's a Wave 7 method with correct naming
            if method and method in wave7_methods:
                correct_id = wave7_methods[method]
                if epic_name == correct_id:  # Correct EPIC-W7-XXX naming
                    wave7_complete.append((epic_name, method))
                    print(f"✅ KEEP: {epic_name} ({method})")
                else:
                    to_delete.append((epic_dir, epic_name, method, "Wrong naming"))
            else:
                to_delete.append((epic_dir, epic_name, method or "Unknown", "Not Wave 7"))
        else:
            to_delete.append((epic_dir, epic_name, "N/A", f"{len(files)} files"))
    
    print(f"\n" + "=" * 80)
    print("SUMMARY")
    print("=" * 80)
    print(f"✅ Wave 7 Complete (KEEP): {len(wave7_complete)} directories")
    print(f"🗑️  To Delete: {len(to_delete)} directories")
    
    # Show what will be kept
    print(f"\n📋 KEEPING THESE {len(wave7_complete)} WAVE 7 DIRECTORIES:")
    for epic_name, method in wave7_complete:
        print(f"  {epic_name} - {method}")
    
    # Show what will be deleted
    print(f"\n🗑️  DELETING THESE {len(to_delete)} DIRECTORIES:")
    for epic_dir, epic_name, method, reason in to_delete[:20]:
        print(f"  {epic_name} - {method} ({reason})")
    if len(to_delete) > 20:
        print(f"  ... and {len(to_delete) - 20} more")
    
    # Calculate remaining
    completed_epic_ids = [epic_id for epic_id, _ in wave7_complete]
    all_wave7_ids = sorted(wave7_methods.values())
    remaining_ids = [eid for eid in all_wave7_ids if eid not in completed_epic_ids]
    
    print(f"\n📊 WAVE 7 STATUS:")
    print(f"  Complete: {len(wave7_complete)}/161")
    print(f"  Remaining: {len(remaining_ids)}/161")
    
    # Ask for confirmation
    print(f"\n" + "=" * 80)
    print("⚠️  WARNING: This will DELETE {len(to_delete)} directories!")
    print("=" * 80)
    response = input("\nType 'DELETE' to proceed with cleanup: ")
    
    if response != 'DELETE':
        print("\n❌ Cleanup cancelled. No changes made.")
        return
    
    # Delete directories
    print(f"\n🗑️  Deleting {len(to_delete)} directories...")
    deleted_count = 0
    for epic_dir, epic_name, method, reason in to_delete:
        try:
            shutil.rmtree(epic_dir)
            deleted_count += 1
            if deleted_count <= 10:
                print(f"  ✓ Deleted {epic_name}")
        except Exception as e:
            print(f"  ✗ Failed to delete {epic_name}: {e}")
    
    if deleted_count > 10:
        print(f"  ... and {deleted_count - 10} more")
    
    print(f"\n✅ Deleted {deleted_count} directories")
    
    # Kill stopped process
    print(f"\n🔪 Killing stopped launcher process...")
    try:
        subprocess.run(['/usr/bin/pkill', '-9', '-f', 'launch_wave7_python.py'], timeout=5)
        print("  ✓ Process killed")
    except:
        print("  ℹ️  No process to kill")
    
    # Save remaining epic list
    with open('wave7_remaining_epics.txt', 'w') as f:
        for epic_id in remaining_ids:
            f.write(f"{epic_id}\n")
    
    print(f"\n✅ Saved {len(remaining_ids)} remaining epics to wave7_remaining_epics.txt")
    
    # Create relaunch script
    print(f"\n📝 Creating relaunch script...")
    with open('relaunch_wave7_clean.sh', 'w') as f:
        f.write("#!/bin/bash\n")
        f.write("# Wave 7 Clean Relaunch - 151 remaining epics\n")
        f.write("set -e\n\n")
        f.write("echo 'Starting Wave 7 Phase 0 execution for 151 remaining epics...'\n")
        f.write("echo ''\n\n")
        f.write("/usr/bin/python3 launch_wave7_python.py\n")
    
    os.chmod('relaunch_wave7_clean.sh', 0o755)
    
    print("\n" + "=" * 80)
    print("✅ CLEANUP COMPLETE!")
    print("=" * 80)
    print(f"Wave 7 directories: {len(wave7_complete)} complete, {len(remaining_ids)} remaining")
    print(f"Deleted: {deleted_count} directories")
    print(f"Next: Run './relaunch_wave7_clean.sh' to continue Wave 7")
    print("=" * 80)

if __name__ == "__main__":
    main()

# Made with Bob
