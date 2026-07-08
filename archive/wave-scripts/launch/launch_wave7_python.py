#!/usr/bin/env python3
"""
Wave 7 Phase 0 - Python-based continuous launcher
Avoids shell PATH issues by using Python subprocess
"""

import os
import subprocess
import sys
from pathlib import Path

def main():
    print("=" * 80)
    print("WAVE 7 PHASE 0 - PYTHON LAUNCHER (CLEAN RESTART)")
    print("=" * 80)
    print()
    
    # Read remaining epics from file
    remaining_file = "wave7_remaining_epics.txt"
    if not os.path.exists(remaining_file):
        print(f"❌ Error: {remaining_file} not found")
        print("Run cleanup_and_relaunch_wave7.py first")
        sys.exit(1)
    
    with open(remaining_file, 'r') as f:
        incomplete = [line.strip() for line in f if line.strip()]
    
    total = len(incomplete)
    print(f"Found {total} incomplete epics from {remaining_file}")
    print()
    
    completed = 0
    failed = 0
    
    for idx, epic_id in enumerate(incomplete, 1):
        # Extract epic number from EPIC-W7-XXX format
        epic_num = int(epic_id.split('-')[-1])
        script = f"_p0_{epic_num:03d}.sh"
        
        print("-" * 80)
        print(f"[{idx}/{total}] Processing {epic_id}")
        print("-" * 80)
        
        if not os.path.exists(script):
            print(f"⚠️  Script {script} not found - skipping")
            failed += 1
            continue
        
        # Execute script
        try:
            result = subprocess.run(
                ["/usr/bin/bash", script],
                capture_output=False,
                text=True,
                timeout=600  # 10 minute timeout per epic
            )
            
            # Check if output file was created
            hotspot_file = f"docs/brain/{epic_id}/00-hotspots.md"
            if os.path.exists(hotspot_file):
                print(f"✅ {epic_id} complete")
                completed += 1
            else:
                print(f"⚠️  {epic_id} script ran but no output file")
                failed += 1
                
        except subprocess.TimeoutExpired:
            print(f"❌ {epic_id} timed out (>10 minutes)")
            failed += 1
        except Exception as e:
            print(f"❌ {epic_id} failed: {e}")
            failed += 1
        
        print()
    
    print("=" * 80)
    print("EXECUTION COMPLETE")
    print("=" * 80)
    print(f"Completed: {completed}/{total}")
    print(f"Failed: {failed}/{total}")
    print()
    
    # Final count
    final_count = len(list(Path("docs/brain").glob("EPIC-W7-*/00-hotspots.md")))
    print(f"Final total: {final_count}/161 epics complete")
    
    if final_count == 161:
        print("🎉 SUCCESS! All 161 Wave 7 epics complete!")
    else:
        print(f"⚠️  {161 - final_count} epics still incomplete")

if __name__ == "__main__":
    main()

# Made with Bob
