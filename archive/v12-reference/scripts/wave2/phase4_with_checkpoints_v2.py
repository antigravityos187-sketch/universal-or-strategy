#!/usr/bin/env python3
"""
Phase 4 Launch with Self-Healing Checkpoint System (v2)

Fixes:
1. Marks manifests as "in_progress" AFTER successful VM launch (not before)
2. Auto-resets stalled "in_progress" to "pending" after 60 minutes
3. Better error handling with graceful fallback
4. Idempotent retries
"""

import json
import subprocess
import time
from pathlib import Path
from datetime import datetime, timedelta

# Configuration
REPO_ROOT = Path("c:/WSGTA/universal-or-strategy")
BRAIN_DIR = REPO_ROOT / "docs/brain"
API_DIR = REPO_ROOT / "docs/API"
VM_NAME = "v12-test-golden-v2"
ZONE = "us-central1-a"
PHASE = "4"
MAX_COINS = "5"
STALL_TIMEOUT_MINUTES = 60  # Auto-reset if in_progress for >60 min

# Epic list (from Wave 2 v4)
EPICS = [
    {"id": "107", "method": "ProcessIpcCommands", "cyc": 76},
    {"id": "108", "method": "ProcessOnExecutionUpdate", "cyc": 67},
    {"id": "109", "method": "HydrateFSMsFromWorkingOrders", "cyc": 45},
    {"id": "110", "method": "HandleFlatPositionUpdate", "cyc": 37},
    {"id": "111", "method": "AdoptFleetOrders", "cyc": 37},
    {"id": "112", "method": "ExtractTargetConfiguration", "cyc": 31},
    {"id": "113", "method": "SweepBrokerOrders", "cyc": 28},
    {"id": "114", "method": "FlattenSinglePosition", "cyc": 27},
    {"id": "115", "method": "ExecuteRetestEntry", "cyc": 26},
]

# API allocation (same as Wave 2 v4)
API_ALLOCATION = {
    "107": "b (2).json",
    "108": "b.json",
    "109": "bob (1).json",
    "110": "bob (2).json",
    "111": "bob (3).json",
    "112": "bob (4).json",
    "113": "bob (5).json",
    "114": "b.json",
    "115": "bob.json",
}


def load_manifest(epic_id: str) -> dict:
    """Load manifest.json for an epic, create if doesn't exist"""
    manifest_path = BRAIN_DIR / f"EPIC-CCN-{epic_id}" / "manifest.json"
    
    if manifest_path.exists():
        return json.loads(manifest_path.read_text())
    
    # Create new manifest
    manifest = {
        "epic_id": f"EPIC-CCN-{epic_id}",
        "created_at": datetime.utcnow().isoformat(),
        "phases": {
            "0": {"status": "completed", "output": "00-hotspots.md"},
            "1": {"status": "completed", "output": "00-scope.md"},
            "1.5": {"status": "completed", "output": "01-scope-boundary.md"},
            "2": {"status": "completed", "output": "02-architecture-plan.md"},
            "3": {"status": "completed", "output": "03-audit-report.md"},
            "4": {"status": "pending", "output": "04-tickets.md"},
            "5": {"status": "pending", "output": "ticket-completion.md"},
            "6": {"status": "pending", "output": "05-completion-report.md"},
        },
        "current_phase": "4",
        "last_updated": datetime.utcnow().isoformat(),
    }
    
    # Create directory if needed
    manifest_path.parent.mkdir(parents=True, exist_ok=True)
    manifest_path.write_text(json.dumps(manifest, indent=2))
    
    return manifest


def save_manifest(epic_id: str, manifest: dict):
    """Save manifest to disk"""
    manifest_path = BRAIN_DIR / f"EPIC-CCN-{epic_id}" / "manifest.json"
    manifest["last_updated"] = datetime.utcnow().isoformat()
    manifest_path.write_text(json.dumps(manifest, indent=2))


def update_manifest(epic_id: str, phase: str, status: str, output: str | None = None):
    """Update manifest with phase status"""
    manifest = load_manifest(epic_id)
    
    manifest["phases"][phase]["status"] = status
    if output:
        manifest["phases"][phase]["output"] = output
    
    save_manifest(epic_id, manifest)
    print(f"[CHECKPOINT] EPIC-CCN-{epic_id} Phase {phase}: {status}")


