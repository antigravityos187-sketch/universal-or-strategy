#!/usr/bin/env python3
"""
V12 Wave 2 v2 - Full Epic-Intake Workflow Execution
Uses v2 epic list (excludes EPIC-CCN-164) and triggers complete workflow

Key Changes from v1:
1. Uses wave2_epics_v2.txt (9 epics, excludes completed EPIC-CCN-164)
2. Prompts trigger full epic-intake workflow (not just analysis)
3. Uses v12-epic-planner mode explicitly
4. Increased MAX_COINS to 200 per epic (full workflow needs more)

Usage:
    python scripts/wave2/launch_wave_v2.py
"""

import subprocess
import sys
import time
from pathlib import Path
from datetime import datetime, timezone

# ── Config ───────────────────────────────────────────────────────────────────
VM_NAME    = "v12-test-golden-v2"
ZONE       = "us-central1-a"
PROJECT    = "project-14c86305-3cba-493f-a73"
RUN_USER   = "malhitticrypto"
REPO       = f"/home/{RUN_USER}/universal-or-strategy"
MAX_COINS  = "200"  # Increased for full workflow (was 50 for analysis only)
# ─────────────────────────────────────────────────────────────────────────────

EPICS_FILE = Path(__file__).parent / "wave2_epics_v2.txt"


def gcloud(*args, check=True) -> subprocess.CompletedProcess:
    # Use full path to gcloud on Windows
    gcloud_exe = r"C:\Program Files (x86)\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd"
    cmd = [gcloud_exe, *args]
    print(f"[CMD] {' '.join(cmd)}")
    return subprocess.run(cmd, check=check, capture_output=False, text=True, shell=True)


def gcloud_capture(*args) -> str:
    gcloud_exe = r"C:\Program Files (x86)\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd"
    cmd = [gcloud_exe, *args]
    result = subprocess.run(cmd, check=False, capture_output=True, text=True, shell=True)
    return result.stdout + result.stderr


def load_epics() -> list[tuple[str, str, int]]:
    """Load epics from file. Returns list of (epic_id, method_name, complexity) tuples."""
    epics = []
    for line in EPICS_FILE.read_text().splitlines():
        line = line.strip()
        if not line or line.startswith("#"):
            continue
        parts = line.split("|")
        if len(parts) == 3:
            epic_id, method, cyc = parts
            epics.append((epic_id.strip(), method.strip(), int(cyc.strip())))
    return epics


def build_wave_script(epics: list[tuple[str, str, int]]) -> str:
    """Generate the wave orchestrator script for FULL epic-intake workflow.
    
    Key changes from v1:
    1. Uses --mode v12-epic-planner explicitly
    2. Prompt triggers full workflow: "Execute complete epic-intake workflow for..."
    3. Increased MAX_COINS to 200 (full workflow needs more than analysis)
    """
    # Expand all paths at Python generation time
    repo_abs = REPO  # /home/malhitticrypto/universal-or-strategy
    logs_abs = f"{repo_abs}/logs"

    lines = [
        "#!/bin/bash",
        "# V12 Wave 2 v2 Orchestrator - Full Epic-Intake Workflow",
        f"# Generated: {datetime.now(timezone.utc).isoformat()}",
        "",
        "# Global git identity for Bob checkpointing",
        f'git config --global user.email "malhitticrypto@gmail.com"',
        f'git config --global user.name "malhitticrypto"',
        "",
        f"mkdir -p {logs_abs}",
        "",
        "# Pull latest repo (best-effort)",
        f"cd {repo_abs} && git pull --ff-only origin main || true",
        "",
        "echo '[WAVE2-V2] Launching parallel Bob Shell agents (FULL WORKFLOW)...'",
    ]

    for epic_id, method, cyc in epics:
        session  = f"v12-{epic_id}"
        log_path = f"{logs_abs}/{epic_id}.log"
        
        # NEW: Prompt triggers FULL epic-intake workflow
        # Uses v12-epic-planner mode and explicit workflow command
        prompt = (
            f"Execute complete epic-intake workflow for {epic_id}: "
            f"Extract {method} (complexity {cyc} -> 8). "
            f"Run all phases: hotspot analysis, scope definition, scope boundary validation, "
            f"architecture planning, DNA audit, and ticket generation."
        )
        
        # Use --mode v12-epic-planner for proper workflow execution
        cmd = (
            f"cd {repo_abs} && "
            f"bob --accept-license --mode v12-epic-planner --max-coins {MAX_COINS} "
            f"-p '{prompt}' "
            f"> {log_path} 2>&1; "
            f"echo DONE_EXIT=$? >> {log_path}"
        )
        lines.append(f'screen -dmS {session} bash -l -c "{cmd}"')
        lines.append(f"echo '[WAVE2-V2] Launched: {epic_id} ({method}, CYC {cyc})'")
        lines.append("sleep 1")

    lines += [
        "",
        "sleep 2",
        f"echo '[WAVE2-V2] All {len(epics)} agents launched (FULL WORKFLOW MODE).'",
        "screen -ls || true",
        "",
        f"echo '[WAVE2-V2] Done. Logs: {logs_abs}'",
        f"echo '[WAVE2-V2] Each agent has {MAX_COINS} bobcoins for complete workflow execution.'",
    ]

    return "\n".join(lines)


