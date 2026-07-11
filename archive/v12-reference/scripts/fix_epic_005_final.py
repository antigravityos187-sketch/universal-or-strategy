#!/usr/bin/env python3
"""
Fix EPIC-W7-005 - Final Attempt

Uses the new universal launcher with fixed environment.
"""

import sys
import os

# Add building-blocks to path
sys.path.insert(0, 'building-blocks/wave7')

from launch_epic_with_fixed_env import launch_epic
import time

def main():
    print("=" * 70)
    print("EPIC-W7-005 Final Fix Attempt")
    print("=" * 70)
    print()
    print("Using universal launcher with fixed environment")
    print("  - Explicit PATH setting")
    print("  - Fresh API key")
    print("  - Python-created directories")
    print()
    
    # Ensure brain directory exists
    os.makedirs("docs/brain/EPIC-W7-005", exist_ok=True)
    
    # Launch with fixed environment
    script = "_p0_005.sh"
    log_file = "logs/phase0/EPIC-W7-005_final_fix.log"
    
    print(f"Launching: {script}")
    print(f"Log: {log_file}")
    print()
    
    proc = launch_epic(script, log_file)
    
    print(f"Process started: PID {proc.pid}")
    print()
    print("Waiting 30 seconds for completion...")
    
    # Wait for completion
    time.sleep(30)
    
    # Check if completed
    hotspots = "docs/brain/EPIC-W7-005/00-hotspots.md"
    manifest = "docs/brain/EPIC-W7-005/manifest.json"
    
    if os.path.exists(hotspots) and os.path.exists(manifest):
        print("✅ SUCCESS! EPIC-W7-005 completed")
        print(f"   - {hotspots}")
        print(f"   - {manifest}")
        
        # Verify overall completion
        completed = 0
        for i in range(1, 162):
            epic_id = f"EPIC-W7-{i:03d}"
            if os.path.exists(f"docs/brain/{epic_id}/00-hotspots.md"):
                completed += 1
        
        print()
        print("=" * 70)
        print(f"WAVE 7 PHASE 0 COMPLETE: {completed}/161 ({completed/161*100:.1f}%)")
        print("=" * 70)
        
        if completed == 161:
            print()
            print("🎉 ALL 161 EPICS COMPLETE!")
            print("Ready for Phase 1 (Scope Definition)")
        
    else:
        print("⚠️  Still incomplete - check log for errors")
        print(f"   tail -f {log_file}")
        
        # Check if process still running
        try:
            os.kill(proc.pid, 0)
            print(f"   Process still running (PID {proc.pid})")
        except OSError:
            print(f"   Process completed (PID {proc.pid})")

if __name__ == "__main__":
    main()

# Made with Bob
