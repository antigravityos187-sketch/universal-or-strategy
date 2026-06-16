#!/usr/bin/env python3
"""
Launch Phase 6 recovery for epics that don't have verification reports yet.
Uses the fixed scripts with flexible prerequisite checks.
"""

import subprocess
import time
from pathlib import Path

# Base directory
base_dir = Path('/home/malhitticrypto/universal-or-strategy')
brain_dir = base_dir / 'docs/brain'
scripts_dir = base_dir / 'scripts/wave4'
logs_dir = base_dir / 'logs/phase6'

# Ensure logs directory exists
logs_dir.mkdir(parents=True, exist_ok=True)

# Find epics needing recovery (no 06-*.md file)
epics_needing_recovery = []

for i in range(1, 81):
    if i == 16:  # Skip EPIC-CCN-016 (deferred)
        continue
    
    epic_num = f"{i:03d}"
    epic_id = f"EPIC-CCN-{epic_num}"
    epic_dir = brain_dir / epic_id
    
    # Check if any 06-*.md file exists
    if not list(epic_dir.glob('06-*.md')):
        epics_needing_recovery.append((epic_num, epic_id))

print(f"Found {len(epics_needing_recovery)} epics needing Phase 6 recovery")
print(f"Already completed: {79 - len(epics_needing_recovery)} epics")

if not epics_needing_recovery:
    print("✅ All epics already have Phase 6 verification reports!")
    exit(0)

# Show first 10 epics needing recovery
print(f"\nFirst 10 epics needing recovery:")
for epic_num, epic_id in epics_needing_recovery[:10]:
    print(f"  - {epic_id}")

# Launch recovery
print(f"\nLaunching Phase 6 recovery for {len(epics_needing_recovery)} epics...")
print(f"Staggered launch: 12s delay between epics")

launched = 0
for epic_num, epic_id in epics_needing_recovery:
    script_path = scripts_dir / f"_p6_{epic_num}.sh"
    log_path = logs_dir / f"{epic_id}.log"
    
    if not script_path.exists():
        print(f"WARNING: Script not found: {script_path}")
        continue
    
    # Launch in screen session
    screen_name = f"p6-recovery-{epic_num}"
    cmd = [
        'screen', '-dmS', screen_name,
        'bash', '-l', '-c',
        f"{script_path} 2>&1 | tee {log_path}"
    ]
    
    subprocess.run(cmd, cwd=base_dir)
    launched += 1
    
    if launched <= 5 or launched % 10 == 0:
        print(f"[{time.strftime('%H:%M:%S')}] Launched: {epic_id} (session: {screen_name})")
    
    # Staggered delay (12s constant)
    if launched < len(epics_needing_recovery):
        time.sleep(12)

print(f"\n✅ Launched {launched} recovery sessions")
print(f"Monitor with: screen -ls | grep 'p6-recovery'")
print(f"Check progress: ls docs/brain/EPIC-CCN-*/06-*.md | wc -l")
print(f"\nExpected final count: 79 verification reports")

# Made with Bob
