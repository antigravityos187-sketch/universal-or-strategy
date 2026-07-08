#!/usr/bin/env python3
"""
Wave 7 Phase 0 - Final 5 Epic Recovery with PATH Fix

ROOT CAUSE: subprocess.Popen inherits broken PATH from parent process
SOLUTION: Explicitly set PATH in subprocess environment

Failed epics: 5, 22, 39, 55, 73
"""

import os
import subprocess
import time
from pathlib import Path

# Failed epics that need PATH fix
FAILED_EPICS = [5, 22, 39, 55, 73]

def launch_epic_with_fixed_path(epic_num):
    """Launch epic with explicitly fixed PATH environment"""
    epic_id = f"EPIC-W7-{epic_num:03d}"
    script = f"_p0_{epic_num:03d}.sh"
    log_file = f"logs/phase0/{epic_id}_recovery3.log"
    
    # Ensure log directory exists
    os.makedirs("logs/phase0", exist_ok=True)
    
    # Ensure brain directory exists (Python creates it, not shell)
    brain_dir = f"docs/brain/{epic_id}"
    os.makedirs(brain_dir, exist_ok=True)
    
    print(f"Launching {epic_id} with fixed PATH...")
    
    # **THE FIX**: Explicitly set PATH in subprocess environment
    env = os.environ.copy()
    env["PATH"] = "/usr/bin:/bin:/usr/local/bin:" + env.get("PATH", "")
    
    with open(log_file, 'w') as log:
        proc = subprocess.Popen(
            ['/usr/bin/bash', script],
            stdout=log,
            stderr=subprocess.STDOUT,
            env=env  # <-- THE CRITICAL FIX
        )
    
    print(f"  PID: {proc.pid}, Log: {log_file}")
    return proc.pid

def main():
    print("=" * 70)
    print("Wave 7 Phase 0 - Final 5 Epic Recovery with PATH Fix")
    print("=" * 70)
    print()
    print("ROOT CAUSE IDENTIFIED:")
    print("  - Working scripts (156) inherited proper PATH from initial launch")
    print("  - Failed scripts (5) inherited broken PATH from recovery launch")
    print("  - Broken PATH missing /usr/bin and /bin (mkdir, cat not found)")
    print()
    print("LONG-TERM FIX:")
    print("  - Explicitly set PATH in subprocess.Popen environment")
    print("  - env['PATH'] = '/usr/bin:/bin:/usr/local/bin:' + existing_path")
    print("  - Python creates directories (not shell mkdir)")
    print()
    print(f"Launching {len(FAILED_EPICS)} epics with fixed environment...")
    print()
    
    pids = []
    for i, epic_num in enumerate(FAILED_EPICS):
        pid = launch_epic_with_fixed_path(epic_num)
        pids.append((epic_num, pid))
        
        # Stagger launches by 5 seconds
        if i < len(FAILED_EPICS) - 1:
            time.sleep(5)
    
    print()
    print("=" * 70)
    print("All 5 epics launched with fixed PATH environment")
    print("=" * 70)
    print()
    print("PIDs:")
    for epic_num, pid in pids:
        print(f"  EPIC-W7-{epic_num:03d}: {pid}")
    print()
    print("Monitor with:")
    print("  tail -f logs/phase0/EPIC-W7-005_recovery3.log")
    print("  tail -f logs/phase0/EPIC-W7-022_recovery3.log")
    print("  tail -f logs/phase0/EPIC-W7-039_recovery3.log")
    print("  tail -f logs/phase0/EPIC-W7-055_recovery3.log")
    print("  tail -f logs/phase0/EPIC-W7-073_recovery3.log")
    print()
    print("Check completion:")
    print("  python3 -c \"")
    print("  for n in [5, 22, 39, 55, 73]:")
    print("      epic_id = f'EPIC-W7-{n:03d}'")
    print("      exists = __import__('os').path.exists(f'docs/brain/{epic_id}/00-hotspots.md')")
    print("      print(f'{epic_id}: {exists}')")
    print("  \"")

if __name__ == "__main__":
    main()

# Made with Bob