def check_phase_status_with_healing(epic_id: str, phase: str) -> str:
    """
    Check phase status with self-healing for stalled agents.
    Auto-resets "in_progress" to "pending" if stalled >60 minutes.
    """
    manifest = load_manifest(epic_id)
    
    # Handle missing phase entry
    if phase not in manifest["phases"]:
        manifest["phases"][phase] = {
            "status": "pending",
            "output": f"0{phase}-tickets.md" if phase == "4" else f"phase-{phase}.md"
        }
        save_manifest(epic_id, manifest)
        print(f"[FIX] Added Phase {phase} entry to EPIC-CCN-{epic_id} manifest")
        return "pending"
    
    status = manifest["phases"][phase]["status"]
    
    # Self-healing: Reset stalled "in_progress" to "pending"
    if status == "in_progress":
        last_updated_str = manifest.get("last_updated", "2000-01-01T00:00:00Z")
        # Handle both with and without 'Z' suffix
        if last_updated_str.endswith('Z'):
            last_updated_str = last_updated_str[:-1]
        
        try:
            last_updated = datetime.fromisoformat(last_updated_str)
            elapsed_minutes = (datetime.utcnow() - last_updated).total_seconds() / 60
            
            if elapsed_minutes > STALL_TIMEOUT_MINUTES:
                print(f"[HEAL] EPIC-CCN-{epic_id} stalled for {elapsed_minutes:.0f} min, resetting to pending")
                manifest["phases"][phase]["status"] = "pending"
                save_manifest(epic_id, manifest)
                return "pending"
        except ValueError as e:
            print(f"[WARN] Could not parse timestamp for EPIC-CCN-{epic_id}: {e}")
    
    return status


def load_api_key(filename: str) -> str:
    """Load API key from JSON file"""
    api_file = API_DIR / filename
    data = json.loads(api_file.read_text())
    return data["apikey"]


def build_phase4_script(epics_to_run: list) -> str:
    """Build bash script for Phase 4 execution"""
    script_lines = [
        "#!/bin/bash",
        "# Phase 4: Ticket Generation with Self-Healing Checkpoints (v2)",
        "set -e",
        "",
        "REPO=/home/malhitticrypto/universal-or-strategy",
        "cd $REPO",
        "",
        "# Create log directory",
        "mkdir -p $REPO/logs/phase4",
        "",
    ]
    
    for epic in epics_to_run:
        epic_id = epic["id"]
        api_file = API_ALLOCATION[epic_id]
        api_key = load_api_key(api_file)
        
        prompt = (
            f"Execute Phase 4 (Ticket Generation) for EPIC-CCN-{epic_id}. "
            f"Read docs/brain/EPIC-CCN-{epic_id}/02-architecture-plan.md and generate "
            f"implementation tickets in docs/brain/EPIC-CCN-{epic_id}/04-tickets.md. "
            f"Update manifest.json with status=completed when done."
        )
        
        log_path = f"$REPO/logs/phase4/EPIC-CCN-{epic_id}.log"
        
        script_lines.extend([
            f"# EPIC-CCN-{epic_id}",
            f"echo '[PHASE4] Starting EPIC-CCN-{epic_id}...'",
            f"screen -dmS phase4-EPIC-{epic_id} bash -c \"",
            f"  export BOBSHELL_API_KEY='{api_key}' && \\",
            f"  cd $REPO && \\",
            f"  bob --accept-license --chat-mode plan --max-coins {MAX_COINS} \\",
            f"    -p '{prompt}' \\",
            f"    > {log_path} 2>&1; \\",
            f"  echo DONE_EXIT=\\$? >> {log_path}",
            f"\"",
            "",
        ])
    
    script_lines.extend([
        "echo '[PHASE4] All agents launched'",
        "screen -ls",
    ])
    
    return "\n".join(script_lines)


