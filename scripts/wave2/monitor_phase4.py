#!/usr/bin/env python3
"""
Phase 4 Monitoring Script
Checks agent status and updates manifests based on completion
"""

import json
import subprocess
import time
from pathlib import Path
from datetime import datetime

# Configuration
REPO_ROOT = Path("c:/WSGTA/universal-or-strategy")
BRAIN_DIR = REPO_ROOT / "docs/brain"
VM_NAME = "v12-test-golden-v2"
ZONE = "us-central1-a"
PHASE = "4"

EPICS = ["107", "108", "109", "110", "111", "112", "113", "114", "115"]


def check_screen_sessions() -> list:
    """Check which screen sessions are still running"""
    result = subprocess.run([
        "gcloud", "compute", "ssh", VM_NAME,
        f"--zone={ZONE}",
        "--command=screen -ls | grep phase4-EPIC || echo 'No sessions'"
    ], capture_output=True, text=True)
    
    running = []
    for line in result.stdout.split("\n"):
        if "phase4-EPIC" in line:
            # Extract epic ID from session name
            epic_id = line.split("phase4-EPIC-")[1].split()[0].strip()
            running.append(epic_id)
    
    return running


def check_log_completion(epic_id: str) -> tuple[bool, int]:
    """Check if log shows DONE_EXIT"""
    result = subprocess.run([
        "gcloud", "compute", "ssh", VM_NAME,
        f"--zone={ZONE}",
        f"--command=grep 'DONE_EXIT' /home/malhitticrypto/universal-or-strategy/logs/phase4/EPIC-CCN-{epic_id}.log || echo 'Not found'"
    ], capture_output=True, text=True)
    
    if "DONE_EXIT=" in result.stdout:
        exit_code = int(result.stdout.split("DONE_EXIT=")[1].strip())
        return True, exit_code
    
    return False, -1


def update_manifest_status(epic_id: str, status: str):
    """Update manifest with completion status"""
    manifest_path = BRAIN_DIR / f"EPIC-CCN-{epic_id}" / "manifest.json"
    
    if not manifest_path.exists():
        print(f"[WARNING] Manifest not found for EPIC-CCN-{epic_id}")
        return
    
    manifest = json.loads(manifest_path.read_text())
    manifest["phases"][PHASE]["status"] = status
    manifest["phases"][PHASE]["completed_at"] = datetime.utcnow().isoformat()
    manifest["last_updated"] = datetime.utcnow().isoformat()
    
    manifest_path.write_text(json.dumps(manifest, indent=2))
    print(f"[CHECKPOINT] EPIC-CCN-{epic_id} Phase {PHASE}: {status}")


def main():
    print("[MONITOR] Phase 4 Status Check")
    print("=" * 60)
    
    running_sessions = check_screen_sessions()
    
    print(f"\n[STATUS] Running sessions: {len(running_sessions)}")
    if running_sessions:
        print(f"[RUNNING] {', '.join(running_sessions)}")
    
    # Check each epic
    completed = []
    failed = []
    in_progress = []
    
    for epic_id in EPICS:
        is_complete, exit_code = check_log_completion(epic_id)
        
        if is_complete:
            if exit_code == 0:
                completed.append(epic_id)
                update_manifest_status(epic_id, "completed")
                print(f"[COMPLETE] EPIC-CCN-{epic_id} (exit {exit_code})")
            else:
                failed.append(epic_id)
                update_manifest_status(epic_id, "failed")
                print(f"[FAILED] EPIC-CCN-{epic_id} (exit {exit_code})")
        elif epic_id in running_sessions:
            in_progress.append(epic_id)
            print(f"[RUNNING] EPIC-CCN-{epic_id}")
        else:
            print(f"[UNKNOWN] EPIC-CCN-{epic_id} (no log or session)")
    
    # Summary
    print("\n" + "=" * 60)
    print(f"[SUMMARY] Phase 4 Status:")
    print(f"  Completed: {len(completed)}/9 ({', '.join(completed) if completed else 'none'})")
    print(f"  Failed: {len(failed)}/9 ({', '.join(failed) if failed else 'none'})")
    print(f"  In Progress: {len(in_progress)}/9 ({', '.join(in_progress) if in_progress else 'none'})")
    
    if len(completed) == 9:
        print("\n[SUCCESS] All Phase 4 agents completed!")
        print("[NEXT] Ready to launch Phase 5")
    elif failed:
        print(f"\n[WARNING] {len(failed)} agents failed - review logs before proceeding")
    elif in_progress:
        print(f"\n[INFO] {len(in_progress)} agents still running - check again in 2 minutes")


if __name__ == "__main__":
    main()

# Made with Bob
