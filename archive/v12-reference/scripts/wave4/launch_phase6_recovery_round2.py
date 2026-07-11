#!/usr/bin/env python3
"""Launch Phase 6 recovery for 5 remaining epics that have Phase 5 files but no Phase 6 reports."""

import subprocess
import time
from pathlib import Path

# Epics that need Phase 6 recovery (have Phase 5 files, missing Phase 6 reports)
recovery_epics = [
    "EPIC-CCN-012",
    "EPIC-CCN-027", 
    "EPIC-CCN-045",
    "EPIC-CCN-060",
    "EPIC-CCN-075"
]

print(f"=== PHASE 6 RECOVERY ROUND 2 ===")
print(f"Launching {len(recovery_epics)} epics")
print()

for i, epic_id in enumerate(recovery_epics, 1):
    epic_num = epic_id.split('-')[-1]
    
    print(f"[{i}/{len(recovery_epics)}] Launching {epic_id}")
    
    # Launch in screen session
    cmd = [
        "screen", "-dmS", f"p6-recovery2-{epic_num}",
        "bash", "-l", "-c",
        f"./scripts/wave4/_p6_{epic_num}.sh 2>&1 | tee logs/phase6/{epic_id}.log"
    ]
    
    subprocess.run(cmd, check=True)
    
    # Staggered delay (12 seconds)
    if i < len(recovery_epics):
        print(f"  Waiting 12 seconds before next launch...")
        time.sleep(12)

print()
print(f"=== RECOVERY ROUND 2 COMPLETE ===")
print(f"Launched: {len(recovery_epics)} epics")
print(f"Monitor: screen -ls | grep p6-recovery2")
print(f"Check: ls docs/brain/EPIC-CCN-*/06-*.md | wc -l")

# Made with Bob