def launch_agents_on_vm(epics_to_run: list) -> bool:
    """
    Launch agents on VM, return True if successful.
    Does NOT mark manifests as in_progress - caller does that after success.
    """
    if not epics_to_run:
        return True
    
    # Build execution script
    print(f"\n[BUILD] Creating Phase 4 script for {len(epics_to_run)} epics...")
    script_content = build_phase4_script(epics_to_run)
    
    script_path = Path("/tmp/wave2_phase4_v2.sh")
    script_path.write_text(script_content, newline="\n")
    print(f"[BUILD] Script created: {script_path}")
    
    # Use full path to gcloud
    gcloud_exe = r"C:\Program Files (x86)\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd"
    
    try:
        # Upload to VM
        print("\n[UPLOAD] Uploading script to VM...")
        subprocess.run([
            gcloud_exe, "compute", "scp",
            str(script_path),
            f"{VM_NAME}:/tmp/wave2_phase4_v2.sh",
            f"--zone={ZONE}"
        ], check=True, capture_output=True, text=True, shell=True)
        
        # Execute on VM
        print("\n[EXECUTE] Launching Phase 4 agents on VM...")
        result = subprocess.run([
            gcloud_exe, "compute", "ssh", VM_NAME,
            f"--zone={ZONE}",
            "--command=bash /tmp/wave2_phase4_v2.sh"
        ], check=True, capture_output=True, text=True, shell=True)
        
        print(result.stdout)
        return True
        
    except subprocess.CalledProcessError as e:
        print(f"\n[ERROR] Failed to launch agents on VM:")
        print(f"  Command: {' '.join(e.cmd)}")
        print(f"  Exit code: {e.returncode}")
        if e.stdout:
            print(f"  Stdout: {e.stdout}")
        if e.stderr:
            print(f"  Stderr: {e.stderr}")
        print("\n[KEEP] Manifests remain 'pending' for retry")
        return False
    except FileNotFoundError:
        print("\n[ERROR] gcloud command not found")
        print("[HINT] Install gcloud CLI or run from environment with gcloud")
        print("[KEEP] Manifests remain 'pending' for retry")
        return False


def main():
    print("[PHASE4] Starting Phase 4 with Self-Healing Checkpoint System (v2)")
    print("=" * 60)
    
    # Step 1: Check which epics need Phase 4 (with self-healing)
    epics_to_run = []
    epics_completed = []
    epics_in_progress = []
    
    for epic in EPICS:
        epic_id = epic["id"]
        status = check_phase_status_with_healing(epic_id, PHASE)
        
        if status == "pending":
            epics_to_run.append(epic)
        elif status == "in_progress":
            epics_in_progress.append(epic)
            print(f"[ACTIVE] EPIC-CCN-{epic_id} Phase 4 currently running")
        elif status == "completed":
            epics_completed.append(epic)
            print(f"[SKIP] EPIC-CCN-{epic_id} Phase 4 already completed")
    
    print(f"\n[STATUS] Epics to run: {len(epics_to_run)}")
    print(f"[STATUS] Epics in progress: {len(epics_in_progress)}")
    print(f"[STATUS] Epics completed: {len(epics_completed)}")
    
    if not epics_to_run:
        print("\n[COMPLETE] All epics already completed or in progress!")
        return
    
    # Step 2: Launch agents on VM
    success = launch_agents_on_vm(epics_to_run)
    
    if not success:
        print("\n[FAILED] Could not launch agents on VM")
        print("[RETRY] Manifests remain 'pending', safe to retry")
        return
    
    # Step 3: ONLY NOW mark as in_progress (after successful launch)
    print("\n[MARK] Marking epics as in_progress...")
    for epic in epics_to_run:
        update_manifest(epic["id"], PHASE, "in_progress")
    
    print("\n[SUCCESS] Phase 4 launched!")
    print(f"[INFO] {len(epics_to_run)} agents running on VM")
    print(f"[INFO] Logs: /home/malhitticrypto/universal-or-strategy/logs/phase4/")
    print(f"[INFO] Manifests: docs/brain/EPIC-CCN-*/manifest.json")
    
    print("\n[NEXT] Monitor with:")
    print(f"  gcloud compute ssh {VM_NAME} --zone={ZONE} --command='screen -ls'")
    print(f"  python scripts/wave2/check_phase4_local.py")


if __name__ == "__main__":
    main()

# Made with Bob - Self-Healing Edition