def clear_stale_ssh_key():
    """Remove stale Plink key cache entry for the VM's IP."""
    result = gcloud_capture(
        "compute", "instances", "describe", VM_NAME,
        f"--zone={ZONE}", f"--project={PROJECT}",
        "--format=get(networkInterfaces[0].accessConfigs[0].natIP)"
    )
    ip = result.strip().split("\n")[0].strip()
    if not ip:
        print("[WARN] Could not get VM IP for key cache cleanup.")
        return

    print(f"[INFO] VM IP: {ip} — clearing Plink SSH host key cache...")
    import winreg
    try:
        key = winreg.OpenKey(
            winreg.HKEY_CURRENT_USER,
            r"Software\SimonTatham\PuTTY\SshHostKeys",
            0, winreg.KEY_ALL_ACCESS
        )
        vals = []
        try:
            i = 0
            while True:
                name, _, _ = winreg.EnumValue(key, i)
                if ip in name:
                    vals.append(name)
                i += 1
        except OSError:
            pass
        for name in vals:
            winreg.DeleteValue(key, name)
            print(f"[INFO] Removed stale key: {name}")
        winreg.CloseKey(key)
    except Exception as e:
        print(f"[WARN] Could not clear key cache: {e}")


def main():
    epics = load_epics()
    print(f"\n[WAVE2-V2] Preparing FULL WORKFLOW execution for {len(epics)} epics on {VM_NAME}")
    print(f"[WAVE2-V2] Mode: v12-epic-planner (complete epic-intake workflow)")
    print(f"[WAVE2-V2] Budget: {MAX_COINS} bobcoins per epic")
    epic_summary = ', '.join(f"{eid} ({method}, CYC {cyc})" for eid, method, cyc in epics)
    print(f"[WAVE2-V2] Epics: {epic_summary}\n")

    # Build the orchestrator script locally
    script = build_wave_script(epics)
    script_path = Path("scripts/wave2/_wave2_v2_launch_generated.sh")
    # Force LF-only line endings
    with open(script_path, 'w', newline='\n', encoding='utf-8') as f:
        f.write(script)
    print(f"[WAVE2-V2] Generated orchestrator: {script_path} (LF line endings)")
    
    # Verify LF-only
    raw = script_path.read_bytes()
    crlf_count = raw.count(b'\r\n')
    if crlf_count > 0:
        print(f"[ERROR] Script still has {crlf_count} CRLF sequences! Aborting.")
        raise RuntimeError("CRLF detected in generated script")
    print(f"[WAVE2-V2] Line ending check: OK (LF only, {len(script.splitlines())} lines)")

    # Clear stale SSH key cache
    clear_stale_ssh_key()

    # Upload via SCP
    remote_path = f"/tmp/wave2_v2_launch.sh"
    print(f"\n[WAVE2-V2] Uploading script to {VM_NAME}:{remote_path}...")
    gcloud(
        "compute", "scp",
        str(script_path),
        f"{VM_NAME}:{remote_path}",
        f"--zone={ZONE}",
        f"--project={PROJECT}",
    )
    print("[WAVE2-V2] Upload complete.")

    # Execute via SSH
    print(f"\n[WAVE2-V2] Executing on VM...")
    gcloud(
        "compute", "ssh", VM_NAME,
        f"--zone={ZONE}",
        f"--project={PROJECT}",
        f"--command=bash {remote_path}",
    )

    print("\n[WAVE2-V2] [OK] Launch complete!")
    print(f"[WAVE2-V2] This will take MUCH LONGER than v1 (full workflow vs analysis only)")
    print(f"[WAVE2-V2] Expected: 30-60 minutes for all 9 epics to complete")
    print(f"[WAVE2-V2] Monitor: python scripts/wave2/launch_wave.py --monitor {VM_NAME}")
    print(f"[WAVE2-V2] Or SSH:  gcloud compute ssh {VM_NAME} --zone={ZONE} --command='screen -ls'")


if __name__ == "__main__":
    main()

# Made with Bob
