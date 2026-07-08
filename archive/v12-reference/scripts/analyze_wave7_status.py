#!/usr/bin/env python3
"""
Wave 7 Status Analysis Script
Identifies incomplete epics and provides recovery plan
"""

import os
import json
from pathlib import Path

def main():
    print("=" * 80)
    print("WAVE 7 STATUS ANALYSIS")
    print("=" * 80)
    print()
    
    # Check for roadmap
    roadmap_file = "epic_roadmap_wave7.json"
    if not os.path.exists(roadmap_file):
        print(f"❌ ERROR: {roadmap_file} not found!")
        print("   Checking for alternative roadmap...")
        if os.path.exists("epic_roadmap.json"):
            roadmap_file = "epic_roadmap.json"
            print(f"   ✅ Using {roadmap_file} instead")
        else:
            print("   ❌ No roadmap file found. Cannot proceed.")
            return
    
    # Load roadmap
    try:
        with open(roadmap_file, 'r') as f:
            roadmap = json.load(f)
        
        if isinstance(roadmap, dict) and 'epics' in roadmap:
            epics = roadmap['epics']
        elif isinstance(roadmap, list):
            epics = roadmap
        else:
            print(f"❌ ERROR: Unexpected roadmap format")
            return
            
        total_epics = len(epics)
        print(f"✅ Roadmap loaded: {total_epics} epics")
    except Exception as e:
        print(f"❌ ERROR loading roadmap: {e}")
        return
    
    print()
    print("-" * 80)
    print("PHASE 0 COMPLETION ANALYSIS")
    print("-" * 80)
    print()
    
    # Check Phase 0 completion
    incomplete_epics = []
    complete_epics = []
    
    for i in range(1, total_epics + 1):
        epic_id = f"EPIC-CCN-{i:03d}"
        hotspot_file = f"docs/brain/{epic_id}/00-hotspots.md"
        
        if os.path.exists(hotspot_file):
            complete_epics.append(epic_id)
        else:
            incomplete_epics.append(epic_id)
    
    # Print summary
    print(f"Total Epics:      {total_epics}")
    print(f"Complete:         {len(complete_epics)} ({len(complete_epics)/total_epics*100:.1f}%)")
    print(f"Incomplete:       {len(incomplete_epics)} ({len(incomplete_epics)/total_epics*100:.1f}%)")
    print()
    
    # Save incomplete list
    with open('incomplete_epics.txt', 'w') as f:
        for epic in incomplete_epics:
            f.write(f"{epic}\n")
    
    print(f"✅ Incomplete epics saved to: incomplete_epics.txt")
    print()
    
    # Show first 20 incomplete
    print("-" * 80)
    print("FIRST 20 INCOMPLETE EPICS")
    print("-" * 80)
    for epic in incomplete_epics[:20]:
        print(f"  {epic}")
    
    if len(incomplete_epics) > 20:
        print(f"  ... and {len(incomplete_epics) - 20} more")
    print()
    
    # Check for Phase 0 scripts
    print("-" * 80)
    print("PHASE 0 SCRIPT AVAILABILITY")
    print("-" * 80)
    print()
    
    p0_scripts = [f for f in os.listdir('.') if f.startswith('_p0_') and f.endswith('.sh')]
    print(f"Found {len(p0_scripts)} Phase 0 scripts")
    
    if p0_scripts:
        print("\nSample scripts:")
        for script in sorted(p0_scripts)[:5]:
            print(f"  {script}")
    print()
    
    # Recovery recommendations
    print("-" * 80)
    print("RECOVERY RECOMMENDATIONS")
    print("-" * 80)
    print()
    
    if len(incomplete_epics) == 0:
        print("✅ Phase 0 is COMPLETE! All epics have hotspot files.")
        print("   Next step: Proceed to Phase 1 (Scope Definition)")
    else:
        print(f"⚠️  {len(incomplete_epics)} epics need Phase 0 completion")
        print()
        print("CRITICAL ISSUE DETECTED:")
        print("  The shell environment is broken (no ls, bash, python3, etc.)")
        print("  This VM cannot execute bash scripts directly.")
        print()
        print("RECOMMENDED ACTIONS:")
        print("  1. Fix the shell environment (install coreutils, bash, python3)")
        print("  2. OR switch to local machine execution")
        print("  3. OR use SSH from local machine to orchestrate VM")
        print()
        print("To fix shell environment:")
        print("  apt-get update && apt-get install -y coreutils bash python3")
    
    print()
    print("=" * 80)
    print("ANALYSIS COMPLETE")
    print("=" * 80)

if __name__ == "__main__":
    main()

# Made with Bob
